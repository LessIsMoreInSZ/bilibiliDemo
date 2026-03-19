using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CrashTest.Unified;

public static class CrashTrigger
{
    private static readonly Dictionary<string, Action> Triggers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["av"] = TriggerAccessViolation,
        ["so"] = TriggerStackOverflow,
        ["hc"] = TriggerHeapCorruption,
        ["dz"] = TriggerDivideByZero,
        ["nr"] = TriggerNullReference,
        ["oom"] = TriggerOutOfMemory,
        ["dl"] = TriggerDeadlock,
        ["md"] = TriggerMissingDll,
        ["ue"] = TriggerUnhandledException,
        ["ta"] = TriggerThreadAbort,
        ["se"] = TriggerSecurityException,
        ["pf"] = TriggerPageFault,
        ["sf"] = TriggerStackBufferOverflow,
        ["io"] = TriggerFatalIOError,
        ["gc"] = TriggerGCFatal,
        ["jit"] = TriggerJitFatal,
        ["tc"] = TriggerThreadContention,
        ["hl"] = TriggerHandleLeak,
        ["com"] = TriggerCOMCrash
    };

    private static readonly object LockA = new();
    private static readonly object LockB = new();

    public static IReadOnlyDictionary<string, (string Name, string Description, uint? ExitCode)> GetAvailableCrashes()
    {
        return new Dictionary<string, (string, string, uint?)>
        {
            ["av"] = ("Access Violation", "内存访问冲突，直接读写无效地址", 0xC0000005),
            ["so"] = ("Stack Overflow", "递归吃满线程栈", 0xC00000FD),
            ["hc"] = ("Heap Corruption", "破坏非托管堆块后释放", 0xC0000374),
            ["dz"] = ("Divide By Zero", "托管除零异常", 0xE0434352),
            ["nr"] = ("Null Reference", "空引用解引用", 0xE0434352),
            ["oom"] = ("Out Of Memory", "持续分配内存直到耗尽", null),
            ["dl"] = ("Deadlock", "双锁互相等待，界面卡死", null),
            ["md"] = ("Missing DLL", "调用不存在的 DLL", 0xC0000135),
            ["ue"] = ("Unhandled Exception", "直接抛出未处理异常", 0xE0434352),
            ["ta"] = ("Thread Abort", "模拟现代 .NET 下不支持的线程中止", 0xE0434352),
            ["se"] = ("Security Exception", "直接抛出安全异常", 0xE0434352),
            ["pf"] = ("Page Fault", "访问保留但不可访问的内存页", 0xC0000005),
            ["sf"] = ("Stack Buffer Overflow", "栈上缓冲区越界写入", 0xC0000409),
            ["io"] = ("Fatal IO Error", "打开非法设备路径导致未处理 IO 错误", 0xE0434352),
            ["gc"] = ("GC Fatal", "破坏内存后强制 GC", null),
            ["jit"] = ("JIT Fatal", "执行非法 IL", 0xE0434352),
            ["tc"] = ("Thread Contention", "高竞争锁导致程序完全无响应", null),
            ["hl"] = ("Handle Leak", "持续泄漏句柄和 GDI 资源", null),
            ["com"] = ("COM Crash", "破坏 COM 对象引用计数", null)
        };
    }

    public static void Execute(string crashType)
    {
        var key = crashType.Trim().ToLowerInvariant();
        if (!Triggers.TryGetValue(key, out var action))
        {
            throw new ArgumentOutOfRangeException(nameof(crashType), $"未知崩溃类型: {crashType}");
        }

        action();
    }

    private static unsafe void TriggerAccessViolation()
    {
        var pointer = (int*)0x1;
        *pointer = unchecked((int)0xDEADBEEF);
    }

    private static void TriggerStackOverflow()
    {
        EatStack(0);
    }

    private static void EatStack(int depth)
    {
        Span<byte> buffer = stackalloc byte[8192];
        buffer[0] = (byte)depth;
        buffer[^1] = (byte)(depth >> 8);
        EatStack(depth + 1);
    }

    private static unsafe void TriggerHeapCorruption()
    {
        var ptr = Marshal.AllocHGlobal(32);
        try
        {
            var bytes = (byte*)ptr;
            for (var i = 0; i < 4096; i++)
            {
                bytes[i] = 0xCC;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    private static void TriggerDivideByZero()
    {
        var zero = 0;
        _ = 1 / zero;
    }

    private static void TriggerNullReference()
    {
        string? value = null;
        _ = value!.Length;
    }

    private static void TriggerOutOfMemory()
    {
        var allocations = new List<byte[]>();
        while (true)
        {
            var chunk = new byte[50 * 1024 * 1024];
            for (var i = 0; i < chunk.Length; i += 4096)
            {
                chunk[i] = 1;
            }

            allocations.Add(chunk);
        }
    }

    private static void TriggerDeadlock()
    {
        var first = new Thread(() =>
        {
            lock (LockA)
            {
                Thread.Sleep(200);
                lock (LockB)
                {
                }
            }
        });

        var second = new Thread(() =>
        {
            lock (LockB)
            {
                Thread.Sleep(200);
                lock (LockA)
                {
                }
            }
        });

        first.Start();
        second.Start();
        first.Join();
        second.Join();
    }

    [DllImport("DefinitelyNotExist_TriggerMissingDll.dll", EntryPoint = "NonExistentFunction")]
    private static extern void NonExistentFunction();

    private static void TriggerMissingDll()
    {
        NonExistentFunction();
    }

    private static void TriggerUnhandledException()
    {
        throw new InvalidOperationException("这是一个故意抛出的未处理异常。");
    }

    private static void TriggerThreadAbort()
    {
        throw new PlatformNotSupportedException("Thread.Abort 在现代 .NET 上不可用，这里用未处理异常模拟。");
    }

    private static void TriggerSecurityException()
    {
        throw new System.Security.SecurityException("模拟安全异常。");
    }

    private static unsafe void TriggerPageFault()
    {
        var reserved = VirtualAlloc(IntPtr.Zero, 4096, 0x2000, 0x01);
        if (reserved == IntPtr.Zero)
        {
            throw new InvalidOperationException("VirtualAlloc 失败。");
        }

        var value = *(int*)reserved;
        GC.KeepAlive(value);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    private static unsafe void TriggerStackBufferOverflow()
    {
        var buffer = stackalloc int[16];
        for (var i = 0; i < 1024 * 1024; i++)
        {
            buffer[i] = i;
        }
    }

    private static void TriggerFatalIOError()
    {
        using var stream = File.Open(@"\\.\NONEXISTENT_DEVICE", FileMode.Open, FileAccess.Read, FileShare.None);
        GC.KeepAlive(stream);
    }

    private static unsafe void TriggerGCFatal()
    {
        var block = Marshal.AllocHGlobal(8);
        try
        {
            var ptr = (nint*)block;
            for (var i = -4096; i < 4096; i++)
            {
                ptr[i] = unchecked((nint)0xDEADBEEF);
            }
        }
        finally
        {
            Marshal.FreeHGlobal(block);
        }

        GC.Collect(2, GCCollectionMode.Aggressive, true, true);
    }

    private static void TriggerJitFatal()
    {
        var method = new DynamicMethod("Broken", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Ldc_I4_2);
        il.Emit(OpCodes.Ret);

        var action = (Action)method.CreateDelegate(typeof(Action));
        action();
    }

    private static void TriggerThreadContention()
    {
        var gate = new object();
        var threads = new List<Thread>();
        var workerCount = Environment.ProcessorCount * 4;

        for (var i = 0; i < workerCount; i++)
        {
            var thread = new Thread(() =>
            {
                while (true)
                {
                    lock (gate)
                    {
                        for (var j = 0; j < 5000; j++)
                        {
                            _ = Math.Sqrt(j);
                        }
                    }
                }
            })
            {
                Priority = ThreadPriority.Highest,
                IsBackground = true
            };

            threads.Add(thread);
            thread.Start();
        }

        while (true)
        {
            lock (gate)
            {
                Thread.Sleep(1);
            }
        }
    }

    private static void TriggerHandleLeak()
    {
        Task.Run(() =>
        {
            while (true)
            {
                _ = CreateFile(
                    @"C:\Windows\System32\kernel32.dll",
                    0x80000000,
                    0,
                    IntPtr.Zero,
                    3,
                    0,
                    IntPtr.Zero);
            }
        });

        Task.Run(() =>
        {
            while (true)
            {
                var font = new Font("Microsoft YaHei UI", 12);
                var brush = new SolidBrush(Color.Red);
                var pen = new Pen(Color.Black);
                var bitmap = new Bitmap(64, 64);
                var graphics = Graphics.FromImage(bitmap);

                graphics.DrawEllipse(pen, 2, 2, 60, 60);
                graphics.DrawString("leak", font, brush, new PointF(2, 20));
            }
        });

        while (true)
        {
            var process = Process.GetCurrentProcess();
            Console.WriteLine($"Handles: {process.HandleCount}, GDI: {GetGdiHandleCount()}");
            Thread.Sleep(1000);
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    [DllImport("user32.dll")]
    private static extern int GetGuiResources(IntPtr hProcess, uint uiFlags);

    private static int GetGdiHandleCount()
    {
        using var process = Process.GetCurrentProcess();
        return GetGuiResources(process.Handle, 0);
    }

    private static unsafe void TriggerCOMCrash()
    {
        var comType = Type.GetTypeFromProgID("Shell.Application")
            ?? throw new COMException("无法创建 Shell.Application COM 对象。");

        var comObject = Activator.CreateInstance(comType)
            ?? throw new COMException("Activator.CreateInstance 返回了 null。");

        var unknown = Marshal.GetIUnknownForObject(comObject);
        Marshal.ReleaseComObject(comObject);

        try
        {
            var vtable = *(IntPtr**)unknown;
            var release = Marshal.GetDelegateForFunctionPointer<ReleaseDelegate>(vtable[2]);
            _ = release(unknown);
            _ = release(unknown);
        }
        finally
        {
            Marshal.Release(unknown);
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate uint ReleaseDelegate(IntPtr instance);
}
