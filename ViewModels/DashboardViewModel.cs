using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Documents;
using BudgetTracker.Services;
using BudgetTracker.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace BudgetTracker.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly DataStore _store;
    private MonthlyBudget _currentBudget = null;
    private bool _isLoading;
    //private readonly IDialogService _dialogs;
    
    #region MonthNavigation
    private static DateTime LatestMonth => new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private static DateTime EarliestMonth => LatestMonth.AddMonths(-12);

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousMonthCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextMonthCommand))]
    public partial DateTime SelectedMonth { get; set; } = LatestMonth;
    
    private bool CanGoToPreviousMonth() => SelectedMonth > EarliestMonth;
    private bool CanGoToNextMonth() => SelectedMonth < LatestMonth;

    [RelayCommand(CanExecute = nameof(CanGoToPreviousMonth))]
    private void PreviousMonth() => SelectedMonth = SelectedMonth.AddMonths(-1);
    
    [RelayCommand(CanExecute = nameof(CanGoToNextMonth))]
    private void  NextMonth() => SelectedMonth = SelectedMonth.AddMonths(1);

    partial void OnSelectedMonthChanged(DateTime value) => LoadMonth();
    #endregion

    #region Budget Table Variables
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NeedsGoal))]
    [NotifyPropertyChangedFor(nameof(WantsGoal))]
    [NotifyPropertyChangedFor(nameof(SavingsGoal))]
    [NotifyPropertyChangedFor(nameof(TotalGoal))]
    [NotifyPropertyChangedFor(nameof(NeedsPercent))]
    [NotifyPropertyChangedFor(nameof(WantsPercent))]
    [NotifyPropertyChangedFor(nameof(SavingsPercent))]
    [NotifyPropertyChangedFor(nameof(TotalPercent))]
    public partial decimal IncomeGoal { get; set; }
    
    [ObservableProperty]
    public partial decimal IncomePlan { get; set; }
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NeedsPercent))]
    [NotifyPropertyChangedFor(nameof(WantsPercent))]
    [NotifyPropertyChangedFor(nameof(SavingsPercent))]
    [NotifyPropertyChangedFor(nameof(TotalPercent))]
    public partial decimal IncomeActual { get; set; }
    #endregion
    
    #region Income Variables Calculations

    partial void OnIncomeGoalChanged(decimal value)
    {
        if (_isLoading) return;
        _currentBudget.IncomeGoal = value;
        _store.Save();
    }

    partial void OnIncomePlanChanged(decimal value)
    {
        if  (_isLoading) return;
        _currentBudget.IncomePlan = value;
        _store.Save();
    }

    partial void OnIncomeActualChanged(decimal value)
    {
        if (_isLoading) return;
        _currentBudget.IncomeActual = value;
        _store.Save();
        Recalculate();
    }
    #endregion

    #region Goals
    public decimal NeedsGoal => IncomeGoal * 0.5m;
    public decimal WantsGoal => IncomeGoal * 0.3m;
    public decimal SavingsGoal => IncomeGoal * 0.2m;
    public decimal TotalGoal => NeedsGoal + WantsGoal + SavingsGoal;
    #endregion
    
    //[ObservableProperty] public partial DateTime SelectedMonth { get; set; } = DateTime.Today;
    
    #region Actuals
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalActual))]
    [NotifyPropertyChangedFor(nameof(NeedsPercent))]
    [NotifyPropertyChangedFor(nameof(TotalPercent))]
    public partial decimal NeedsActual { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalActual))]
    [NotifyPropertyChangedFor(nameof(WantsPercent))]
    [NotifyPropertyChangedFor(nameof(TotalPercent))]
    public partial decimal WantsActual { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalActual))]
    [NotifyPropertyChangedFor(nameof(SavingsPercent))]
    [NotifyPropertyChangedFor(nameof(TotalPercent))]
    public partial decimal SavingsActual { get; set; }
    
    public decimal TotalActual => NeedsActual + WantsActual + SavingsActual;
    #endregion
    
    #region Percentages
    public decimal NeedsPercent => Percent(NeedsActual);
    public decimal WantsPercent => Percent(WantsActual);
    public decimal SavingsPercent => Percent(SavingsActual);
    public decimal TotalPercent => NeedsPercent + WantsPercent + SavingsPercent;

    private decimal Percent(decimal actual) => IncomeActual == 0 ? 0 : actual / IncomeActual;
    #endregion
    
    #region Progress Bars
    public ObservableCollection<CategorySpending> CategorySummaries { get; } = new();
    #endregion

    #region Pie chart
    [ObservableProperty]
    public partial ISeries[] BudgetTrendSeries { get; set; } = [];
    
    private static PieSeries<double> MakeSlice(string name, decimal value, string hexColor) => new()
    {
        Name = name,
        Values = [(double)value],
        Fill = new SolidColorPaint(SKColor.Parse(hexColor)),
        OuterRadiusOffset = 0,
        InnerRadius = 115,
        CornerRadius = 10,
    };
    #endregion

    public DashboardViewModel(DataStore store)
    {
        _store = store;
        store.Transactions.CollectionChanged += OnTransactionsChanged;
        foreach (var item in store.Transactions)
        {
            item.PropertyChanged += OnTransactionEdited;
        }

        LoadMonth();
    }

    private void LoadMonth()
    {
        _currentBudget = _store.GetorCreateBudget(SelectedMonth.Year, SelectedMonth.Month);
        
        _isLoading = true;
        IncomeGoal = _currentBudget.IncomeGoal;
        IncomePlan = _currentBudget.IncomePlan;
        IncomeActual = _currentBudget.IncomeActual;
        _isLoading = false;
        
        Recalculate();
    }
    
    private void Recalculate()
    {
        var month = _store.Transactions
            .Where(t => t.Date.Month == SelectedMonth.Month && t.Date.Year == SelectedMonth.Year)
            .ToList();

        NeedsActual   = month.Where(t => Categories.Needs.Contains(t.Category)).Sum(t => t.Amount);
        WantsActual   = month.Where(t => Categories.Wants.Contains(t.Category)).Sum(t => t.Amount);
        SavingsActual = month.Where(t => Categories.Savings.Contains(t.Category)).Sum(t => t.Amount);

        CategorySummaries.Clear();

        var cards = new List<CategorySpending>();

        foreach (var name in Categories.All)
        {
            var categoryAmount =month.Where(t => t.Category == name).Sum(t => t.Amount);
            var share = IncomeActual == 0 ? 0 : (double)(categoryAmount / IncomeActual);
            
            cards.Add(new CategorySpending(){Name = name, Amount = categoryAmount, Share = share});
        }

        foreach (var card in cards.OrderByDescending(c => c.Amount))
        {
            CategorySummaries.Add(card);
        }

        BudgetTrendSeries =
        [
            MakeSlice("Needs", NeedsActual, "#b09bba"),
            MakeSlice("Wants", WantsActual, "#a6ad9d"),
            MakeSlice("Savings", SavingsActual, "#dca28e")
        ];
    }

    private void OnTransactionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (Transaction t in e.NewItems) 
            {
                t.PropertyChanged += OnTransactionEdited;
            }
        }

        if (e.OldItems != null)
        {
            foreach (Transaction t in e.OldItems)
            {
                t.PropertyChanged -= OnTransactionEdited;
            }
        }
        
        Recalculate();
    }

    private void OnTransactionEdited(object? sender, PropertyChangedEventArgs e) => Recalculate();
}