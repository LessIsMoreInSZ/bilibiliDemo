using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfApp.Controls
{
    /// <summary>
    /// 时钟
    /// </summary>
    public sealed class Clock : Control
    {
        #region DependencyProperty

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(DateTime), typeof(Clock));
        /// <summary>
        /// 当前时间
        /// </summary>
        public DateTime Time { get => (DateTime)GetValue(TimeProperty); set => SetValue(TimeProperty, value); }

        public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register("SmallChange", typeof(UInt32), typeof(Clock), new PropertyMetadata(1000u, new PropertyChangedCallback(OnSmallChangePropertyChangedCallback)));

        private static void OnSmallChangePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Clock)d).BeginUpdate();
        }

        /// <summary>
        /// 最小改变毫秒数
        /// </summary>
        public UInt32 SmallChange { get => (UInt32)GetValue(SmallChangeProperty); set => SetValue(SmallChangeProperty, value); }

        #endregion

        static Clock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Clock), new FrameworkPropertyMetadata(typeof(Clock)));
        }

        public Clock()
        {
            timer = new DispatcherTimer();
            timer.Tick += OnTick;

            IsVisibleChanged += delegate { BeginUpdate(); };
        }

        private void BeginUpdate()
        {
            // 不可见时，停用计时器
            if (!IsVisible || SmallChange == 0)
            {
                if (timer.IsEnabled)
                    timer.Stop();
            }
            else
            {
                if (timer.Interval.TotalMilliseconds != SmallChange)
                    timer.Interval = TimeSpan.FromMilliseconds(SmallChange);

                timer.Start();
            }
        }

        private void OnTick(Object sender, EventArgs e)
        {
            Time = DateTime.Now;
        }

        private DispatcherTimer timer;
    }
}
