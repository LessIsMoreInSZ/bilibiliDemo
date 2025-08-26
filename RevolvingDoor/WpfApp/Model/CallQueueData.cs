using System;
using System.Collections.ObjectModel;

namespace WpfApp.Model
{
    /// <summary>
    /// 叫号队列
    /// </summary>
    public sealed class CallQueueData
    {
        /// <summary>
        /// 科室名称
        /// </summary>
        public String DeptName { get; set; }

        /// <summary>
        /// 医生
        /// </summary>
        public Doctor Doctor { get; set; }

        /// <summary>
        /// 病人列表
        /// </summary>
        public ObservableCollection<Patient> Patients { get; } = new ObservableCollection<Patient>();
    }
}
