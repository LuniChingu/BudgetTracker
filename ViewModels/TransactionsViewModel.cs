using BudgetTracker.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using BudgetTracker.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BudgetTracker.ViewModels;

public partial class TransactionsViewModel : ObservableObject
{
    private readonly DataStore _store;

    public TransactionsViewModel(DataStore store)
    {
        _store = store;
        _store.Transactions.CollectionChanged += OnTransactionsChanged;
        foreach (var transaction in _store.Transactions)
        {
            transaction.PropertyChanged += OnTransactionEdited;
        }
    }

    #region Data table
    public ObservableCollection<Transaction> Transactions => _store.Transactions;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    public partial Transaction? SelectedTransaction { get; set; }
    #endregion
    
    #region Add Form
    
    [ObservableProperty]
    public partial bool IsAddOpen { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial string NewName { get; set; }  = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    public partial decimal? NewAmount { get; set; }

    [ObservableProperty]
    public partial DateTime NewDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial string NewCategory { get; set; } = Categories.Default;
    
    private void ResetForm()
    {
        NewName = "";
        NewAmount = null;
        NewDate = DateTime.Today;
        NewCategory = Categories.Default;
    }
    #endregion
    
    #region Commands
    [RelayCommand]
    private void ToggleAdd()=> IsAddOpen = !IsAddOpen;

    //[RelayCommand]
    //private void CancelAdd()
    //{
    //    ResetForm();
    //    IsAddOpen = false;
    //}
    
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
        
        //ResetForm();
        IsAddOpen = false;
    }

    partial void OnIsAddOpenChanged(bool value)
    {
        if (!value) ResetForm();
    }

    private bool canAdd() => !string.IsNullOrWhiteSpace(NewName) && NewAmount is > 0;

    [RelayCommand(CanExecute = nameof(canDelete))]
    private void Delete()
    {
        if (SelectedTransaction is null) return;
        Transactions.Remove(SelectedTransaction);
    }
    
    private bool canDelete() => SelectedTransaction is not null;
    #endregion
    
    #region Saving
    private void OnTransactionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (Transaction t in e.NewItems)
                t.PropertyChanged += OnTransactionEdited;
        if (e.OldItems != null)
            foreach (Transaction t in e.OldItems)
                t.PropertyChanged -= OnTransactionEdited;
        
        _store.Save();
    }
    
    private void OnTransactionEdited(object? sender, PropertyChangedEventArgs e) => _store.Save();
    #endregion
}