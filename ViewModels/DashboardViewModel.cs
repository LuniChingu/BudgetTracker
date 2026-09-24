using System.Collections.Specialized;
using System.ComponentModel;
using BudgetTracker.Services;
using BudgetTracker.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Measure;
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
    public partial decimal IncomeGoal { get; set; }

    public decimal NeedsGoal => IncomeGoal * 0.5m;
    public decimal WantsGoal => IncomeGoal * 0.3m;
    public decimal SavingsGoal => IncomeGoal * 0.2m;
    public decimal TotalGoal => NeedsGoal + WantsGoal + SavingsGoal;

    [ObservableProperty]
    public partial DateTime SelectedMonth { get; set; } = DateTime.Today;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalActual))]
    [NotifyPropertyChangedFor(nameof(NeedsPercent))]
    private decimal _needsActual;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalActual))]
    [NotifyPropertyChangedFor(nameof(WantsPercent))]
    private decimal _wantsActual;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalActual))]
    [NotifyPropertyChangedFor(nameof(SavingsPercent))]
    private decimal _savingsActual;

    public decimal TotalActual => NeedsActual + WantsActual + SavingsActual;

    public decimal NeedsPercent => Percent(NeedsActual);
    public decimal WantsPercent => Percent(WantsActual);
    public decimal SavingsPercent => Percent(SavingsActual);
    public decimal TotalPercent => NeedsPercent + WantsPercent + SavingsPercent;

    private decimal Percent(decimal actual) => IncomeGoal == 0 ? 0 : actual / IncomeGoal;

    [ObservableProperty]
    public partial ISeries[] Series { get; set; } = [];

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

        Series =
        [
            MakeSlice("Needs", NeedsActual, "#85baff"),
            MakeSlice("Wants", WantsActual, "#2f4a9c"),
            MakeSlice("Savings", SavingsActual, "#1b2047")
        ];
    }

    private static PieSeries<double> MakeSlice(string name, decimal value, string hexColor) => new()
    {
        Name = name,
        Values = [(double)value],
        Fill = new SolidColorPaint(SKColor.Parse(hexColor)),
        MaxRadialColumnWidth = 40,
        OuterRadiusOffset = 0,
        
        ShowDataLabels = true,
        DataLabelsSize = 12,
        DataLabelsPaint = new SolidColorPaint(SKColors.AliceBlue),
        DataLabelsFormatter = p => $"{name}:\n{p.StackedValue!.Share:P0}"
    };
}