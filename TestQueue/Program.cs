using System.Collections.Concurrent;
using System.Threading.Channels;

namespace TestQueue
{
    #region BlockingCollection
    //internal class Program
    //{
    //    private static CancellationTokenSource cts1 = new CancellationTokenSource();
    //    private static CancellationTokenSource cts2 = new CancellationTokenSource();

    //    private static BlockingCollection<string>? ReadWriteQueue = new BlockingCollection<string>();
    //    static void Main(string[] args)
    //    {


    //        //WriteTask();
    //        ReadTask();

    //        Console.ReadLine();
    //    }

    //    static void WriteTask()
    //    {
    //        Task.Factory.StartNew(() =>
    //        {
    //            Task.Delay(2000);
    //            ReadWriteQueue.Add(DateTime.Now.ToString());
    //        },cts1.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    //    }

    //    static void ReadTask()
    //    {
    //        Task.Factory.StartNew(() =>
    //        {
    //            string str = ReadWriteQueue.Take();
    //            Console.WriteLine(str);
    //            Console.WriteLine("Hello, World!");
    //        }, cts2.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    //    }
    //}
    #endregion

    public class Program
    {
        //你可以指定缓冲区大小，或者使用无界通道（BoundedChannelOptions为null）：
        static Channel<int> myChannel = Channel.CreateUnbounded<int>();

        private static CancellationTokenSource cts1 = new CancellationTokenSource();
        private static CancellationTokenSource cts2 = new CancellationTokenSource();
        static void Main(string[] args)
        {

            WriteTask();
            ReadTask();

            Console.ReadLine();
        }

        private async static Task WriteTask()
        {
            await Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (myChannel.Writer.WaitToWriteAsync().Result)
                    {
                        if (myChannel.Writer.TryWrite(42))
                        {
                            Console.WriteLine("Item written successfully.");
                        }
                        else
                        {
                            // 通道当前无法接受更多写入，可以在这里执行其他操作
                        }
                    }
                    else
                    {
                        // 通道暂时不可写，可以等待或处理这种情况
                    }
                }
            }, cts1.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);


            // 异步版本
            //async Task WriteItems(ChannelWriter<int> writer)
            //{
            //    for (int i = 0; i < 10; i++)
            //    {
            //        await writer.WaitToWriteAsync();
            //        while (!writer.TryWrite(i))
            //        {
            //            await Task.Yield(); // 让出执行权，稍后再试
            //        }
            //        Console.WriteLine($"Wrote: {i}");
            //    }
            //    writer.Complete();
            //}
        }

        private async static Task ReadTask()
        {
            await Task.Factory.StartNew(async() =>
            {
                while (true)
                {
                    if (myChannel.Reader.TryRead(out int item))
                    {
                        Console.WriteLine($"Received: {item}");
                    }
                    else
                    {
                        // 没有数据可读，可以在这里执行其他操作，而不是阻塞
                        await Task.Delay(100); // 可选：等待一段时间后再次尝试读取
                    }
                }
            }, cts2.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            // 异步版本
            //while (await myChannel.Reader.WaitToReadAsync())
            //{
            //    if (myChannel.Reader.TryRead(out int item))
            //    {
            //        Console.WriteLine($"Received: {item}");
            //    }
            //}
        }
    }
}
