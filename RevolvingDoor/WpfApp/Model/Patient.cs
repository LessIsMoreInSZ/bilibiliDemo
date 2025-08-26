using GalaSoft.MvvmLight;
using System;

namespace WpfApp.Model
{
    /// <summary>
    /// 病人
    /// </summary>
    public sealed class Patient : ObservableObject
    {
        /// <summary>
        /// 流水号
        /// </summary>
        public Int32 No { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public String Name { get; set; }
    }
}
