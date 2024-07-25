using System;
using System.Reflection.Metadata;
using Quartz;
using Quartz.Impl;

namespace Quartz
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 创建 StdSchedulerFactory 实例
            var sf = new StdSchedulerFactory();

            // 从工厂获取 IScheduler 实例
            var scheduler = sf.GetScheduler().Result;

            // 启动调度器
            scheduler.Start();

            // 定义作业详情
            var jobDetail = JobBuilder.Create<HelloJob>()
                .WithIdentity("job1", "group1")
                .Build();

            // 定义触发器，立即执行，之后每5秒执行一次
            var trigger = TriggerBuilder.Create()
                .WithIdentity("trigger1", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever())
                .Build();

            // 将作业和触发器添加到调度器
            scheduler.ScheduleJob(jobDetail, trigger);

            // 为了能看到输出，让主线程等待
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            // 正常关闭调度器
            scheduler.Shutdown();
        }
    }

    public class HelloJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Hello, Quartz.NET Job Executed at: " + DateTime.Now);
        }

        Task IJob.Execute(IJobExecutionContext context)
        {
            Console.WriteLine("Hello, Quartz.NET Job Executed at: " + DateTime.Now);
            //throw new NotImplementedException();
        }
    }
}
