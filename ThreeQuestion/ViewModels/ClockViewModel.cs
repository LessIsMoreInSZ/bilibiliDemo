using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ThreeQuestion.ViewModels
{
    public class ClockViewModel : INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private double _hourAngle;
        private double _minuteAngle;
        private double _secondAngle;
        private string _statusText = "Ready";
        private bool _isChiming;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<TickItem> Ticks { get; } = new();
        public ObservableCollection<NumberItem> Numbers { get; } = new();

        public double HourAngle
        {
            get => _hourAngle;
            set { _hourAngle = value; OnPropertyChanged(); }
        }

        public double MinuteAngle
        {
            get => _minuteAngle;
            set { _minuteAngle = value; OnPropertyChanged(); }
        }

        public double SecondAngle
        {
            get => _secondAngle;
            set { _secondAngle = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public RelayCommand MinuteRepeaterCommand { get; }

        public ClockViewModel()
        {
            GenerateTicks();
            GenerateNumbers();
            
            MinuteRepeaterCommand = new RelayCommand(async () => await PlayMinuteRepeater(), () => !_isChiming);

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => UpdateTime();
            _timer.Start();
            UpdateTime();
        }

        private void UpdateTime()
        {
            DateTime now = DateTime.Now;
            HourAngle = ((now.Hour % 12) + now.Minute / 60.0) * 30;
            MinuteAngle = (now.Minute + now.Second / 60.0) * 6;
            SecondAngle = now.Second * 6;
        }

        private void GenerateTicks()
        {
            for (int i = 0; i < 60; i++)
            {
                bool isHour = i % 5 == 0;
                Ticks.Add(new TickItem
                {
                    Angle = i * 6,
                    Width = isHour ? 4 : 1,
                    Height = isHour ? 12 : 6,
                    // Center the tick: Translate -W/2, -H/2. Then Translate Up -180.
                    // We can do this in View via Binding, but simpler to expose properties.
                    // Actually, if we use ItemsControl with Canvas, we can bind RenderTransform.
                    Color = isHour ? "#D4AF37" : "#969696"
                });
            }
        }

        private void GenerateNumbers()
        {
            double radius = 155;
            double centerX = 200;
            double centerY = 200;

            for (int i = 1; i <= 12; i++)
            {
                // Measure size is unknown here in VM, so we can't perfectly center-align by subtracting Width/2.
                // However, we can position the TopLeft at the calculated point.
                // A better trick in XAML is to center the TextBlock's RenderTransform or use a Canvas that centers content.
                // We will calculate the CENTER point here, and let XAML handle the alignment (Margin= -Width/2 ...) 
                // Wait, we can't bind Margin to ActualWidth easily.
                // Standard approach: Bind Canvas.Left/Top to a value.
                
                double angleDeg = (i * 30) - 90;
                double angleRad = angleDeg * (Math.PI / 180.0);
                
                // We'll provide the Center Point (X,Y) for the number.
                // The View will be responsible for valid centering (e.g. RenderTransform Translate -50%, -50%).
                double x = centerX + radius * Math.Cos(angleRad);
                double y = centerY + radius * Math.Sin(angleRad);

                Numbers.Add(new NumberItem
                {
                    Text = i.ToString(),
                    X = x,
                    Y = y
                });
            }
        }

        private async Task PlayMinuteRepeater()
        {
            if (_isChiming) return;
            _isChiming = true;
            StatusText = "Listening...";
            
            // Force command re-evaluation
            CommandManager.InvalidateRequerySuggested();

            try
            {
                await Task.Run(() =>
                {
                    DateTime now = DateTime.Now;
                    int hours = now.Hour % 12;
                    if (hours == 0) hours = 12;
                    int quarters = now.Minute / 15;
                    int minutesPast = now.Minute % 15;

                    UpdateStatus($"Hours: {hours}");
                    for (int i = 0; i < hours; i++)
                    {
                        BeepLow();
                        Thread.Sleep(600);
                    }

                    if (quarters > 0 || minutesPast > 0) Thread.Sleep(800);

                    UpdateStatus($"Quarters: {quarters}");
                    for (int i = 0; i < quarters; i++)
                    {
                        BeepHigh(); Thread.Sleep(80);
                        BeepLow(); Thread.Sleep(500);
                    }

                    if (minutesPast > 0) Thread.Sleep(800);

                    UpdateStatus($"Minutes: {minutesPast}");
                    for (int i = 0; i < minutesPast; i++)
                    {
                        BeepHigh();
                        Thread.Sleep(300);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _isChiming = false;
                StatusText = "Ready";
                // Need to dispatch property change or command update on UI thread? 
                // PropertyChanged is marshaled if bound, but CommandManager isn't.
                Application.Current.Dispatcher.Invoke(CommandManager.InvalidateRequerySuggested);
            }
        }

        private void UpdateStatus(string text)
        {
            StatusText = text;
        }

        private void BeepLow()
        {
            if (OperatingSystem.IsWindows()) try { Console.Beep(300, 200); } catch { }
        }

        private void BeepHigh()
        {
            if (OperatingSystem.IsWindows()) try { Console.Beep(600, 150); } catch { }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class TickItem
    {
        public double Angle { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Color { get; set; } = "Black";
    }

    public class NumberItem
    {
        public string Text { get; set; } = "";
        public double X { get; set; }
        public double Y { get; set; }
    }
}
