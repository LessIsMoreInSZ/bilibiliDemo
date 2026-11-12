/*----------------------------------------------------------------
// 示例：文件 IO 探针 FileIoProbe
//
// 回答的问题："这个文件是不是被人改了？什么时候改的？改完的内容
//              和我想写进去的一致吗？"——这是配置文件漂移、
//              "日志突然不滚了"、"参数保存后读回来不对"这类问题的标准打法。
//
// 能力：
//   1) 内容指纹：对文件算 SHA256 短指纹 + 长度，前后各打一次，一比对就知道改没改。
//   2) 耗时测量：Measure 包住任意 IO 操作（读大文件/写日志/拷贝）。
//   3) 变更哨兵：Watch 用 FileSystemWatcher 盯着目录，谁动了文件立刻留痕。
//
// 场景：用户说"配置时好时坏"；怀疑多进程/多线程同时写一个文件；
//       想证明"程序根本没把文件写出来"而不是"写错了"。
//----------------------------------------------------------------*/
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ProbeDemo
{
    /// <summary>文件 IO 探针：内容指纹 / IO 耗时 / 目录变更哨兵。静态类。</summary>
    public static class FileIoProbe
    {
        /// <summary>打印文件指纹（SHA256 前 8 字节 + 大小）。文件不存在时明确留痕。</summary>
        [Conditional("PROBE")]
        public static void Fingerprint(string tag, string path)
        {
            if (!File.Exists(path))
            {
                Write($"{Now} [FILE:{tag}] {path} 不存在");
                return;
            }
            using (var sha = SHA256.Create())
            using (var fs = File.OpenRead(path))
            {
                var hash = sha.ComputeHash(fs);
                var sb = new StringBuilder(16);
                for (int i = 0; i < 8; i++) sb.Append(hash[i].ToString("X2"));
                Write($"{Now} [FILE:{tag}] {Path.GetFileName(path)} 指纹 {sb} · {fs.Length} 字节");
            }
        }

        /// <summary>比较两个指纹是否一致由外部肉眼/工具比对完成；这里提供便捷的"前后一致？"结论。</summary>
        [Conditional("PROBE")]
        public static void Compare(string tag, string pathA, string pathB)
        {
            bool same = SameContent(pathA, pathB);
            Write($"{Now} [FILE:{tag}] {Path.GetFileName(pathA)} vs {Path.GetFileName(pathB)} —— {(same ? "内容一致" : "内容不同（被改动）")}");
        }

        /// <summary>
        /// IO 耗时测量。注意：本方法【不】挂 [Conditional]——业务 io() 必须照常执行，
        /// 只有打点输出经条件剥除（与 ProbeScope.Run 同构）。
        /// 千万不要把业务代码写进挂 Conditional 的探针委托里，Release 下会被连业务一起剥掉。
        /// </summary>
        public static void Measure(string name, Action io)
        {
            var watch = Stopwatch.StartNew();
            try
            {
                io();
                Write($"{Now} [FILE:{name}] 完成，耗时 {watch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                Write($"{Now} [FILE:{name}] !! {ex.GetType().Name}: {ex.Message}（耗时 {watch.ElapsedMilliseconds}ms 后失败）");
                throw;
            }
        }

        /// <summary>目录变更哨兵：返回 FileSystemWatcher（需调用方保活）。事件全部经 Write 收口。</summary>
        public static FileSystemWatcher Watch(string tag, string dir)
        {
            var fsw = new FileSystemWatcher(dir) { EnableRaisingEvents = true, IncludeSubdirectories = true };
            FileSystemEventHandler onAny = (s, e) =>
                Write($"{Now} [FILE:{tag}] {e.ChangeType}: {e.Name}");
            RenamedEventHandler onRename = (s, e) =>
                Write($"{Now} [FILE:{tag}] Renamed: {e.OldName} -> {e.Name}");
            fsw.Created += onAny;
            fsw.Changed += onAny;
            fsw.Deleted += onAny;
            fsw.Renamed += onRename;
            Write($"{Now} [FILE:{tag}] 开始盯守目录 {dir}");
            return fsw;
        }

        private static bool SameContent(string a, string b)
        {
            if (!File.Exists(a) || !File.Exists(b)) return false;
            using (var sha = SHA256.Create())
            {
                string ha, hb;
                using (var fs = File.OpenRead(a)) ha = Convert.ToBase64String(sha.ComputeHash(fs));
                using (var fs = File.OpenRead(b)) hb = Convert.ToBase64String(sha.ComputeHash(fs));
                return ha == hb;
            }
        }

        [Conditional("PROBE")]
        private static void Write(string line)
        {
            Trace.WriteLine(line);
        }

        private static string Now => "[PROBE] " + DateTime.Now.ToString("HH:mm:ss.fff");
    }
}
