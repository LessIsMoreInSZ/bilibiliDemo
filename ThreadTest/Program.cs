using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadTest
{
    internal class Program
    {
        #region 验证lock的耗时
        //private static readonly object testLock = new object();
        //object a;
        //static void Main(string[] args)
        //{
        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    Task.Run(() =>
        //    {
        //        Thread.Sleep(5000);
        //        lock (testLock)
        //        {
        //            //Console.WriteLine("In Lock");
        //        }
        //        Console.WriteLine("in");
        //    });
        //    stopwatch.Stop();
        //    Console.WriteLine($"In Lock Time:{stopwatch.ElapsedMilliseconds}");

        //    Stopwatch stopwatch1 = new Stopwatch();
        //    stopwatch1.Start();
        //    Task.Run(() =>
        //    {
        //        Thread.Sleep(5000);
        //        Console.WriteLine("out");
        //    });
        //    stopwatch1.Stop();
        //    Console.WriteLine($"No Lock Time:{stopwatch1.ElapsedMilliseconds}");
        //    Console.ReadLine();

        //    Task.Run(() =>
        //    {
        //        object a = new object();
        //        while (true)
        //        {
        //            int b = int.Parse(a.ToString());
        //            b++;
        //        }
        //    });
        //}

        #endregion

        #region Lock使用
        public class SharedState
        {
            public int State { get; set; }
        }

        public class Job
        {
            private SharedState _sharedState;
            public Job(SharedState sharedState) => _sharedState = sharedState;

            public void DoTheJob()
            {
                for (int i = 0; i < 50000; i++)
                {
                    // 多个任务使用相同的字段，可以锁定所有任务
                    lock (_sharedState)
                    {
                        _sharedState.State += 1;
                    }
                }
            }
        }

        static void Main()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            // 创建多任务共享的字段
            var state = new SharedState();
            // 定义任务的数量
            var tasks = new System.Threading.Tasks.Task[20];

            for (int i = 0; i < tasks.Length; i++)
                // 虽然使用的是不同的Job实例，但传入的是相同的字段
                tasks[i] = System.Threading.Tasks.Task.Run(() => new Job(state).DoTheJob());

            // 等待所有任务执行完成
            System.Threading.Tasks.Task.WaitAll(tasks);

            // 输出多任务的执行结果
            System.Console.WriteLine($"summarized {state.State}");
            stopwatch.Stop();
            //Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.WriteLine(stopwatch.ElapsedTicks);


            // 单个线程测试
            Stopwatch stopwatch1 = new Stopwatch();
            stopwatch1.Start();
            int test = 0;
            for(int i = 0; i < 1000000; i++)
            {
                test++;
            }
            stopwatch1.Stop();
            Console.WriteLine(stopwatch1.ElapsedTicks);

            // 单个线程不引入局部变量测试
            Stopwatch stopwatch2 = new Stopwatch();
            stopwatch2.Start();
            int test1 = 0;
            for (test1 = 0; test1 < 1000000; test1++) { }
            stopwatch2.Stop();
            Console.WriteLine(stopwatch2.ElapsedTicks);

            Console.ReadKey();
        }
    }

    #endregion
}

