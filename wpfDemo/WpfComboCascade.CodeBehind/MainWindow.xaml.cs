using System.Windows;
using System.Windows.Controls;

namespace WpfComboCascade.CodeBehind;
/// <summary>
/// Todo:下拉框没有源
/// </summary>
public partial class MainWindow : Window
{
    private readonly List<int> _options = Enumerable.Range(1, 6).ToList();
    private readonly ComboBox[] _dependentComboBoxes;

    public MainWindow()
    {
        InitializeComponent();

        _dependentComboBoxes = new[] { ComboBox2, ComboBox3, ComboBox4, ComboBox5, ComboBox6 };
        InitializeComboBoxes();
    }

    private void InitializeComboBoxes()
    {
        ComboBox1.ItemsSource = _options;
        ComboBox1.SelectedIndex = 0;

        foreach (var comboBox in _dependentComboBoxes)
        {
            comboBox.ItemsSource = _options;
        }

        UpdateDependentSelections(ComboBox1.SelectedItem as int?);
    }

    private void ComboBox1_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateDependentSelections(ComboBox1.SelectedItem as int?);
    }

    private void UpdateDependentSelections(int? startValue)
    {
        if (!startValue.HasValue)
        {
            foreach (var comboBox in _dependentComboBoxes)
            {
                comboBox.SelectedItem = null;
            }

            return;
        }

        for (var index = 0; index < _dependentComboBoxes.Length; index++)
        {
            var nextValue = startValue.Value + index + 1;
            _dependentComboBoxes[index].SelectedItem = nextValue <= 6 ? nextValue : null;
        }
    }
}
