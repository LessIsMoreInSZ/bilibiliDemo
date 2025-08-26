using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp.Controls
{
    /// <summary>
    /// 叫号队列
    /// </summary>
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(CallQueueItem))]
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_ItemsPresenter", Type = typeof(ItemsPresenter))]

    public sealed class CallQueue : ItemsControl
    {
        #region DependencyProperty

        public static readonly DependencyProperty DesiredRowHeightProperty = DependencyProperty.RegisterAttached("DesiredRowHeight", typeof(Double), typeof(CallQueue),
            new FrameworkPropertyMetadata(40d, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDesiredRowHeightPropertyChangedCallback)),
            new ValidateValueCallback(OnDesiredRowHeightPropertyValidateValueCallback));

        private static void OnDesiredRowHeightPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CallQueue queue)
                queue.BeginUpdate();
        }

        private static Boolean OnDesiredRowHeightPropertyValidateValueCallback(Object value)
        {
            return ((Double)value) > 0;
        }

        /// <summary>
        /// 设计时行高
        /// </summary>
        public Double DesiredRowHeight { get => (Double)GetValue(DesiredRowHeightProperty); set => SetValue(DesiredRowHeightProperty, value); }

        public static readonly DependencyPropertyKey RenderRowHeightProperty = DependencyProperty.RegisterReadOnly("RenderRowHeight", typeof(Double), typeof(CallQueue), new PropertyMetadata(0d));
        /// <summary>
        /// 渲染时行高
        /// </summary>
        public Double RenderRowHeight { get => (Double)GetValue(RenderRowHeightProperty.DependencyProperty); }

        public static readonly DependencyProperty HeadRowHeightProperty = DependencyProperty.RegisterAttached("HeadRowHeight", typeof(Double), typeof(CallQueue),
            new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnHeadRowHeightPropertyChangedCallback)),
            new ValidateValueCallback(OnHeadRowHeightPropertyValidateValueCallback));

        private static void OnHeadRowHeightPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CallQueue queue)
                queue.BeginUpdate();
        }

        private static Boolean OnHeadRowHeightPropertyValidateValueCallback(Object value)
        {
            var d = (Double)value;
            return Double.IsNaN(d) || d > 0;
        }

        /// <summary>
        /// 第一行高度
        /// </summary>
        public Double HeadRowHeight { get => (Double)GetValue(HeadRowHeightProperty); set => SetValue(HeadRowHeightProperty, value); }


        public static readonly DependencyProperty NextRowHeightProperty = DependencyProperty.RegisterAttached("NextRowHeight", typeof(Double), typeof(CallQueue),
            new FrameworkPropertyMetadata(Double.NaN, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnNextRowHeightPropertyChangedCallback)),
            new ValidateValueCallback(OnNextRowHeightPropertyValidateValueCallback));

        private static void OnNextRowHeightPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CallQueue queue)
                queue.BeginUpdate();
        }

        private static Boolean OnNextRowHeightPropertyValidateValueCallback(Object value)
        {
            var d = (Double)value;
            return Double.IsNaN(d) || d > 0;
        }

        /// <summary>
        /// 第二行高度
        /// </summary>
        public Double NextRowHeight { get => (Double)GetValue(NextRowHeightProperty); set => SetValue(NextRowHeightProperty, value); }

        public static readonly DependencyPropertyKey LastVisibleRowsCountProperty = DependencyProperty.RegisterReadOnly("LastVisibleRowsCount", typeof(UInt32), typeof(CallQueue), new PropertyMetadata(0u));

        /// <summary>
        /// 剩余可见行数
        /// </summary>
        public UInt32 LastVisibleRowsCount { get => (UInt32)GetValue(LastVisibleRowsCountProperty.DependencyProperty); }

        #endregion

        static CallQueue()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CallQueue), new FrameworkPropertyMetadata(typeof(CallQueue)));
        }

        public CallQueue()
        {
            SetValue(RenderRowHeightProperty, DesiredRowHeight);
        }

        #region Method

        protected override Boolean IsItemItsOwnContainerOverride(Object item)
        {
            return item is CallQueueItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CallQueueItem();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, Object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is CallQueueItem cell)
            {
                var index = Items.IndexOf(item);

                if (index == 0)
                {
                    cell.IsHead = true;
                    if (!Double.IsNaN(HeadRowHeight))
                        cell.Height = HeadRowHeight;
                    else
                        cell.Height = RenderRowHeight;
                }
                else if (index == 1)
                {
                    cell.IsNext = true;
                    if (!Double.IsNaN(NextRowHeight))
                        cell.Height = NextRowHeight;
                    else
                        cell.Height = RenderRowHeight;
                }
                else
                    cell.Height = RenderRowHeight;
            }
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
                itemsPresenter.SizeChanged += OnSizeChanged;
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

            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void Update()
        {
            if (itemsPresenter == null)
                return;

            var last_height = this.RenderSize.Height;   // 剩余可见高度

            if (!Double.IsNaN(HeadRowHeight))
                last_height -= HeadRowHeight;
            if (!Double.IsNaN(NextRowHeight))
                last_height -= NextRowHeight;

            var last_row_count = 0u;    // 剩余可见行数
            var render_row_height = 0d;

            if (last_height <= 0)
                render_row_height = 0;
            else
            {
                last_row_count = (UInt32)Math.Round(last_height / DesiredRowHeight);
                render_row_height = last_height / last_row_count;
            }

            if (Double.IsNaN(HeadRowHeight))
                last_row_count--;
            if (Double.IsNaN(NextRowHeight))
                last_row_count--;

            if (LastVisibleRowsCount != last_row_count)
                SetValue(LastVisibleRowsCountProperty, last_row_count);
            if (RenderRowHeight != render_row_height)
                SetValue(RenderRowHeightProperty, render_row_height);

            for (var i = 0; i < Items.Count; i++)
            {
                var item = (CallQueueItem)ItemContainerGenerator.ContainerFromIndex(i);

                if (i == 0)
                {
                    item.IsHead = true;
                    item.IsNext = false;

                    if (!Double.IsNaN(HeadRowHeight))
                        item.Height = HeadRowHeight;
                    else
                        item.Height = render_row_height;
                }
                else if (i == 1)
                {
                    item.IsHead = false;
                    item.IsNext = true;

                    if (!Double.IsNaN(NextRowHeight))
                        item.Height = NextRowHeight;
                    else
                        item.Height = render_row_height;
                }
                else
                {
                    item.IsHead = false;
                    item.IsNext = false;
                    item.Height = render_row_height;

                    item.Visibility = i - 2 >= last_row_count ? Visibility.Collapsed : Visibility.Visible;      // 不显示时隐藏且不占用空间
                }
            }
        }

        #endregion

        #region Field

        private Canvas canvas;
        private ItemsPresenter itemsPresenter;
        private Action delayUpdate;

        #endregion
    }

    public sealed class CallQueueItem : ContentControl
    {
        #region

        public static readonly DependencyProperty IsHeadProperty = DependencyProperty.Register("IsHead", typeof(Boolean), typeof(CallQueueItem));
        /// <summary>
        /// 是否队列头
        /// </summary>
        public Boolean IsHead { get => (Boolean)GetValue(IsHeadProperty); set => SetValue(IsHeadProperty, value); }

        public static readonly DependencyProperty IsNextProperty = DependencyProperty.Register("IsNext", typeof(Boolean), typeof(CallQueueItem));
        /// <summary>
        /// 是否下一位
        /// </summary>
        public Boolean IsNext { get => (Boolean)GetValue(IsNextProperty); set => SetValue(IsNextProperty, value); }

        #endregion

        static CallQueueItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CallQueueItem), new FrameworkPropertyMetadata(typeof(CallQueueItem)));
        }
    }
}
