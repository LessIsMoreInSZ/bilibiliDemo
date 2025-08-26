using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WpfApp.Model;

namespace WpfApp.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            var random = new Random();

            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {

            }

            if (random.NextDouble() < 0.5)
                carouselText = "WPF滚动轮播文字（走马灯效果）；可调节滚动速度、首尾间距、文字大小、颜色，以及常规的背景、边框、内外边距；文字不限长度、自动适应容器大小...";
            else
                carouselText = "WPF滚动轮播文字（走马灯效果）";

            var queue_count = random.Next(5, 20);

            var doctors = new Doctor[]
            {
                    new Doctor
                    {
                        Name = "张一鸣",
                        Avatar = "Images\\1.jpg",
                        Job = "主任医师"
                    },
                    new Doctor
                    {
                        Name = "扁鹊",
                        Avatar = "Images\\2.jpg",
                        Job = "实习生"
                    },
                    new Doctor
                    {
                        Name = "华佗在世",
                        Avatar = "Images\\3.jpg",
                        Job = "医院院长"
                    }
            };

            var names = new String[] { "赵振文", "欧阳菊月", "欧阳颖慧", "芈语林", "邴芃", "冷洁" };

            for (var i = 0; i < queue_count; i++)
            {
                var callQueueData = new CallQueueData
                {
                    DeptName = $"第{i + 1}科室",
                    Doctor = doctors[random.Next(doctors.Length)]
                };

                var patient_count = random.Next(10);

                for (var j = 0; j < patient_count; j++)
                {
                    var patient = new Patient
                    {
                        Name = names[random.Next(names.Length)],
                        No = i * 10 + j + 1
                    };

                    callQueueData.Patients.Add(patient);
                }

                CallQueueDatas.Add(callQueueData);
            }
        }

        #region Properties

        private String hospitalName = "运城市幼妇保健医院";
        /// <summary>
        /// 医院名称
        /// </summary>
        public String HospitalName { get => hospitalName; set => this.Set(ref hospitalName, value); }

        private String title = "超声波等待区";
        /// <summary>
        /// 标题
        /// </summary>
        public String Title { get => title; set => this.Set(ref title, value); }

        public String carouselText;
        /// <summary>
        /// 滚动文字
        /// </summary>
        public String CarouselText { get => carouselText; set => this.Set(ref carouselText, value); }

        /// <summary>
        /// 叫号队列集合
        /// </summary>
        public ObservableCollection<CallQueueData> CallQueueDatas { get; } = new ObservableCollection<CallQueueData>();

        #endregion
    }
}