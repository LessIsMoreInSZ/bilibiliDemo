using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl;
namespace QuartzTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //实例一个调度工厂，并且从调度工厂实例一个调度器
            //此时线程已启动，不过是处于等待时间
            StdSchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = factory.GetScheduler().Result;

            //创建一个Job,与业务逻辑JobLgoTask绑定
            //IJobDetail job = JobBuilder
            //                    .Create<JobLgoTask>()                   //获取JobBuilder
            //                    .WithIdentity("jobname1", "group1")     //添加Job的名字和分组
            //                    .WithDescription("一个简单的任务")      //添加描述
            //                    .Build();                               //生成IJobDetail绑定

            var JobDataMap = new JobDataMap();
            JobDataMap.Add("Name", "张三");
            JobDataMap.Add("Age", "1");
            JobDataMap.Add("IsRun", "false");

            IJobDetail job = JobBuilder.Create<JobLgoTask>()
                                .WithIdentity("job1", "jobGroup1")      //唯一标识
                                .UsingJobData(JobDataMap)
                                .StoreDurably(true)                     //即时没有指定的触发器，该job也会被存储
                                .SetJobData(JobDataMap)                 //设置datamap值，与using有相同效果
                                .WithDescription("我是描述信息")        //该作业的描述信息
                                .RequestRecovery(true)                  //如果当前任务崩溃，则会重新执行该作业
                                .Build();

            //创建一个触发器
            //ITrigger trigger =
            //    TriggerBuilder.Create()                                  //获取TriggerBuilder
            //                  .StartNow()                                //马上开始
            //                  .StartAt(DateBuilder.TodayAt(01, 00, 00))  //开始时间，今天的1点（hh,mm,ss），可使用StartNow()
            //                  .EndAt(DateBuilder.TodayAt(06,00,00))      //结束时间 今天的6点
            //                  .ForJob(job)                               //将触发器关联给指定的job
            //                  .WithPriority(10)                          //优先级，当触发时间一样时，优先级大的触发器先执行
            //                  .WithIdentity("tname1", "group1")          //添加名字和分组
            //                  .WithSimpleSchedule(x => x.WithIntervalInSeconds(1) //调度，一秒执行一次，执行三次
            //                                            .WithRepeatCount(10)
            //                                            .Build())
            //                  .Build();

            //ITrigger trigger = TriggerBuilder.Create()
            //                    .WithIdentity("trigger1", "triggerGroup1")
            //                    .StartNow()
            //                    .WithDailyTimeIntervalSchedule(w => w
            //                                                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(1, 0))            //1点开始
            //                                                    .EndingDailyAt(TimeOfDay.HourMinuteAndSecondOfDay(22, 10, 30))  //22点10分30秒结束
            //                                                    .OnEveryDay()                                                   //每天
            //                                                    .OnMondayThroughFriday()                                        //周一至周五
            //                                                    .OnSaturdayAndSunday()                                          //周末
            //                                                    .OnDaysOfTheWeek(DayOfWeek.Monday,DayOfWeek.Friday)             //指定周一，周五
            //                                                    .EndingDailyAfterCount(5)                                       //每天执行5次结束
            //                                                    )
            //                    .Build();


            //创建一个触发器
            ITrigger trigger =
                TriggerBuilder.Create()                                  //获取TriggerBuilder
                              .StartNow()                                //马上开始
                              .StartAt(DateBuilder.TodayAt(13, 00, 00))  //开始时间，今天的1点（hh,mm,ss），可使用StartNow()
                              .EndAt(DateBuilder.TodayAt(15, 00, 00))      //结束时间 今天的6点
                              .ForJob(job)                               //将触发器关联给指定的job
                              .WithPriority(10)                          //优先级，当触发时间一样时，优先级大的触发器先执行
                              .WithIdentity("tname1", "group1")          //添加名字和分组
                              //.WithSimpleSchedule(x => x.WithIntervalInSeconds(1) //调度，一秒执行一次，执行三次
                              //                          .WithRepeatCount(10)
                              //                          .Build())
                              .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMilliseconds(50)) //调度，一秒执行一次，执行三次
                                                        .WithRepeatCount(100)
                                                        .Build())
                              .Build();
            scheduler.ScheduleJob(job, trigger);
            scheduler.Start();
            Console.ReadKey();
        }
    }
}
