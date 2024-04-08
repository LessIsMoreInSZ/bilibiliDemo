using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFCustomChart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public VisualHost1 _visualHost;
        double scale;
        public MainWindow()
        {
            InitializeComponent();

            _visualHost = new VisualHost1();
            DrawingContainer.Children.Add(_visualHost);

            double canvasWidth = DrawingContainer.Width;
            double canvasHeight = DrawingContainer.Height;
            //double canvasWidth = 3000;
            //double canvasHeight = 3000;


            //new SelectBoxRect(DrawingContainer);

            //using (new PerformanceCounter("ghfgh"))
            using (new PerformanceCounter())
            {
                DrawRectangles1(canvasWidth, canvasHeight);
            }
            scale = 1;
        }

        private void Window_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (e.Delta < 0)
                {
                    scale -= 0.01;
                }
                else
                {
                    scale += 0.01;
                }
                // scale += (double)e.Delta / 35000;
                ScaleTransform transfrom = new ScaleTransform();
                transfrom.ScaleX = transfrom.ScaleY = scale;
                this.DrawingContainer.RenderTransform = transfrom;
            }
        }

        private Pen CreateAndFreezePen()
        {
            // 创建Pen
            Pen pen = new Pen(Brushes.Black, 1);

            // 冻结Pen
            if (pen.CanFreeze)
            {
                pen.Freeze();
            }

            return pen;
        }

        private void DrawRectangles1(double canvasWidth, double canvasHeight)
        {
            int rows = 100; // 纵向矩形数量
            int columns = 100; // 横向矩形数量


            using (DrawingContext drawingContext = _visualHost.RenderOpen())
            {

                double rectangleWidth = canvasWidth / columns;
                double rectangleHeight = canvasHeight / rows;

                var pen = CreateAndFreezePen();
                for (int i = 0; i < columns; i++)
                {
                    for (int j = 0; j < rows; j++)
                    {
                        double x = i * rectangleWidth;
                        double y = j * rectangleHeight;

                        // 创建矩形几何图形
                        Rect rectangleRect = new Rect(new Point(x, y), new Size(rectangleWidth, rectangleHeight));
                        Geometry rectangleGeometry = new RectangleGeometry(rectangleRect);

                        // 绘制矩形
                        //drawingContext.DrawGeometry(Brushes.Blue, new Pen(Brushes.Black, 1), rectangleGeometry);
                        drawingContext.DrawGeometry(Brushes.Blue, pen, rectangleGeometry);

                    }
                }
            }
        }
    }

    public class VisualHost1 : FrameworkElement
    {
        private readonly DrawingVisual _drawingVisual;

        public VisualHost1()
        {
            _drawingVisual = new DrawingVisual();
        }

        public DrawingContext RenderOpen()
        {
            return _drawingVisual.RenderOpen();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawDrawing(_drawingVisual.Drawing);
        }
    }
}