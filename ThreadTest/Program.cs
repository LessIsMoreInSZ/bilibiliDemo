using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadTest
{
    internal class Program
    {
        private static readonly object testLock = new object();
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Task.Run(() =>
            {
                Thread.Sleep(5000);
                lock (testLock)
                {
                    //Console.WriteLine("In Lock");
                }
                Console.WriteLine("in");
            });
            stopwatch.Stop();
            Console.WriteLine($"In Lock Time:{stopwatch.ElapsedMilliseconds}");

            Stopwatch stopwatch1 = new Stopwatch();
            stopwatch1.Start();
            Task.Run(() =>
            {
                Thread.Sleep(5000);
                Console.WriteLine("out");
            });
            stopwatch1.Stop();
            Console.WriteLine($"No Lock Time:{stopwatch1.ElapsedMilliseconds}");
            Console.ReadLine();
        }
    }
}
