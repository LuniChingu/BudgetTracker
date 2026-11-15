namespace BudgetTracker.Services;

public interface IDialogService
{
    bool Confirm(string message, string title);
}