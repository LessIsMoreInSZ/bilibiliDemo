using System;
using System.Linq;
using Stateless;

namespace StatelessStateMachineDemo
{
    internal static class Program
    {
        private static void Main()
        {
            var order = new OrderContext();
            var machine = BuildStateMachine(order);

            WriteHeadline("Stateless Demo");
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

        private static StateMachine<OrderState, OrderTrigger> BuildStateMachine(OrderContext order)
        {
            var machine = new StateMachine<OrderState, OrderTrigger>(OrderState.Draft);

            machine.OnTransitioned(transition =>
                Console.WriteLine("状态迁移: {0} --{1}--> {2}", transition.Source, transition.Trigger, transition.Destination));

            machine.Configure(OrderState.Draft)
                .OnEntry(() => Console.WriteLine("进入草稿状态。"))
                .PermitIf(OrderTrigger.Submit, OrderState.Submitted, () => order.HasLineItems)
                .Permit(OrderTrigger.Cancel, OrderState.Cancelled);

            machine.Configure(OrderState.Submitted)
                .OnEntry(() => Console.WriteLine("订单已提交，等待审核。"))
                .Permit(OrderTrigger.Approve, OrderState.Approved)
                .Permit(OrderTrigger.Reject, OrderState.Rejected)
                .Permit(OrderTrigger.Cancel, OrderState.Cancelled);

            machine.Configure(OrderState.Approved)
                .OnEntry(() => Console.WriteLine("审核通过，可以支付。"))
                .Permit(OrderTrigger.Pay, OrderState.Paid);

            machine.Configure(OrderState.Rejected)
                .OnEntry(() => Console.WriteLine("审核被拒，允许修改后重新提交。"))
                .Permit(OrderTrigger.Reopen, OrderState.Draft);

            machine.Configure(OrderState.Paid)
                .OnEntry(() => Console.WriteLine("支付完成，等待仓库发货。"))
                .Permit(OrderTrigger.Ship, OrderState.Shipped);

            machine.Configure(OrderState.Shipped)
                .OnEntry(() => Console.WriteLine("订单已发货，流程完成。"));

            machine.Configure(OrderState.Cancelled)
                .OnEntry(() => Console.WriteLine("订单已取消。"));

            return machine;
        }

        private static void Attempt(StateMachine<OrderState, OrderTrigger> machine, OrderTrigger trigger, string title)
        {
            Console.WriteLine();
            Console.WriteLine("[" + title + "]");

            if (!machine.CanFire(trigger))
            {
                Console.WriteLine("触发失败: 状态 {0} 不允许事件 {1}。", machine.State, trigger);
                if (trigger == OrderTrigger.Submit)
                {
                    Console.WriteLine("补充说明: Draft -> Submit 额外要求订单至少有一条明细。");
                }

                PrintState(machine);
                return;
            }

            machine.Fire(trigger);
            PrintState(machine);
        }

        private static void PrintState(StateMachine<OrderState, OrderTrigger> machine)
        {
            var triggers = machine.GetPermittedTriggersAsync().GetAwaiter().GetResult();
            Console.WriteLine("当前状态: " + machine.State);
            Console.WriteLine("可触发事件: " + string.Join(", ", triggers.Select(x => x.ToString())));
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
}
