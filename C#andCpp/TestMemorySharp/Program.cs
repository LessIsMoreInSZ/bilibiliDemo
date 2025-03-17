using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TestMemorySharp
{
    internal class Program
    {
        // C#端启用CRT调试
        [DllImport("msvcrtd.dll")]
        public static extern void _CrtSetDbgFlag(UInt32 flag);

        [DllImport("TestMemory.dll")]
        public static extern void FreeMemory(IntPtr ptr);

        [DllImport("TestMemory.dll")]
        public static extern IntPtr AllocateMemory(int size);

        static void Main(string[] args)
        {
            //_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);

            #region low逼版
            //Console.WriteLine("开始内存泄漏测试...");

            //// 循环分配内存（但不释放）
            //for (int i = 0; i < 10000; i++)
            //{
            //    AllocateMemory(5000); // 分配1KB
            //    Console.WriteLine($"分配第 {i + 1} 块内存");
            //}
            //Console.WriteLine("\n测试结束 - 内存未释放！");
            #endregion

            //改进版
            IntPtr[] handles = new IntPtr[10000];
            for (int i = 0; i < 10000; i++)
            {
                handles[i] = AllocateMemory(5000);
            }
            Console.WriteLine($"分配完成，Private Bytes: {GetPrivateBytes()}");


            // 正确释放内存
            //foreach (IntPtr ptr in handles)
            //{
            //    FreeMemory(ptr);
            //}

            // 释放内存
            for (int i = 0; i < handles.Length; i++)
            {
                FreeMemory(handles[i]);
            }
            Console.WriteLine($"释放完成，Private Bytes: {GetPrivateBytes()}");

            Console.ReadLine();
        }

        private static long GetPrivateBytes()
        {
            using (var process = Process.GetCurrentProcess())
            {
                return process.PrivateMemorySize64 / 1024 / 1024;
            }

        }
    }

    public class SafeBuffer : IDisposable
    {
        [DllImport("TestMemory.dll")]
        public static extern void FreeMemory(IntPtr ptr);

        [DllImport("TestMemory.dll")]
        public static extern IntPtr AllocateMemory(int size);
        private IntPtr _handle;

        public SafeBuffer(int size)
        {
            _handle = AllocateMemory(size);
        }

        public void Dispose()
        {
            FreeMemory(_handle);
            GC.SuppressFinalize(this);
        }
    }
}
