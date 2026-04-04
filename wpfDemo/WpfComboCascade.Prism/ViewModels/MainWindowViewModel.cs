using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace WpfComboCascade.PrismDemo.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private readonly int[] _allOptions = Enumerable.Range(1, 6).ToArray();
    private readonly int?[] _selections = new int?[6];
    private readonly bool[] _manualSelections = new bool[6];
    private ObservableCollection<int>[] _optionCollections = Array.Empty<ObservableCollection<int>>();
    private int? _firstSelection;
    private int? _secondSelection;
    private int? _thirdSelection;
    private int? _fourthSelection;
    private int? _fifthSelection;
    private int? _sixthSelection;
    private bool _isApplyingState;

    public MainWindowViewModel()
    {
        FirstOptions = new ObservableCollection<int>();
        SecondOptions = new ObservableCollection<int>();
        ThirdOptions = new ObservableCollection<int>();
        FourthOptions = new ObservableCollection<int>();
        FifthOptions = new ObservableCollection<int>();
        SixthOptions = new ObservableCollection<int>();

        _optionCollections = new[]
        {
            FirstOptions,
            SecondOptions,
            ThirdOptions,
            FourthOptions,
            FifthOptions,
            SixthOptions
        };

        ApplyStateToViewModel();
    }

    public ObservableCollection<int> FirstOptions { get; }
    public ObservableCollection<int> SecondOptions { get; }
    public ObservableCollection<int> ThirdOptions { get; }
    public ObservableCollection<int> FourthOptions { get; }
    public ObservableCollection<int> FifthOptions { get; }
    public ObservableCollection<int> SixthOptions { get; }

    public int? FirstSelection
    {
        get => _firstSelection;
        set
        {
            if (SetProperty(ref _firstSelection, value) && !_isApplyingState)
            {
                HandleUserSelection(0, value);
            }
        }
    }

    public int? SecondSelection
    {
        get => _secondSelection;
        set
        {
            if (SetProperty(ref _secondSelection, value) && !_isApplyingState)
            {
                HandleUserSelection(1, value);
            }
        }
    }

    public int? ThirdSelection
    {
        get => _thirdSelection;
        set
        {
            if (SetProperty(ref _thirdSelection, value) && !_isApplyingState)
            {
                HandleUserSelection(2, value);
            }
        }
    }

    public int? FourthSelection
    {
        get => _fourthSelection;
        set
        {
            if (SetProperty(ref _fourthSelection, value) && !_isApplyingState)
            {
                HandleUserSelection(3, value);
            }
        }
    }

    public int? FifthSelection
    {
        get => _fifthSelection;
        set
        {
            if (SetProperty(ref _fifthSelection, value) && !_isApplyingState)
            {
                HandleUserSelection(4, value);
            }
        }
    }

    public int? SixthSelection
    {
        get => _sixthSelection;
        set
        {
            if (SetProperty(ref _sixthSelection, value) && !_isApplyingState)
            {
                HandleUserSelection(5, value);
            }
        }
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

        ApplyStateToViewModel();
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

    private void ApplyStateToViewModel()
    {
        _isApplyingState = true;

        try
        {
            SetProperty(ref _firstSelection, _selections[0], nameof(FirstSelection));
            SetProperty(ref _secondSelection, _selections[1], nameof(SecondSelection));
            SetProperty(ref _thirdSelection, _selections[2], nameof(ThirdSelection));
            SetProperty(ref _fourthSelection, _selections[3], nameof(FourthSelection));
            SetProperty(ref _fifthSelection, _selections[4], nameof(FifthSelection));
            SetProperty(ref _sixthSelection, _selections[5], nameof(SixthSelection));

            for (var index = 0; index < _optionCollections.Length; index++)
            {
                ReplaceOptions(_optionCollections[index], BuildOptionsFor(index));
            }
        }
        finally
        {
            _isApplyingState = false;
        }
    }

    private IReadOnlyList<int> BuildOptionsFor(int index)
    {
        var excludedValues = Enumerable.Range(0, _selections.Length)
            .Where(otherIndex => otherIndex != index && _manualSelections[otherIndex] && _selections[otherIndex].HasValue)
            .Select(otherIndex => _selections[otherIndex]!.Value)
            .ToHashSet();

        return _allOptions.Where(option => !excludedValues.Contains(option)).ToArray();
    }

    private static void ReplaceOptions(ObservableCollection<int> target, IReadOnlyList<int> values)
    {
        target.Clear();
        foreach (var value in values)
        {
            target.Add(value);
        }
    }
}
