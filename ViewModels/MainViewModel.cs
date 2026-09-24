using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BudgetTracker.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DashboardViewModel _dashboard;
    private readonly TransactionsViewModel _transactions;

    [ObservableProperty] private object _currentViewModel;

    public MainViewModel(DashboardViewModel dashboard, TransactionsViewModel transactions)
    {
        _dashboard = dashboard;
        _transactions = transactions;
        _currentViewModel = dashboard;
    }
    
    [RelayCommand] private void ShowDashboard() => CurrentViewModel =  _dashboard;
    [RelayCommand] private void ShowTransactions() => CurrentViewModel  = _transactions;
}