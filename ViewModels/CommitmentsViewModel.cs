using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using BudgetTracker.Models;
using BudgetTracker.Services;
using CommunityToolkit.Mvvm.Input;

namespace BudgetTracker.ViewModels;

public partial class CommitmentsViewModel : ObservableObject
{
    private readonly DataStore _store;

    public CommitmentsViewModel(DataStore store)
    {
        _store = store;
    }

    public ObservableCollection<Commitment> Commitments => _store.Commitments;

    [RelayCommand]
    private void AddCommitment()
    {
        var newCommitment = new Commitment
        {
            Name = "",
        };
        _store.Commitments.Add(newCommitment);
        _store.Save();
    }

    [RelayCommand]
    private void RemoveCommitment(Commitment? commitment)
    {
        if (commitment is null) return;
        
        _store.Commitments.Remove(commitment);
        _store.Save();
    }
}