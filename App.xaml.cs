using System.Configuration;
using System.Data;
using System.Windows;
using BudgetTracker.Services;
using BudgetTracker.ViewModels;

namespace BudgetTracker;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var store = new DataStore();

        try
        {
            store.Load();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        
        var mainViewModel = new MainViewModel(new DashboardViewModel(store),
            new TransactionsViewModel(store, new DialogService()));

        var window = new MainWindow() { DataContext = mainViewModel };
        window.Show();
    }
}