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
                carouselText = "WPF�����ֲ����֣������Ч�������ɵ��ڹ����ٶȡ���β��ࡢ���ִ�С����ɫ���Լ�����ı������߿�����߾ࣻ���ֲ��޳��ȡ��Զ���Ӧ������С...";
            else
                carouselText = "WPF�����ֲ����֣������Ч����";

            var queue_count = random.Next(5, 20);

            var doctors = new Doctor[]
            {
                    new Doctor
                    {
                        Name = "��һ��",
                        Avatar = "Images\\1.jpg",
                        Job = "����ҽʦ"
                    },
                    new Doctor
                    {
                        Name = "��ȵ",
                        Avatar = "Images\\2.jpg",
                        Job = "ʵϰ��"
                    },
                    new Doctor
                    {
                        Name = "��٢����",
                        Avatar = "Images\\3.jpg",
                        Job = "ҽԺԺ��"
                    }
            };

            var names = new String[] { "������", "ŷ������", "ŷ��ӱ��", "������", "���M", "���" };

            for (var i = 0; i < queue_count; i++)
            {
                var callQueueData = new CallQueueData
                {
                    DeptName = $"��{i + 1}����",
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

        private String hospitalName = "�˳����׸�����ҽԺ";
        /// <summary>
        /// ҽԺ����
        /// </summary>
        public String HospitalName { get => hospitalName; set => this.Set(ref hospitalName, value); }

        private String title = "�������ȴ���";
        /// <summary>
        /// ����
        /// </summary>
        public String Title { get => title; set => this.Set(ref title, value); }

        public String carouselText;
        /// <summary>
        /// ��������
        /// </summary>
        public String CarouselText { get => carouselText; set => this.Set(ref carouselText, value); }

        /// <summary>
        /// �кŶ��м���
        /// </summary>
        public ObservableCollection<CallQueueData> CallQueueDatas { get; } = new ObservableCollection<CallQueueData>();

        #endregion
    }
}