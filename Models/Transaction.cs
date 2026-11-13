using CommunityToolkit.Mvvm.ComponentModel;

namespace BudgetTracker.Models;

public partial class Transaction : ObservableObject
{
    [ObservableProperty]
    public partial string? TransactionName { get; set; }

    [ObservableProperty]
    public partial DateTime Date { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial decimal Amount { get; set; }

    [ObservableProperty]
    public partial string? Category { get; set; }
    //public string? Currency { get; set; }
}