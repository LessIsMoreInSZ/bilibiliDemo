/*----------------------------------------------------------------
// 示例：快照对比探针 SnapshotProbe
//
// 回答的问题："对象状态到底是哪一步被谁改掉的？"
// 能力：把对象属性 Dump 成文本快照；两份快照 diff，打印改动的字段。
//
// 场景：配置对象"波特率不知什么时候从 9600 变成了 19200"；
//       列表项莫名少了一条；状态标志位被某个隐藏分支翻转——
//       在可疑方法前后各拍一张快照，diff 直接给出嫌疑人名单。
//----------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ProbeDemo
{
    /// <summary>对象快照/对比探针（反射公共实例属性，跳过索引器）。</summary>
    public static class SnapshotProbe
    {
        /// <summary>Dump 对象的全部公共实例属性为 "名=值" 行（换行分隔）。</summary>
        public static string Dump(object obj)
        {
            if (obj == null) return "(null)";
            var sb = new StringBuilder();
            foreach (var prop in PublicProps(obj.GetType()))
            {
                object v;
                try
                {
                    v = prop.GetValue(obj);
                }
                catch (TargetInvocationException ex) when (ex.InnerException != null)
                {
                    Exception cause = ex.InnerException;
                    v = $"<读取失败: {cause.GetType().Name}: {cause.Message}>";
                }
                sb.Append(prop.Name).Append('=').Append(v).AppendLine();
            }
            return sb.ToString();
        }

        /// <summary>
        /// 对比改动前后的快照，按字段逐项打印 变更/新增/消失。
        /// beforeDump 用先拍的 Dump() 结果；after 传修改后的对象。
        /// Release（未定义 PROBE）下整条剥除。
        /// </summary>
        [Conditional("PROBE")]
        public static void Compare(string name, string beforeDump, object after)
        {
            var d1 = Parse(beforeDump);
            var d2 = Parse(Dump(after));

            foreach (string key in d1.Keys.Union(d2.Keys))
            {
                string v1 = d1.TryGetValue(key, out var a) ? a : null;
                string v2 = d2.TryGetValue(key, out var b) ? b : null;
                if (string.Equals(v1, v2)) continue;

                if (v1 == null)      Write($"{Now} [SNAP:{name}] 新增: {key}={v2}");
                else if (v2 == null) Write($"{Now} [SNAP:{name}] 消失: {key}={v1}");
                else                 Write($"{Now} [SNAP:{name}] {key}: {v1} -> {v2}");
            }
        }

        private static IEnumerable<PropertyInfo> PublicProps(Type t) =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
             .Where(p => p.GetIndexParameters().Length == 0);   // 跳过 Chars 这类索引器

        private static Dictionary<string, string> Parse(string dump)
        {
            var dict = new Dictionary<string, string>();
            if (dump == null) return dict;
            foreach (string line in dump.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                int eq = line.IndexOf('=');
                if (eq > 0) dict[line.Substring(0, eq)] = line.Substring(eq + 1);
            }
            return dict;
        }

        [Conditional("PROBE")]
        private static void Write(string line) => Trace.WriteLine(line);

        private static string Now => "[PROBE] " + DateTime.Now.ToString("HH:mm:ss.fff");
    }
}
