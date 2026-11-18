using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BudgetTracker.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly DashboardViewModel _dashboard;
    private readonly TransactionsViewModel _transactions;
    private readonly CommitmentsViewModel _commitments;

    [ObservableProperty]
    public partial object CurrentViewModel { get; set; }

    public MainViewModel(DashboardViewModel dashboard, TransactionsViewModel transactions,  CommitmentsViewModel commitments)
    {
        _dashboard = dashboard;
        _transactions = transactions;
        _commitments = commitments;
        CurrentViewModel = dashboard;
    }
    
    [RelayCommand] private void ShowDashboard() => CurrentViewModel =  _dashboard;
    [RelayCommand] private void ShowTransactions() => CurrentViewModel  = _transactions;
    [RelayCommand] private void ShowCommitments() => CurrentViewModel = _commitments;
}