using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp.Controls
{
    /// <summary>
    /// 叫号表
    /// </summary>
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(CallTableItem))]
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_ItemsPresenter", Type = typeof(ItemsPresenter))]
    public sealed class CallTable : ItemsControl
    {
        #region DependencyProperty

        public static readonly DependencyProperty DesiredColumnWidthProperty = DependencyProperty.Register("DesiredColumnWidth", typeof(Double), typeof(CallTable),
            new PropertyMetadata(200d, new PropertyChangedCallback(OnDesiredColumnWidthPropertyChangedCallback)),
            new ValidateValueCallback(OnDesiredColumnWidthPropertyValidateValueCallback));

        private static void OnDesiredColumnWidthPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CallTable)d).BeginUpdate();
        }

        private static Boolean OnDesiredColumnWidthPropertyValidateValueCallback(Object value)
        {
            return ((Double)value) > 0;
        }

        /// <summary>
        /// 设计时列宽
        /// </summary>
        public Double DesiredColumnWidth { get => (Double)GetValue(DesiredColumnWidthProperty); set => SetValue(DesiredColumnWidthProperty, value); }

        public static readonly DependencyPropertyKey RenderColumnWidthProperty = DependencyProperty.RegisterReadOnly("RenderColumnWidth", typeof(Double), typeof(CallTable), new PropertyMetadata(0d));

        public Double RenderColumnWidth { get => (Double)GetValue(RenderColumnWidthProperty.DependencyProperty); }

        public static readonly DependencyProperty DesiredRowHeightProperty = CallQueue.DesiredRowHeightProperty.AddOwner(typeof(CallTable));

        /// <summary>
        /// 设计时行高
        /// </summary>
        public Double DesiredRowHeight { get => (Double)GetValue(DesiredRowHeightProperty); set => SetValue(DesiredRowHeightProperty, value); }


        public static readonly DependencyProperty HeadRowHeightProperty = CallQueue.HeadRowHeightProperty.AddOwner(typeof(CallTable));

        /// <summary>
        /// 第一行高度
        /// </summary>
        public Double HeadRowHeight { get => (Double)GetValue(HeadRowHeightProperty); set => SetValue(HeadRowHeightProperty, value); }


        public static readonly DependencyProperty NextRowHeightProperty = CallQueue.NextRowHeightProperty.AddOwner(typeof(CallTable));

        /// <summary>
        /// 第二行高度
        /// </summary>
        public Double NextRowHeight { get => (Double)GetValue(NextRowHeightProperty); set => SetValue(NextRowHeightProperty, value); }


        public static readonly DependencyProperty IntervalProperty = DependencyProperty.Register("Interval", typeof(UInt32), typeof(CallTable), new PropertyMetadata(15000u, new PropertyChangedCallback(OnIntervalPropertyChangedCallback)));

        private static void OnIntervalPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var callTable = (CallTable)d;

            if (callTable.timer != null)
                callTable.timer.Interval = TimeSpan.FromMilliseconds((UInt32)e.NewValue);
        }

        /// <summary>
        /// 切换页面时间间隔（毫秒）
        /// </summary>
        public UInt32 Interval { get => (UInt32)GetValue(IntervalProperty); set => SetValue(IntervalProperty, value); }

        public static readonly DependencyProperty AnimationDurationProperty = DependencyProperty.Register("AnimationDuration", typeof(UInt32), typeof(CallTable), new PropertyMetadata(1000u));

        /// <summary>
        /// 动画运行时间（毫秒）
        /// </summary>
        public UInt32 AnimationDuration { get => (UInt32)GetValue(AnimationDurationProperty); set => SetValue(AnimationDurationProperty, value); }

        #endregion

        static CallTable()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CallTable), new FrameworkPropertyMetadata(typeof(CallTable)));
        }

        public CallTable()
        {
            SetValue(RenderColumnWidthProperty, DesiredColumnWidth);

            IsVisibleChanged += delegate { BeginUpdate(); };

            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Interval) };
            timer.Tick += OnTick;
        }

        #region Method

        protected override Boolean IsItemItsOwnContainerOverride(Object item)
        {
            return item is CallTableItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CallTableItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, Object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            ((CallTableItem)element).Width = RenderColumnWidth;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (canvas != null)
                canvas.SizeChanged -= OnSizeChanged;
            if (itemsPresenter != null)
                itemsPresenter.SizeChanged -= OnSizeChanged;

            canvas = this.Template.FindName("PART_Canvas", this) as Canvas;
            itemsPresenter = this.Template.FindName("PART_ItemsPresenter", this) as ItemsPresenter;

            if (canvas != null)
                canvas.SizeChanged += OnSizeChanged;

            if (itemsPresenter != null)
            {
                itemsPresenter.SizeChanged += OnSizeChanged;
                Canvas.SetLeft(itemsPresenter, 0);
            }
        }

        private void OnSizeChanged(Object sender, SizeChangedEventArgs e)
        {
            BeginUpdate();
        }

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

            }, DispatcherPriority.Background);
        }

        private void Update()
        {
            if (canvas == null || itemsPresenter == null || !IsVisible)
            {
                if (timer.IsEnabled)
                    timer.Stop();

                return;
            }

            #region 计算实际列宽

            columnCount = (Int32)Math.Round(canvas.RenderSize.Width / DesiredColumnWidth);     // 列数
            var column_width = canvas.RenderSize.Width / columnCount;      // 平均列宽

            if (RenderColumnWidth != column_width)
            {
                // 更新列宽
                SetValue(RenderColumnWidthProperty, column_width);

                for (var i = 0; i < Items.Count; i++)
                {
                    var item = (CallTableItem)ItemContainerGenerator.ContainerFromIndex(i);

                    item.Width = column_width;
                    item.Visibility = i >= columnCount ? Visibility.Hidden : Visibility.Visible;   // 超出隐藏
                }

                itemsPresenter.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));    // 重新测量布局
            }

            #endregion

            #region 更新页数

            var page_count = (Int32)Math.Ceiling(itemsPresenter.DesiredSize.Width / canvas.RenderSize.Width);   // 页数

            if (pageCount != page_count)
            {
                pageCount = page_count;

                // 当前页超过页数，跳转到第一页
                if (pageIndex >= pageCount)
                    SetPageIndex(0);    // 矫正位置

                if (pageCount <= 1)
                {
                    // 页数小于1时，停止定时器
                    if (timer.IsEnabled)
                        timer.Stop();
                }
                else if (!timer.IsEnabled)
                    timer.Start();
            }
            else
                SetPageIndex(pageIndex);    // 矫正位置

            #endregion
        }

        private void SetPageIndex(Int32 index)
        {
            var lastIndex = pageIndex;
            pageIndex = index;

            // 仅显示当前页和下一页
            for (var i = 0; i < Items.Count; i++)
            {
                var item = (CallTableItem)ItemContainerGenerator.ContainerFromIndex(i);
                var itemIndex = i / columnCount;
                item.Visibility = (itemIndex == lastIndex || itemIndex == index) ? Visibility.Visible : Visibility.Hidden;
            }

            var duration = TimeSpan.FromMilliseconds(AnimationDuration);
            var ease = new CircleEase { EasingMode = EasingMode.EaseOut };
            //IEasingFunction ease = null;

            if (index == 0)
            {
                // 切换到第一页
                if (pageCount == 0)
                {
                    // 停止动画
                    itemsPresenter.BeginAnimation(Canvas.LeftProperty, null);
                }
                else if (lastIndex != 0)
                {
                    // 插入镜像实现轮播效果
                    var rec = new Rectangle
                    {
                        Width = itemsPresenter.RenderSize.Width,
                        Height = canvas.RenderSize.Height,
                        Fill = new VisualBrush(itemsPresenter) { Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top }
                    };

                    Canvas.SetLeft(rec, canvas.RenderSize.Width);

                    canvas.Children.Add(rec);

                    var sb = new Storyboard();

                    var ani1 = new DoubleAnimation
                    {
                        To = -pageCount * canvas.RenderSize.Width,
                        Duration = duration,
                        EasingFunction = ease
                    };

                    Storyboard.SetTarget(ani1, itemsPresenter);
                    Storyboard.SetTargetProperty(ani1, new PropertyPath(Canvas.LeftProperty));

                    sb.Children.Add(ani1);

                    var ani2 = new DoubleAnimation
                    {
                        To = 0,
                        Duration = duration,
                        EasingFunction = ease
                    };

                    Storyboard.SetTarget(ani2, rec);
                    Storyboard.SetTargetProperty(ani2, new PropertyPath(Canvas.LeftProperty));

                    sb.Children.Add(ani2);

                    // 动画完成后执行该事件
                    EventHandler handler = null;
                    handler = (s, e) =>
                    {
                        sb.Completed -= handler;

                        Canvas.SetLeft(itemsPresenter, 0);
                        itemsPresenter.BeginAnimation(Canvas.LeftProperty, null);

                        canvas.Children.Remove(rec);

                        // 仅显示当前页
                        for (var i = 0; i < Items.Count; i++)
                        {
                            var item = (CallTableItem)ItemContainerGenerator.ContainerFromIndex(i);
                            var itemIndex = i / columnCount;
                            item.Visibility = itemIndex == index ? Visibility.Visible : Visibility.Hidden;
                        }
                    };

                    sb.Completed += handler;
                    sb.Begin();
                }
            }
            else
            {
                // 切换到下一页
                var left = -pageIndex * canvas.RenderSize.Width;

                var ani = new DoubleAnimation
                {
                    To = left,
                    Duration = duration,
                    EasingFunction = ease
                };

                // 动画完成后执行该事件
                EventHandler handler = null;
                handler = (s, e) =>
                {
                    ani.Completed -= handler;

                    // 仅显示当前页
                    for (var i = 0; i < Items.Count; i++)
                    {
                        var item = (CallTableItem)ItemContainerGenerator.ContainerFromIndex(i);
                        var itemIndex = i / columnCount;
                        item.Visibility = itemIndex == index ? Visibility.Visible : Visibility.Hidden;
                    }
                };

                ani.Completed += handler;
                itemsPresenter.BeginAnimation(Canvas.LeftProperty, ani, HandoffBehavior.SnapshotAndReplace);    // 更新位置
            }
        }

        private void OnTick(Object sender, EventArgs e)
        {
            var nextPage = (pageIndex + 1) % pageCount;
            SetPageIndex(nextPage);
        }

        #endregion

        #region Field

        private Canvas canvas;
        private ItemsPresenter itemsPresenter;
        private Action delayUpdate;
        private Int32 pageCount;        // 页数
        private Int32 columnCount;      // 一页多少项
        private Int32 pageIndex;        // 第几页
        private DispatcherTimer timer;      // 定时刷新页面

        #endregion
    }

    public sealed class CallTableItem : ContentControl
    {
        static CallTableItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CallTableItem), new FrameworkPropertyMetadata(typeof(CallTableItem)));
        }
    }
}
