/*----------------------------------------------------------------
// 示例：作用域耗时探针 ProbeScope
//
// 两种用法：
//   1) using 作用域：
//      using (ProbeScope.Measure("加载配方")) { DoWork(); }
//   2) 委托包装（推荐，异常自动记录后再原样抛出）：
//      ProbeScope.Run("加载配方", () => DoWork());
//
// 退出时打印：总耗时、正常/异常退出。探针不改变异常语义（不动语义原则）。
//
// 说明：所有输出经私有 [Conditional("PROBE")] 方法 Write 收口——
// 没定义 PROBE 符号的 Release 发布构建里，打点调用被编译器剥除，
// 时间戳字符串拼接也不会执行（零开销探针）。
//----------------------------------------------------------------*/
using System;
using System.Diagnostics;

namespace ProbeDemo
{
    /// <summary>作用域耗时探针：Measure 返回的 IDisposable 在 Dispose 时打点。</summary>
    public sealed class ProbeScope : IDisposable
    {
        private readonly string _name;
        private readonly Stopwatch _watch = Stopwatch.StartNew();
        private string _exitStatus;

        private ProbeScope(string name) { _name = name; }

        public static ProbeScope Measure(string name)
        {
            var probe = new ProbeScope(name);
            Write($"{Now} >>> 进入 {name}");
            return probe;
        }

        /// <summary>委托包装：异常路径自动标记，记录后原样抛出。</summary>
        public static void Run(string name, Action action)
        {
            using (var probe = Measure(name))
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    probe.Fail(ex);
                    throw;
                }
            }
        }

        /// <summary>作用域中途手工打点（例如某关键行之后）。</summary>
        public void Mark(string note)
        {
            Write($"{Now} --- {_name} 推进至 {note}，已耗时 {_watch.ElapsedMilliseconds}ms");
        }

        /// <summary>标记异常退出（Run 自动调用；手工 using 时也可在 catch 中调用）。</summary>
        public void Fail(Exception ex)
        {
            _exitStatus = $"!! 异常退出 {ex.GetType().Name}: {ex.Message}";
        }

        public void Dispose()
        {
            _watch.Stop();
            Write($"{Now} <<< 退出 {_name}（{_exitStatus ?? "正常退出"}），总耗时 {_watch.ElapsedMilliseconds}ms");
        }

        // 剥除开关：Release（未定义 PROBE）下所有 Write 调用点整体消失
        [Conditional("PROBE")]
        private static void Write(string line)
        {
            Trace.WriteLine(line);
        }

        private static string Now => "[PROBE] " + DateTime.Now.ToString("HH:mm:ss.fff");
    }
}
