using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomStateMachineDemo
{
    internal static class Program
    {
        private static void Main()
        {
            var order = new OrderContext();
            var machine = BuildStateMachine(order);

            WriteHeadline("手写状态机 Demo");
            PrintState(machine);

            Attempt(machine, OrderTrigger.Submit, "草稿状态下直接提交");

            order.HasLineItems = true;
            Console.WriteLine("已补充订单明细，允许再次提交。");
            Attempt(machine, OrderTrigger.Submit, "有明细后提交");
            Attempt(machine, OrderTrigger.Approve, "审核通过");
            Attempt(machine, OrderTrigger.Pay, "完成支付");
            Attempt(machine, OrderTrigger.Ship, "发货");
            Attempt(machine, OrderTrigger.Cancel, "发货后尝试取消");

            Console.WriteLine();
            Console.WriteLine("演示结束。");
        }

        private static SimpleStateMachine<OrderState, OrderTrigger> BuildStateMachine(OrderContext order)
        {
            var machine = new SimpleStateMachine<OrderState, OrderTrigger>(OrderState.Draft);

            machine.Configure(OrderState.Draft)
                .OnEntry((from, to, trigger) => Console.WriteLine("进入草稿状态。"))
                .Permit(OrderTrigger.Submit, OrderState.Submitted, () => order.HasLineItems, "订单至少要有一条明细")
                .Permit(OrderTrigger.Cancel, OrderState.Cancelled);

            machine.Configure(OrderState.Submitted)
                .OnEntry((from, to, trigger) => Console.WriteLine("订单已提交，等待审核。"))
                .Permit(OrderTrigger.Approve, OrderState.Approved)
                .Permit(OrderTrigger.Reject, OrderState.Rejected)
                .Permit(OrderTrigger.Cancel, OrderState.Cancelled);

            machine.Configure(OrderState.Approved)
                .OnEntry((from, to, trigger) => Console.WriteLine("审核通过，可以支付。"))
                .Permit(OrderTrigger.Pay, OrderState.Paid);

            machine.Configure(OrderState.Rejected)
                .OnEntry((from, to, trigger) => Console.WriteLine("审核被拒，允许修改后重新提交。"))
                .Permit(OrderTrigger.Reopen, OrderState.Draft);

            machine.Configure(OrderState.Paid)
                .OnEntry((from, to, trigger) => Console.WriteLine("支付完成，等待仓库发货。"))
                .Permit(OrderTrigger.Ship, OrderState.Shipped);

            machine.Configure(OrderState.Shipped)
                .OnEntry((from, to, trigger) => Console.WriteLine("订单已发货，流程完成。"));

            machine.Configure(OrderState.Cancelled)
                .OnEntry((from, to, trigger) => Console.WriteLine("订单已取消。"));

            return machine;
        }

        private static void Attempt(SimpleStateMachine<OrderState, OrderTrigger> machine, OrderTrigger trigger, string title)
        {
            Console.WriteLine();
            Console.WriteLine("[" + title + "]");

            if (!machine.CanFire(trigger, out var reason))
            {
                Console.WriteLine("触发失败: " + reason);
                PrintState(machine);
                return;
            }

            machine.Fire(trigger);
            PrintState(machine);
        }

        private static void PrintState(SimpleStateMachine<OrderState, OrderTrigger> machine)
        {
            Console.WriteLine("当前状态: " + machine.State);
            Console.WriteLine("可触发事件: " + string.Join(", ", machine.GetPermittedTriggers()));
        }

        private static void WriteHeadline(string text)
        {
            Console.WriteLine("=".PadRight(40, '='));
            Console.WriteLine(text);
            Console.WriteLine("=".PadRight(40, '='));
        }
    }

    internal enum OrderState
    {
        Draft,
        Submitted,
        Approved,
        Rejected,
        Paid,
        Shipped,
        Cancelled
    }

    internal enum OrderTrigger
    {
        Submit,
        Approve,
        Reject,
        Reopen,
        Pay,
        Ship,
        Cancel
    }

    internal sealed class OrderContext
    {
        public bool HasLineItems { get; set; }
    }

    internal sealed class SimpleStateMachine<TState, TTrigger>
    {
        private readonly Dictionary<TState, StateConfiguration> _configurations;

        public SimpleStateMachine(TState initialState)
        {
            State = initialState;
            _configurations = new Dictionary<TState, StateConfiguration>();
        }

        public TState State { get; private set; }

        public StateConfiguration Configure(TState state)
        {
            if (!_configurations.TryGetValue(state, out var configuration))
            {
                configuration = new StateConfiguration(state);
                _configurations[state] = configuration;
            }

            return configuration;
        }

        public bool CanFire(TTrigger trigger, out string reason)
        {
            if (!TryFindTransition(State, trigger, out var transition, out reason))
            {
                return false;
            }

            if (!transition.IsAllowed())
            {
                reason = "当前状态不满足条件: " + transition.Description;
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public void Fire(TTrigger trigger)
        {
            if (!TryFindTransition(State, trigger, out var transition, out var reason))
            {
                throw new InvalidOperationException(reason);
            }

            if (!transition.IsAllowed())
            {
                throw new InvalidOperationException("当前状态不满足条件: " + transition.Description);
            }

            var source = State;
            var destination = transition.DestinationState;

            GetConfiguration(source).RunExit(source, destination, trigger);
            State = destination;
            GetConfiguration(destination).RunEntry(source, destination, trigger);
        }

        public IReadOnlyList<TTrigger> GetPermittedTriggers()
        {
            if (!_configurations.TryGetValue(State, out var configuration))
            {
                return Array.Empty<TTrigger>();
            }

            return configuration.Transitions
                .Where(t => t.IsAllowed())
                .Select(t => t.Trigger)
                .ToArray();
        }

        private bool TryFindTransition(TState state, TTrigger trigger, out Transition transition, out string reason)
        {
            if (!_configurations.TryGetValue(state, out var configuration))
            {
                transition = null;
                reason = "状态 " + state + " 尚未配置。";
                return false;
            }

            transition = configuration.Transitions.FirstOrDefault(t => EqualityComparer<TTrigger>.Default.Equals(t.Trigger, trigger));

            if (transition == null)
            {
                reason = "状态 " + state + " 不支持事件 " + trigger + "。";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private StateConfiguration GetConfiguration(TState state)
        {
            if (!_configurations.TryGetValue(state, out var configuration))
            {
                throw new InvalidOperationException("状态 " + state + " 尚未配置。");
            }

            return configuration;
        }

        internal sealed class StateConfiguration
        {
            private readonly List<Action<TState, TState, TTrigger>> _entryActions;
            private readonly List<Action<TState, TState, TTrigger>> _exitActions;

            public StateConfiguration(TState state)
            {
                State = state;
                Transitions = new List<Transition>();
                _entryActions = new List<Action<TState, TState, TTrigger>>();
                _exitActions = new List<Action<TState, TState, TTrigger>>();
            }

            public TState State { get; }

            public List<Transition> Transitions { get; }

            public StateConfiguration Permit(TTrigger trigger, TState destinationState, Func<bool> guard = null, string description = null)
            {
                Transitions.Add(new Transition(trigger, destinationState, guard, description));
                return this;
            }

            public StateConfiguration OnEntry(Action<TState, TState, TTrigger> action)
            {
                _entryActions.Add(action);
                return this;
            }

            public StateConfiguration OnExit(Action<TState, TState, TTrigger> action)
            {
                _exitActions.Add(action);
                return this;
            }

            public void RunEntry(TState source, TState destination, TTrigger trigger)
            {
                foreach (var action in _entryActions)
                {
                    action(source, destination, trigger);
                }
            }

            public void RunExit(TState source, TState destination, TTrigger trigger)
            {
                foreach (var action in _exitActions)
                {
                    action(source, destination, trigger);
                }
            }
        }

        internal sealed class Transition
        {
            private readonly Func<bool> _guard;

            public Transition(TTrigger trigger, TState destinationState, Func<bool> guard, string description)
            {
                Trigger = trigger;
                DestinationState = destinationState;
                _guard = guard ?? (() => true);
                Description = string.IsNullOrWhiteSpace(description) ? "未提供条件说明" : description;
            }

            public TTrigger Trigger { get; }

            public TState DestinationState { get; }

            public string Description { get; }

            public bool IsAllowed()
            {
                return _guard();
            }
        }
    }
}
