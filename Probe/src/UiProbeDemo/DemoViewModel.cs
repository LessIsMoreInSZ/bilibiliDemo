using System.ComponentModel;

namespace UiProbeDemo
{
    /// <summary>演示用业务模型:与探针无关,Release 照常运行。</summary>
    public class DemoViewModel : INotifyPropertyChanged
    {
        private string _label = "实时值 50";
        private double _value = 50;

        public string Label { get => _label; set { _label = value; HasChanged(nameof(Label)); } }
        public double Value { get => _value; set { _value = value; Label = $"实时值 {value:F0}"; HasChanged(nameof(Value)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void HasChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}
