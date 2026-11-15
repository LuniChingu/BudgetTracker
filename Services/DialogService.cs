using System.Windows;

namespace BudgetTracker.Services;

public class DialogService : IDialogService
{
    public bool Confirm(string message, string title) => MessageBox.Show(message, title,
        MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
}