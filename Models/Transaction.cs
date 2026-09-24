using CommunityToolkit.Mvvm.ComponentModel;

namespace BudgetTracker.Models;

public partial class Transaction : ObservableObject
{
    [ObservableProperty] private string? _transactionName;
    [ObservableProperty] private DateTime _date = DateTime.Today;
    [ObservableProperty] private decimal _amount;
    [ObservableProperty] private string? _category;
    //public string? Currency { get; set; }
}