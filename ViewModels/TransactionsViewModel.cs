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
    private readonly IDialogService _dialogService;

    public TransactionsViewModel(DataStore store, DialogService dialogService)
    {
        _store = store;
        _dialogService = dialogService;
        _store.Transactions.CollectionChanged += OnTransactionsChanged;
        foreach (var transaction in _store.Transactions)
        {
            transaction.PropertyChanged += OnTransactionEdited;
        }
    }

    #region Data table
    public ObservableCollection<Transaction> Transactions => _store.Transactions;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    public partial Transaction? SelectedTransaction { get; set; }
    #endregion
    
    #region Submit Form
    
    [ObservableProperty]
    public partial bool IsAddOpen { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial string NewName { get; set; }  = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    public partial decimal? NewAmount { get; set; }

    [ObservableProperty]
    public partial DateTime NewDate { get; set; } = DateTime.Today;

    [ObservableProperty]
    public partial string NewCategory { get; set; } = Categories.Default;
    
    private void ResetForm()
    {
        SetEditing(null);
        NewName = "";
        NewAmount = null;
        NewDate = DateTime.Today;
        NewCategory = Categories.Default;
    }
    #endregion
    
    #region Commands
    [RelayCommand]
    private void ToggleAdd()=> IsAddOpen = !IsAddOpen;

    partial void OnIsAddOpenChanged(bool value)
    {
        if (!value) ResetForm();
    }

    [RelayCommand(CanExecute = nameof(canEdit))]
    private void Edit()
    {
        if (SelectedTransaction is not { } t) return;
        
        SetEditing(t);
        NewName = t.TransactionName ?? "";
        NewAmount = t.Amount;
        NewDate = t.Date;
        NewCategory = t.Category ?? Categories.Default;
        IsAddOpen = true;
    }
    
    private bool canEdit() => SelectedTransaction is not null;

    [RelayCommand]
    private void EditRow(Transaction? transaction)
    {
        if (transaction is null) return;
        
        SelectedTransaction = transaction;
        Edit();
    }

    [RelayCommand(CanExecute = nameof(canSubmit))]
    private void Submit()
    {
        if (_editingTransaction is { } t)
        {
            t.TransactionName = NewName;
            t.Date =  NewDate;
            t.Amount = NewAmount ?? 0;
            t.Category = NewCategory;
        }
        else
        {
            Transactions.Add(new Transaction
            {
                TransactionName = NewName,
                Date = NewDate,
                Amount = NewAmount ?? 0,
                Category = NewCategory
            });
        }
        
        IsAddOpen = false;
    }
    
    private bool canSubmit() => !string.IsNullOrWhiteSpace(NewName) && NewAmount is > 0;

    [RelayCommand]
    private void DeleteRow(Transaction? transaction)
    {
        if (transaction is null) return;
        
        var name = string.IsNullOrWhiteSpace(transaction.TransactionName) 
            ? "this transaction"
            : $"\"{transaction.TransactionName}\"";

        if (!_dialogService.Confirm($"Delete {name}? This cannot be undone.", "Delete transaction"))
            return;
        
        if (transaction == _editingTransaction) IsAddOpen = false;
        if(SelectedTransaction == transaction) SelectedTransaction = null;
        
        Transactions.Remove(transaction);
    }
    #endregion
    
    #region Editing
    private Transaction? _editingTransaction;
    
    public bool IsEditing => _editingTransaction is not null;
    public string SubmitLabel => IsEditing ? "Save changes" : "+ Add transaction";

    private void SetEditing(Transaction? transaction)
    {
        _editingTransaction = transaction;
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(SubmitLabel));
    }
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