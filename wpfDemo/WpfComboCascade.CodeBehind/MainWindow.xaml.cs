using System.Windows;
using System.Windows.Controls;

namespace WpfComboCascade.CodeBehind;

public partial class MainWindow : Window
{
    private readonly int[] _allOptions = Enumerable.Range(1, 6).ToArray();
    private readonly int?[] _selections = new int?[6];
    private readonly bool[] _manualSelections = new bool[6];
    private ComboBox[] _comboBoxes = Array.Empty<ComboBox>();
    private bool _isApplyingState;

    public MainWindow()
    {
        InitializeComponent();

        _comboBoxes = new[] { ComboBox1, ComboBox2, ComboBox3, ComboBox4, ComboBox5, ComboBox6 };
        ApplyStateToUi();
    }

    private void ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isApplyingState || sender is not ComboBox changedComboBox)
        {
            return;
        }

        var index = Array.IndexOf(_comboBoxes, changedComboBox);
        if (index < 0)
        {
            return;
        }

        var selectedValue = changedComboBox.SelectedItem is int value ? value : (int?)null;
        HandleUserSelection(index, selectedValue);
    }

    private void HandleUserSelection(int index, int? selectedValue)
    {
        if (index == 0)
        {
            ApplyCascadeFromFirst(selectedValue);
        }
        else
        {
            ApplyManualSelection(index, selectedValue);
        }

        ApplyStateToUi();
    }

    private void ApplyCascadeFromFirst(int? firstValue)
    {
        ResetState();

        if (!firstValue.HasValue)
        {
            return;
        }

        _selections[0] = firstValue.Value;
        _manualSelections[0] = true;

        for (var index = 1; index < _selections.Length; index++)
        {
            var nextValue = firstValue.Value + index;
            _selections[index] = nextValue <= 6 ? nextValue : null;
        }
    }

    private void ApplyManualSelection(int index, int? selectedValue)
    {
        _selections[index] = selectedValue;
        _manualSelections[index] = selectedValue.HasValue;

        if (!selectedValue.HasValue)
        {
            RemoveInvalidAutoSelections();
            return;
        }

        var duplicatedManualSelectionExists = Enumerable.Range(0, _selections.Length)
            .Any(otherIndex => otherIndex != index
                && _manualSelections[otherIndex]
                && _selections[otherIndex] == selectedValue);

        if (duplicatedManualSelectionExists)
        {
            _selections[index] = null;
            _manualSelections[index] = false;
            return;
        }

        RemoveInvalidAutoSelections();
    }

    private void RemoveInvalidAutoSelections()
    {
        var manualValues = _manualSelections
            .Select((isManual, index) => new { isManual, index })
            .Where(item => item.isManual && _selections[item.index].HasValue)
            .Select(item => _selections[item.index]!.Value)
            .ToHashSet();

        for (var index = 0; index < _selections.Length; index++)
        {
            if (_manualSelections[index])
            {
                continue;
            }

            if (_selections[index].HasValue && manualValues.Contains(_selections[index]!.Value))
            {
                _selections[index] = null;
            }
        }
    }

    private void ResetState()
    {
        for (var index = 0; index < _selections.Length; index++)
        {
            _selections[index] = null;
            _manualSelections[index] = false;
        }
    }

    private void ApplyStateToUi()
    {
        _isApplyingState = true;

        try
        {
            for (var index = 0; index < _comboBoxes.Length; index++)
            {
                _comboBoxes[index].ItemsSource = BuildOptionsFor(index);
                _comboBoxes[index].SelectedItem = _selections[index];
            }
        }
        finally
        {
            _isApplyingState = false;
        }
    }

    private List<int> BuildOptionsFor(int index)
    {
        var excludedValues = Enumerable.Range(0, _selections.Length)
            .Where(otherIndex => otherIndex != index && _manualSelections[otherIndex] && _selections[otherIndex].HasValue)
            .Select(otherIndex => _selections[otherIndex]!.Value)
            .ToHashSet();

        return _allOptions.Where(option => !excludedValues.Contains(option)).ToList();
    }
}
