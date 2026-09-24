using BudgetTracker.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using BudgetTracker.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BudgetTracker.ViewModels;

public partial class TransactionsViewModel(DataStore store) : ObservableObject
{
    public ObservableCollection<Transaction> Transactions => store.Transactions;

    public string[] CategoryOptions => Categories.All;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private Transaction? _selectedTransaction;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private string _newName = "New Name";
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private decimal? _newAmount;
    
    [ObservableProperty]
    private DateTime _newDate = DateTime.Today;

    [ObservableProperty]
    private string _newCategory = Categories.All[0];

    [RelayCommand(CanExecute = nameof(canAdd))]
    private void Add()
    {
        Transactions.Add(new Transaction
        {
            TransactionName = NewName,
            Date = NewDate,
            Amount = NewAmount ?? 0,
            Category = NewCategory
        });

        store.Save();
        
        NewName = "New Name";
        NewAmount = null;
        NewDate = DateTime.Today;
        NewCategory = Categories.All[0];
    }

    private bool canAdd() => !string.IsNullOrWhiteSpace(NewName) && NewAmount is > 0;

    [RelayCommand(CanExecute = nameof(canDelete))]
    private void Delete()
    {
        if (SelectedTransaction is null) return;
        
        Transactions.Remove(SelectedTransaction);
        store.Save();
    }
    
    private bool canDelete() => SelectedTransaction is not null;
}