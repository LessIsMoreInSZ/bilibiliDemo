using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace WpfApp.Controls
{
    /// <summary>
    /// 文字走马灯效果
    /// </summary>
    [ContentProperty("Text")]
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_Txt1", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_Txt2", Type = typeof(TextBlock))]
    public sealed class CarouselText : Control
    {
        #region DependencyProperty

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(String), typeof(CarouselText));

        /// <summary>
        /// 滚动的文字
        /// </summary>
        public String Text { get => (String)GetValue(TextProperty); set => SetValue(TextProperty, value); }

        public static readonly DependencyProperty SpaceProperty = DependencyProperty.Register("Space", typeof(Double), typeof(CarouselText), new PropertyMetadata(OnSpacePropertyChanged));

        private static void OnSpacePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CarouselText)d).BeginUpdate();
        }

        /// <summary>
        /// 间距
        /// </summary>
        public Double Space { get => (Double)GetValue(SpaceProperty); set => SetValue(SpaceProperty, value); }

        public static readonly DependencyProperty SpeedProperty = DependencyProperty.Register("Speed", typeof(Double), typeof(CarouselText), new PropertyMetadata(120d, OnSpeedPropertyChanged));

        private static void OnSpeedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CarouselText)d).BeginUpdate();
        }

        /// <summary>
        /// 滚动速度（每秒wpf单位数）
        /// </summary>
        public Double Speed { get => (Double)GetValue(SpeedProperty); set => SetValue(SpeedProperty, value); }

        #endregion

        static CarouselText()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CarouselText), new FrameworkPropertyMetadata(typeof(CarouselText)));        // 重写默认样式
        }

        public CarouselText()
        {
            IsVisibleChanged += delegate { BeginUpdate(); };
        }

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (canvas != null)
                canvas.SizeChanged -= OnSizeChanged;

            if (txt1 != null)
            {
                txt1.SizeChanged -= OnSizeChanged;
                txt1.BeginAnimation(Canvas.LeftProperty, null);     // 停用动画
            }

            if (txt2 != null)
                txt2.BeginAnimation(Canvas.LeftProperty, null);     // 停用动画

            canvas = this.Template.FindName("PART_Canvas", this) as Canvas;
            txt1 = this.Template.FindName("PART_Txt1", this) as TextBlock;
            txt2 = this.Template.FindName("PART_Txt2", this) as TextBlock;

            if (canvas == null || txt1 == null || txt2 == null)
                return;

            txt1.SizeChanged += OnSizeChanged;
            canvas.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(Object sender, SizeChangedEventArgs e)
        {
            BeginUpdate();
        }

        /// <summary>
        /// 多个事件同时触发时，仅执行一次
        /// </summary>
        private void BeginUpdate()
        {
            delayUpdate = Update;

            this.Dispatcher.InvokeAsync(() =>
            {
                if (delayUpdate != null)
                {
                    delayUpdate.Invoke();
                    delayUpdate = null;
                }

            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void Update()
        {
            if (canvas == null || txt1 == null || txt2 == null)
                return;

            // 使水平居中
            var canvas_top = (canvas.RenderSize.Height - txt1.RenderSize.Height) / 2;
            Canvas.SetTop(txt1, canvas_top);
            Canvas.SetTop(txt2, canvas_top);

            // 复位
            Canvas.SetLeft(txt1, canvas.RenderSize.Width);
            Canvas.SetLeft(txt2, canvas.RenderSize.Width);
            // 停用动画
            txt1.BeginAnimation(Canvas.LeftProperty, null);
            txt2.BeginAnimation(Canvas.LeftProperty, null);

            // 当不可见时，不启用动画
            if (String.IsNullOrEmpty(Text) || !IsVisible)
                return;

            // 使用新动画
            var from = canvas.RenderSize.Width; // 起点位置
            var to = -txt1.RenderSize.Width;    // 终点位置
            var len = txt1.RenderSize.Width >= canvas.RenderSize.Width - Space ? txt1.RenderSize.Width + Space : canvas.RenderSize.Width;   // 加上间距的长度，同一时刻只能出现一条信息

            var begin = TimeSpan.FromSeconds(len / Speed);      // 第二个动画延迟时间
            var duration = TimeSpan.FromSeconds((from - to) / Speed);     // 动画从开始到结束的时间
            var total = begin + begin;      // 加上延迟，一次动画的时间

            var sb = new Storyboard();

            var ani1 = new DoubleAnimationUsingKeyFrames
            {
                Duration = total,
                RepeatBehavior = RepeatBehavior.Forever
            };

            ani1.KeyFrames.Add(new DiscreteDoubleKeyFrame(from, TimeSpan.FromSeconds(0)));
            ani1.KeyFrames.Add(new LinearDoubleKeyFrame(to, duration));

            Storyboard.SetTarget(ani1, txt1);
            Storyboard.SetTargetProperty(ani1, new PropertyPath(Canvas.LeftProperty));

            sb.Children.Add(ani1);

            var ani2 = new DoubleAnimationUsingKeyFrames()
            {
                BeginTime = begin,
                Duration = total,
                RepeatBehavior = RepeatBehavior.Forever
            };

            ani2.KeyFrames.Add(new DiscreteDoubleKeyFrame(from, TimeSpan.FromSeconds(0)));
            ani2.KeyFrames.Add(new LinearDoubleKeyFrame(to, duration));

            Storyboard.SetTarget(ani2, txt2);
            Storyboard.SetTargetProperty(ani2, new PropertyPath(Canvas.LeftProperty));

            sb.Children.Add(ani2);

            #region 不显示时隐藏，不造成性能浪费

            txt1.SetCurrentValue(VisibilityProperty, Visibility.Hidden);
            txt2.SetCurrentValue(VisibilityProperty, Visibility.Hidden);

            var ani11 = new ObjectAnimationUsingKeyFrames
            {
                Duration = total,
                RepeatBehavior = RepeatBehavior.Forever
            };

            ani11.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = Visibility.Visible, KeyTime = TimeSpan.FromSeconds(0) });
            ani11.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = Visibility.Hidden, KeyTime = duration });

            Storyboard.SetTarget(ani11, txt1);
            Storyboard.SetTargetProperty(ani11, new PropertyPath(VisibilityProperty));

            sb.Children.Add(ani11);

            var ani22 = new ObjectAnimationUsingKeyFrames
            {
                BeginTime = begin,
                Duration = total,
                RepeatBehavior = RepeatBehavior.Forever
            };

            ani22.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = Visibility.Visible, KeyTime = TimeSpan.FromSeconds(0) });
            ani22.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = Visibility.Hidden, KeyTime = duration });

            Storyboard.SetTarget(ani22, txt2);
            Storyboard.SetTargetProperty(ani22, new PropertyPath(VisibilityProperty));

            sb.Children.Add(ani22);

            #endregion

            sb.Begin();
        }

        #endregion

        #region Field

        private Canvas canvas;
        private TextBlock txt1;
        private TextBlock txt2;
        private Action delayUpdate;

        #endregion
    }
}
