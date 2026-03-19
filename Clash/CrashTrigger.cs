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
    /// <summary>
    /// 保存崩溃类型缩写与触发逻辑之间的映射关系。
    /// </summary>
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

    /// <summary>
    /// 死锁测试使用的第一个锁对象。
    /// </summary>
    private static readonly object LockA = new();

    /// <summary>
    /// 死锁测试使用的第二个锁对象。
    /// </summary>
    private static readonly object LockB = new();

    /// <summary>
    /// 返回所有可触发崩溃的展示信息，供界面和命令行入口使用。
    /// </summary>
    /// <returns>以崩溃类型缩写为键的描述信息字典。</returns>
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

    /// <summary>
    /// 根据崩溃类型缩写执行对应的触发逻辑。
    /// </summary>
    /// <param name="crashType">崩溃类型缩写。</param>
    public static void Execute(string crashType)
    {
        var key = crashType.Trim().ToLowerInvariant();
        if (!Triggers.TryGetValue(key, out var action))
        {
            throw new ArgumentOutOfRangeException(nameof(crashType), $"未知崩溃类型: {crashType}");
        }

        action();
    }

    /// <summary>
    /// 通过写入无效内存地址制造访问冲突。
    /// </summary>
    private static unsafe void TriggerAccessViolation()
    {
        var pointer = (int*)0x1;
        *pointer = unchecked((int)0xDEADBEEF);
    }

    /// <summary>
    /// 通过无限递归持续消耗栈空间，最终触发栈溢出。
    /// </summary>
    private static void TriggerStackOverflow()
    {
        EatStack(0);
    }

    /// <summary>
    /// 为栈溢出测试递归分配栈内缓冲区，加速栈空间耗尽。
    /// </summary>
    /// <param name="depth">当前递归深度。</param>
    private static void EatStack(int depth)
    {
        Span<byte> buffer = stackalloc byte[8192];
        buffer[0] = (byte)depth;
        buffer[^1] = (byte)(depth >> 8);
        EatStack(depth + 1);
    }

    /// <summary>
    /// 在非托管堆上越界写入后释放内存，用于模拟堆损坏。
    /// </summary>
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

    /// <summary>
    /// 执行除零操作，触发托管算术异常。
    /// </summary>
    private static void TriggerDivideByZero()
    {
        var zero = 0;
        _ = 1 / zero;
    }

    /// <summary>
    /// 解引用空引用，触发空引用异常。
    /// </summary>
    private static void TriggerNullReference()
    {
        string? value = null;
        _ = value!.Length;
    }

    /// <summary>
    /// 持续分配大块内存直到进程内存耗尽。
    /// </summary>
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

    /// <summary>
    /// 启动两个线程以相反顺序获取锁，构造稳定死锁。
    /// </summary>
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

    /// <summary>
    /// 声明一个实际不存在的 DLL 导入项，用于触发加载失败。
    /// </summary>
    [DllImport("DefinitelyNotExist_TriggerMissingDll.dll", EntryPoint = "NonExistentFunction")]
    private static extern void NonExistentFunction();

    /// <summary>
    /// 调用不存在的原生函数，触发 DLL 缺失异常。
    /// </summary>
    private static void TriggerMissingDll()
    {
        NonExistentFunction();
    }

    /// <summary>
    /// 直接抛出未处理异常，验证默认异常终止路径。
    /// </summary>
    private static void TriggerUnhandledException()
    {
        throw new InvalidOperationException("这是一个故意抛出的未处理异常。");
    }

    /// <summary>
    /// 在现代 .NET 中用未处理异常模拟线程中止类故障。
    /// </summary>
    private static void TriggerThreadAbort()
    {
        throw new PlatformNotSupportedException("Thread.Abort 在现代 .NET 上不可用，这里用未处理异常模拟。");
    }

    /// <summary>
    /// 直接抛出安全异常，模拟权限或安全策略失败。
    /// </summary>
    private static void TriggerSecurityException()
    {
        throw new System.Security.SecurityException("模拟安全异常。");
    }

    /// <summary>
    /// 保留一页不可访问内存并直接读取，模拟页错误。
    /// </summary>
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

    /// <summary>
    /// 调用 Win32 VirtualAlloc 分配或保留内存页。
    /// </summary>
    /// <param name="lpAddress">建议的起始地址。</param>
    /// <param name="dwSize">分配大小，单位为字节。</param>
    /// <param name="flAllocationType">分配类型标志。</param>
    /// <param name="flProtect">页面保护属性。</param>
    /// <returns>成功时返回分配地址，失败时返回零指针。</returns>
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAlloc(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    /// <summary>
    /// 在栈上小缓冲区外进行大量写入，用于触发栈缓冲区破坏。
    /// </summary>
    private static unsafe void TriggerStackBufferOverflow()
    {
        var buffer = stackalloc int[16];
        for (var i = 0; i < 1024 * 1024; i++)
        {
            buffer[i] = i;
        }
    }

    /// <summary>
    /// 打开非法设备路径，触发严重 IO 异常。
    /// </summary>
    private static void TriggerFatalIOError()
    {
        using var stream = File.Open(@"\\.\NONEXISTENT_DEVICE", FileMode.Open, FileAccess.Read, FileShare.None);
        GC.KeepAlive(stream);
    }

    /// <summary>
    /// 先破坏非托管内存，再强制执行 GC，模拟更隐蔽的致命内存错误。
    /// </summary>
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

    /// <summary>
    /// 构造非法 IL 并执行，触发 JIT 或运行时异常。
    /// </summary>
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

    /// <summary>
    /// 创建大量高优先级线程竞争同一把锁，使进程陷入严重线程争用。
    /// </summary>
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

    /// <summary>
    /// 持续泄漏文件句柄和 GDI 资源，观察进程资源耗尽过程。
    /// </summary>
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

    /// <summary>
    /// 调用 Win32 CreateFile 创建文件句柄；此处故意不关闭以模拟句柄泄漏。
    /// </summary>
    /// <param name="lpFileName">目标文件路径。</param>
    /// <param name="dwDesiredAccess">访问权限标志。</param>
    /// <param name="dwShareMode">共享模式标志。</param>
    /// <param name="lpSecurityAttributes">安全属性指针。</param>
    /// <param name="dwCreationDisposition">创建方式标志。</param>
    /// <param name="dwFlagsAndAttributes">文件属性与标志。</param>
    /// <param name="hTemplateFile">模板文件句柄。</param>
    /// <returns>创建出的原生文件句柄。</returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile);

    /// <summary>
    /// 读取指定进程的 USER 或 GDI 资源计数。
    /// </summary>
    /// <param name="hProcess">目标进程句柄。</param>
    /// <param name="uiFlags">资源类型标志，0 表示 GDI。</param>
    /// <returns>对应资源的当前数量。</returns>
    [DllImport("user32.dll")]
    private static extern int GetGuiResources(IntPtr hProcess, uint uiFlags);

    /// <summary>
    /// 返回当前进程的 GDI 句柄数量。
    /// </summary>
    /// <returns>当前进程的 GDI 资源计数。</returns>
    private static int GetGdiHandleCount()
    {
        using var process = Process.GetCurrentProcess();
        return GetGuiResources(process.Handle, 0);
    }

    /// <summary>
    /// 错误操作 COM 对象引用计数，模拟原生互操作崩溃。
    /// </summary>
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

    /// <summary>
    /// 表示 COM 对象 Release 函数签名的委托。
    /// </summary>
    /// <param name="instance">COM 接口实例指针。</param>
    /// <returns>释放后的剩余引用计数。</returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate uint ReleaseDelegate(IntPtr instance);
}
