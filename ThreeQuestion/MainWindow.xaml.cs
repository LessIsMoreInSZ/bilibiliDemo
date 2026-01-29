using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ThreeQuestion
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private bool _isChiming = false;

        public MainWindow()
        {
            InitializeComponent();
            DrawTicks();
            DrawNumbers();
            StartClock();
        }

        private void DrawNumbers()
        {
            // Center is (200, 200).
            // Radius for numbers. Ticks at 180ish. Numbers should be inside that. Say 155.
            double radius = 155;
            double centerX = 200;
            double centerY = 200;

            for (int i = 1; i <= 12; i++)
            {
                TextBlock tb = new TextBlock();
                tb.Text = i.ToString();
                tb.FontSize = 24;
                tb.FontFamily = new FontFamily("Times New Roman"); // Classic serif
                tb.FontWeight = FontWeights.Bold;
                tb.Foreground = new SolidColorBrush(Color.FromRgb(212, 175, 55)); // Gold
                
                // Add a slight shadow to make it pop against dark bg
                tb.Effect = new System.Windows.Media.Effects.DropShadowEffect() 
                { 
                    Color = Colors.Black, 
                    BlurRadius = 2, 
                    ShadowDepth = 1,
                    Opacity = 0.8
                };

                // Measure size to center perfectly
                tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double width = tb.DesiredSize.Width;
                double height = tb.DesiredSize.Height;

                // Angle in DEGREES. 
                // 12 is -90, 1 is -60, 2 is -30, 3 is 0.
                // Formula: angle = (i * 30) - 90
                double angleDeg = (i * 30) - 90;
                double angleRad = angleDeg * (Math.PI / 180.0);

                double x = centerX + radius * Math.Cos(angleRad) - (width / 2);
                double y = centerY + radius * Math.Sin(angleRad) - (height / 2);

                Canvas.SetLeft(tb, x);
                Canvas.SetTop(tb, y);
                NumbersCanvas.Children.Add(tb);
            }
        }

        private void DrawTicks()
        {
            for (int i = 0; i < 60; i++)
            {
                Rectangle tick = new Rectangle();
                if (i % 5 == 0)
                {
                    // Hour tick - Gold
                    tick.Width = 4;
                    tick.Height = 12;
                    tick.Fill = new SolidColorBrush(Color.FromRgb(212, 175, 55)); // Gold
                }
                else
                {
                    // Minute tick - Darker/Subtle
                    tick.Width = 1;
                    tick.Height = 6;
                    tick.Fill = new SolidColorBrush(Color.FromRgb(150, 150, 150)); 
                }

                TransformGroup tg = new TransformGroup();
                // Center the tick rectangle itself
                tg.Children.Add(new TranslateTransform(-tick.Width / 2, -tick.Height / 2));
                // Move up to the edge (Radius approx 180)
                tg.Children.Add(new TranslateTransform(0, -180));
                // Rotate
                tg.Children.Add(new RotateTransform(i * 6));
                // Move to center of canvas (200, 200)
                tg.Children.Add(new TranslateTransform(200, 200));

                tick.RenderTransform = tg;
                TicksCanvas.Children.Add(tick);
            }
        }

        private void StartClock()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            UpdateHands(); // Initial update
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            UpdateHands();
        }

        private void UpdateHands()
        {
            DateTime now = DateTime.Now;

            // Hour hand
            double hourAngle = ((now.Hour % 12) + now.Minute / 60.0) * 30;
            HourHandTransform.Angle = hourAngle;

            // Minute hand
            double minuteAngle = (now.Minute + now.Second / 60.0) * 6;
            MinuteHandTransform.Angle = minuteAngle;

            // Second hand
            double secondAngle = now.Second * 6;
            SecondHandTransform.Angle = secondAngle;
        }

        private async void OnMinuteRepeaterClick(object sender, RoutedEventArgs e)
        {
            if (_isChiming) return;
            _isChiming = true;
            StatusText.Text = "Listening...";

            try
            {
                await Task.Run(() => PlayMinuteRepeater());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                _isChiming = false;
                StatusText.Text = "Ready";
            }
        }

        private void PlayMinuteRepeater()
        {
            DateTime now = DateTime.Now;
            
            // 1. Calculate Tri-Question Components
            
            // Hours (1-12)
            int hours = now.Hour % 12;
            if (hours == 0) hours = 12;

            // Quarters (0-3)
            int quarters = now.Minute / 15;

            // Minutes past quarter (0-14)
            int minutesPast = now.Minute % 15;

            // 2. Play Sounds report
            
            // Hours: Low Tone
            UpdateStatusSafe($"Hours: {hours}");
            for (int i = 0; i < hours; i++)
            {
                BeepLow();
                Thread.Sleep(600); // Slow pace
            }

            if (quarters > 0 || minutesPast > 0) Thread.Sleep(800); // Pause between groups

            // Quarters: Ding-Dong (High-Low)
            UpdateStatusSafe($"Quarters: {quarters}");
            for (int i = 0; i < quarters; i++)
            {
                BeepHigh(); // Ding
                Thread.Sleep(80); // Very short gap
                BeepLow();  // Dong
                Thread.Sleep(500); 
            }

            if (minutesPast > 0) Thread.Sleep(800);

            // Minutes: High Tone
            UpdateStatusSafe($"Minutes: {minutesPast}");
            for (int i = 0; i < minutesPast; i++)
            {
                BeepHigh();
                Thread.Sleep(300); // Faster pace
            }
            
            UpdateStatusSafe("Done");
            Thread.Sleep(1000);
        }

        private void BeepLow()
        {
            if (OperatingSystem.IsWindows())
            {
                try { Console.Beep(300, 200); } catch { }
            }
        }

        private void BeepHigh()
        {
             if (OperatingSystem.IsWindows())
             {
                 try { Console.Beep(600, 150); } catch { }
             }
        }

        private void UpdateStatusSafe(string text)
        {
            Dispatcher.Invoke(() => StatusText.Text = text);
        }
    }
}