using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomStateMachineDemo
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序入口点
        /// </summary>
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

        /// <summary>
        /// 构建订单状态机，定义所有状态之间的转换规则
        /// </summary>
        /// <param name="order">订单上下文，包含订单的附加信息（如是否有明细）</param>
        /// <returns>配置好的状态机实例</returns>
        private static SimpleStateMachine<OrderState, OrderTrigger> BuildStateMachine(OrderContext order)
        {
            var machine = new SimpleStateMachine<OrderState, OrderTrigger>(OrderState.Draft);

            // 配置草稿状态
            machine.Configure(OrderState.Draft)
                .OnEntry((from, to, trigger) => Console.WriteLine("进入草稿状态。"))
                .Permit(OrderTrigger.Submit, OrderState.Submitted, () => order.HasLineItems, "订单至少要有一条明细")
                .Permit(OrderTrigger.Cancel, OrderState.Cancelled);

            // 配置已提交状态
            machine.Configure(OrderState.Submitted)
                .OnEntry((from, to, trigger) => Console.WriteLine("订单已提交，等待审核。"))
                .Permit(OrderTrigger.Approve, OrderState.Approved)
                .Permit(OrderTrigger.Reject, OrderState.Rejected)
                .Permit(OrderTrigger.Cancel, OrderState.Cancelled);

            // 配置已审核状态
            machine.Configure(OrderState.Approved)
                .OnEntry((from, to, trigger) => Console.WriteLine("审核通过，可以支付。"))
                .Permit(OrderTrigger.Pay, OrderState.Paid);

            // 配置已驳回状态
            machine.Configure(OrderState.Rejected)
                .OnEntry((from, to, trigger) => Console.WriteLine("审核被拒，允许修改后重新提交。"))
                .Permit(OrderTrigger.Reopen, OrderState.Draft);

            // 配置已支付状态
            machine.Configure(OrderState.Paid)
                .OnEntry((from, to, trigger) => Console.WriteLine("支付完成，等待仓库发货。"))
                .Permit(OrderTrigger.Ship, OrderState.Shipped);

            // 配置已发货状态（终态，无出边转换）
            machine.Configure(OrderState.Shipped)
                .OnEntry((from, to, trigger) => Console.WriteLine("订单已发货，流程完成。"));

            // 配置已取消状态（终态，无出边转换）
            machine.Configure(OrderState.Cancelled)
                .OnEntry((from, to, trigger) => Console.WriteLine("订单已取消。"));

            return machine;
        }

        /// <summary>
        /// 尝试触发状态机的一个事件
        /// </summary>
        /// <param name="machine">状态机实例</param>
        /// <param name="trigger">要触发的事件</param>
        /// <param name="title">本次尝试的标题描述</param>
        private static void Attempt(SimpleStateMachine<OrderState, OrderTrigger> machine, OrderTrigger trigger, string title)
        {
            Console.WriteLine();
            Console.WriteLine("[" + title + "]");

            // 检查是否可以触发该事件
            if (!machine.CanFire(trigger, out var reason))
            {
                Console.WriteLine("触发失败: " + reason);
                PrintState(machine);
                return;
            }

            // 执行事件转换
            machine.Fire(trigger);
            PrintState(machine);
        }

        /// <summary>
        /// 打印状态机的当前状态和可触发的事件列表
        /// </summary>
        /// <param name="machine">状态机实例</param>
        private static void PrintState(SimpleStateMachine<OrderState, OrderTrigger> machine)
        {
            Console.WriteLine("当前状态: " + machine.State);
            Console.WriteLine("可触发事件: " + string.Join(", ", machine.GetPermittedTriggers()));
        }

        /// <summary>
        /// 打印标题头（装饰性方法，用于控制台输出美化）
        /// </summary>
        /// <param name="text">要显示的标题文本</param>
        private static void WriteHeadline(string text)
        {
            Console.WriteLine("=".PadRight(40, '='));
            Console.WriteLine(text);
            Console.WriteLine("=".PadRight(40, '='));
        }
    }

    /// <summary>
    /// 订单状态枚举
    /// </summary>
    internal enum OrderState
    {
        Draft,      // 草稿
        Submitted,  // 已提交
        Approved,   // 已审核
        Rejected,   // 已驳回
        Paid,       // 已支付
        Shipped,    // 已发货
        Cancelled   // 已取消
    }

    /// <summary>
    /// 订单触发事件枚举
    /// </summary>
    internal enum OrderTrigger
    {
        Submit,     // 提交
        Approve,    // 审核通过
        Reject,     // 审核驳回
        Reopen,     // 重新打开（从驳回状态回到草稿）
        Pay,        // 支付
        Ship,       // 发货
        Cancel      // 取消
    }

    /// <summary>
    /// 订单上下文，存储订单相关的业务数据
    /// </summary>
    internal sealed class OrderContext
    {
        /// <summary>
        /// 订单是否有明细项
        /// </summary>
        public bool HasLineItems { get; set; }
    }

    /// <summary>
    /// 通用状态机实现
    /// </summary>
    /// <typeparam name="TState">状态类型</typeparam>
    /// <typeparam name="TTrigger">触发事件类型</typeparam>
    internal sealed class SimpleStateMachine<TState, TTrigger>
    {
        /// <summary>
        /// 存储每个状态的配置信息
        /// </summary>
        private readonly Dictionary<TState, StateConfiguration> _configurations;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="initialState">初始状态</param>
        public SimpleStateMachine(TState initialState)
        {
            State = initialState;
            _configurations = new Dictionary<TState, StateConfiguration>();
        }

        /// <summary>
        /// 当前状态
        /// </summary>
        public TState State { get; private set; }

        /// <summary>
        /// 获取或创建指定状态的配置对象
        /// </summary>
        /// <param name="state">要配置的状态</param>
        /// <returns>状态配置对象</returns>
        public StateConfiguration Configure(TState state)
        {
            if (!_configurations.TryGetValue(state, out var configuration))
            {
                configuration = new StateConfiguration(state);
                _configurations[state] = configuration;
            }

            return configuration;
        }

        /// <summary>
        /// 检查是否可以触发指定事件
        /// </summary>
        /// <param name="trigger">要触发的事件</param>
        /// <param name="reason">如果无法触发，返回原因</param>
        /// <returns>是否可以触发</returns>
        public bool CanFire(TTrigger trigger, out string reason)
        {
            // 尝试查找当前状态下对应事件的转换
            if (!TryFindTransition(State, trigger, out var transition, out reason))
            {
                return false;
            }

            // 检查守卫条件是否满足
            if (!transition.IsAllowed())
            {
                reason = "当前状态不满足条件: " + transition.Description;
                return false;
            }

            reason = string.Empty;
            return true;
        }

        /// <summary>
        /// 触发事件，执行状态转换
        /// </summary>
        /// <param name="trigger">要触发的事件</param>
        /// <exception cref="InvalidOperationException">当转换无效或条件不满足时抛出</exception>
        public void Fire(TTrigger trigger)
        {
            // 查找转换
            if (!TryFindTransition(State, trigger, out var transition, out var reason))
            {
                throw new InvalidOperationException(reason);
            }

            // 检查守卫条件
            if (!transition.IsAllowed())
            {
                throw new InvalidOperationException("当前状态不满足条件: " + transition.Description);
            }

            var source = State;
            var destination = transition.DestinationState;

            // 执行源状态的退出动作
            GetConfiguration(source).RunExit(source, destination, trigger);
            // 更新状态
            State = destination;
            // 执行目标状态的进入动作
            GetConfiguration(destination).RunEntry(source, destination, trigger);
        }

        /// <summary>
        /// 获取当前状态下所有允许触发的事件列表
        /// </summary>
        /// <returns>允许触发的事件列表</returns>
        public IReadOnlyList<TTrigger> GetPermittedTriggers()
        {
            if (!_configurations.TryGetValue(State, out var configuration))
            {
                return Array.Empty<TTrigger>();
            }

            // 返回所有守卫条件满足的转换对应的事件
            return configuration.Transitions
                .Where(t => t.IsAllowed())
                .Select(t => t.Trigger)
                .ToArray();
        }

        /// <summary>
        /// 尝试查找从指定状态出发、由指定事件触发的转换
        /// </summary>
        /// <param name="state">源状态</param>
        /// <param name="trigger">触发事件</param>
        /// <param name="transition">找到的转换对象</param>
        /// <param name="reason">如果未找到，返回原因</param>
        /// <returns>是否找到转换</returns>
        private bool TryFindTransition(TState state, TTrigger trigger, out Transition transition, out string reason)
        {
            // 检查源状态是否已配置
            if (!_configurations.TryGetValue(state, out var configuration))
            {
                transition = null;
                reason = "状态 " + state + " 尚未配置。";
                return false;
            }

            // 查找对应事件的转换
            transition = configuration.Transitions.FirstOrDefault(t => EqualityComparer<TTrigger>.Default.Equals(t.Trigger, trigger));

            if (transition == null)
            {
                reason = "状态 " + state + " 不支持事件 " + trigger + "。";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        /// <summary>
        /// 获取指定状态的配置对象
        /// </summary>
        /// <param name="state">状态</param>
        /// <returns>状态配置对象</returns>
        /// <exception cref="InvalidOperationException">当状态未配置时抛出</exception>
        private StateConfiguration GetConfiguration(TState state)
        {
            if (!_configurations.TryGetValue(state, out var configuration))
            {
                throw new InvalidOperationException("状态 " + state + " 尚未配置。");
            }

            return configuration;
        }

        /// <summary>
        /// 状态配置类，用于配置特定状态的行为和转换
        /// </summary>
        internal sealed class StateConfiguration
        {
            /// <summary>
            /// 进入动作列表
            /// </summary>
            private readonly List<Action<TState, TState, TTrigger>> _entryActions;

            /// <summary>
            /// 退出动作列表
            /// </summary>
            private readonly List<Action<TState, TState, TTrigger>> _exitActions;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="state">要配置的状态</param>
            public StateConfiguration(TState state)
            {
                State = state;
                Transitions = new List<Transition>();
                _entryActions = new List<Action<TState, TState, TTrigger>>();
                _exitActions = new List<Action<TState, TState, TTrigger>>();
            }

            /// <summary>
            /// 当前配置的状态
            /// </summary>
            public TState State { get; }

            /// <summary>
            /// 从当前状态出发的转换列表
            /// </summary>
            public List<Transition> Transitions { get; }

            /// <summary>
            /// 添加一个允许的状态转换
            /// </summary>
            /// <param name="trigger">触发事件</param>
            /// <param name="destinationState">目标状态</param>
            /// <param name="guard">守卫条件（可选），为 null 时表示无条件允许</param>
            /// <param name="description">条件描述</param>
            /// <returns>当前配置对象，支持链式调用</returns>
            public StateConfiguration Permit(TTrigger trigger, TState destinationState, Func<bool> guard = null, string description = null)
            {
                Transitions.Add(new Transition(trigger, destinationState, guard, description));
                return this;
            }

            /// <summary>
            /// 添加进入状态时执行的动作
            /// </summary>
            /// <param name="action">动作委托，参数为（源状态，目标状态，触发事件）</param>
            /// <returns>当前配置对象，支持链式调用</returns>
            public StateConfiguration OnEntry(Action<TState, TState, TTrigger> action)
            {
                _entryActions.Add(action);
                return this;
            }

            /// <summary>
            /// 添加退出状态时执行的动作
            /// </summary>
            /// <param name="action">动作委托，参数为（源状态，目标状态，触发事件）</param>
            /// <returns>当前配置对象，支持链式调用</returns>
            public StateConfiguration OnExit(Action<TState, TState, TTrigger> action)
            {
                _exitActions.Add(action);
                return this;
            }

            /// <summary>
            /// 执行所有进入动作
            /// </summary>
            public void RunEntry(TState source, TState destination, TTrigger trigger)
            {
                foreach (var action in _entryActions)
                {
                    action(source, destination, trigger);
                }
            }

            /// <summary>
            /// 执行所有退出动作
            /// </summary>
            public void RunExit(TState source, TState destination, TTrigger trigger)
            {
                foreach (var action in _exitActions)
                {
                    action(source, destination, trigger);
                }
            }
        }

        /// <summary>
        /// 状态转换类，描述从一个状态到另一个状态的转换
        /// </summary>
        internal sealed class Transition
        {
            /// <summary>
            /// 守卫条件委托
            /// </summary>
            private readonly Func<bool> _guard;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="trigger">触发此转换的事件</param>
            /// <param name="destinationState">目标状态</param>
            /// <param name="guard">守卫条件（可选），为 null 时表示始终允许</param>
            /// <param name="description">条件描述</param>
            public Transition(TTrigger trigger, TState destinationState, Func<bool> guard, string description)
            {
                Trigger = trigger;
                DestinationState = destinationState;
                _guard = guard ?? (() => true);
                Description = string.IsNullOrWhiteSpace(description) ? "未提供条件说明" : description;
            }

            /// <summary>
            /// 触发此转换的事件
            /// </summary>
            public TTrigger Trigger { get; }

            /// <summary>
            /// 目标状态
            /// </summary>
            public TState DestinationState { get; }

            /// <summary>
            /// 转换的条件说明
            /// </summary>
            public string Description { get; }

            /// <summary>
            /// 判断此转换是否允许（守卫条件是否满足）
            /// </summary>
            /// <returns>是否允许转换</returns>
            public bool IsAllowed()
            {
                return _guard();
            }
        }
    }
}