using System.Collections.Specialized;
using System.ComponentModel;
using BudgetTracker.Services;
using BudgetTracker.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace BudgetTracker.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly DataStore _store;
    //private readonly IDialogService _dialogs;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NeedsGoal))]
    [NotifyPropertyChangedFor(nameof(WantsGoal))]
    [NotifyPropertyChangedFor(nameof(SavingsGoal))]
    [NotifyPropertyChangedFor(nameof(TotalGoal))]
    [NotifyPropertyChangedFor(nameof(NeedsPercent))]
    [NotifyPropertyChangedFor(nameof(WantsPercent))]
    [NotifyPropertyChangedFor(nameof(SavingsPercent))]
    [NotifyPropertyChangedFor(nameof(TotalPercent))]
    private partial decimal IncomeGoal { get; set; }

    public decimal NeedsGoal => IncomeGoal * 0.5m;
    public decimal WantsGoal => IncomeGoal * 0.3m;
    public decimal SavingsGoal => IncomeGoal * 0.2m;
    public decimal TotalGoal => NeedsGoal + WantsGoal + SavingsGoal;

    [ObservableProperty] private partial DateTime SelectedMonth { get; set; } = DateTime.Today;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalActual))]
    [NotifyPropertyChangedFor(nameof(NeedsPercent))]
    private partial decimal NeedsActual { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalActual))]
    [NotifyPropertyChangedFor(nameof(WantsPercent))]
    private partial decimal WantsActual { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalActual))]
    [NotifyPropertyChangedFor(nameof(SavingsPercent))]
    private partial decimal SavingsActual { get; set; }

    public decimal TotalActual => NeedsActual + WantsActual + SavingsActual;

    public decimal NeedsPercent => Percent(NeedsActual);
    public decimal WantsPercent => Percent(WantsActual);
    public decimal SavingsPercent => Percent(SavingsActual);
    public decimal TotalPercent => NeedsPercent + WantsPercent + SavingsPercent;

    private decimal Percent(decimal actual) => IncomeGoal == 0 ? 0 : actual / IncomeGoal;

    [ObservableProperty]
    public partial ISeries[] BudgetTrendSeries { get; set; } = [];

    public DashboardViewModel(DataStore store)
    {
        _store = store;
        IncomeGoal = store.IncomeGoal;
        store.Transactions.CollectionChanged += OnTransactionsChanged;

        foreach (var item in store.Transactions)
        {
            item.PropertyChanged += OnTransactionEdited;
        }

        Recalculate();
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

    private void OnTransactionEdited(object? sender, PropertyChangedEventArgs e)
    {
        Recalculate();
        _store.Save();
    }
    
    partial void OnSelectedMonthChanged(DateTime value) => Recalculate();

    partial void OnIncomeGoalChanged(decimal value)
    {
        _store.IncomeGoal = value;
        _store.Save();
    }

    private void Recalculate()
    {
        var month = _store.Transactions
            .Where(t => t.Date.Month == SelectedMonth.Month && t.Date.Year == SelectedMonth.Year)
            .ToList();

        NeedsActual   = month.Where(t => Categories.Needs.Contains(t.Category)).Sum(t => t.Amount);
        WantsActual   = month.Where(t => Categories.Wants.Contains(t.Category)).Sum(t => t.Amount);
        SavingsActual = month.Where(t => Categories.Savings.Contains(t.Category)).Sum(t => t.Amount);

        BudgetTrendSeries =
        [
            MakeSlice("Needs", NeedsActual, "#b09bba"),
            MakeSlice("Wants", WantsActual, "#a6ad9d"),
            MakeSlice("Savings", SavingsActual, "#dca28e")
        ];
    }

    private static PieSeries<double> MakeSlice(string name, decimal value, string hexColor) => new()
    {
        Name = name,
        Values = [(double)value],
        Fill = new SolidColorPaint(SKColor.Parse(hexColor)),
        OuterRadiusOffset = 0,
        InnerRadius = 115,
        CornerRadius = 10,
    };
}