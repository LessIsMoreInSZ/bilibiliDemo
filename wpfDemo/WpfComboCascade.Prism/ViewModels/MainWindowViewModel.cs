using Prism.Mvvm;

namespace WpfComboCascade.PrismDemo.ViewModels;

public class MainWindowViewModel : BindableBase
{
    private int? _firstSelection;
    private int? _secondSelection;
    private int? _thirdSelection;
    private int? _fourthSelection;
    private int? _fifthSelection;
    private int? _sixthSelection;

    public MainWindowViewModel()
    {
        NumberOptions = Enumerable.Range(1, 6).ToArray();
        FirstSelection = 1;
    }

    public IReadOnlyList<int> NumberOptions { get; }

    public int? FirstSelection
    {
        get => _firstSelection;
        set
        {
            if (SetProperty(ref _firstSelection, value))
            {
                UpdateSelections();
            }
        }
    }

    public int? SecondSelection
    {
        get => _secondSelection;
        private set => SetProperty(ref _secondSelection, value);
    }

    public int? ThirdSelection
    {
        get => _thirdSelection;
        private set => SetProperty(ref _thirdSelection, value);
    }

    public int? FourthSelection
    {
        get => _fourthSelection;
        private set => SetProperty(ref _fourthSelection, value);
    }

    public int? FifthSelection
    {
        get => _fifthSelection;
        private set => SetProperty(ref _fifthSelection, value);
    }

    public int? SixthSelection
    {
        get => _sixthSelection;
        private set => SetProperty(ref _sixthSelection, value);
    }

    private void UpdateSelections()
    {
        SecondSelection = GetNextValue(1);
        ThirdSelection = GetNextValue(2);
        FourthSelection = GetNextValue(3);
        FifthSelection = GetNextValue(4);
        SixthSelection = GetNextValue(5);
    }

    private int? GetNextValue(int offset)
    {
        if (!FirstSelection.HasValue)
        {
            return null;
        }

        var value = FirstSelection.Value + offset;
        return value <= 6 ? value : null;
    }
}
