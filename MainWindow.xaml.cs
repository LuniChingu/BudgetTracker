using System.Windows;
using System.Windows.Input;

//TODO: calculate percentages correctly (category / income = %%)
//TODO: add fields to fill in planned income and actual income
//TODO: make adding a transaction also a separate window or popup + add ability to delete rows from the data grid (editing is already possible since the data grid is not readonly)
//TODO: add an ability to pick a month to see in the dashboard (default is current month) + add ability to save the month's view into some sort of document?

//currently the budget table will only show current month's (according to computer time) transactions totals

namespace BudgetTracker;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
    }

    #region Upper Panel
    private void UpperBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }
    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void btnMinimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState =  WindowState.Minimized;
    }
    #endregion

    #region Budget Table calculations

    private readonly string[] needsCategories = ["Bills", "Transport", "Food"];
    private readonly string[] wantsCategories = ["Shopping"];
    private readonly  string[] savingsCategories = ["Other"];
    
    #endregion
}