using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using LiveChartsCore;
using BudgetTracker.Models;

//TODO: calculate percentages correctly (category / income = %%)
//TODO: add color coding based on percentages
//TODO: add fields to fill in planned income and actual income
//TODO: make adding a transaction also a separate window or popup + add ability to delete rows from the data grid (editing is already possible since the data grid is not readonly)
//TODO: add an ability to pick a month to see in the dashboard (default is current month) + add ability to save the month's view into some sort of document?

//currently the budget table will only show current month's (according to computer time) transactions totals

namespace BudgetTracker;

public partial class MainWindow
{
    //Budget Objects
    private decimal incomeGoal;
    private decimal needsGoal;
    private decimal wantsGoal;
    private decimal savingsGoal;
    private decimal totalGoal;
    
    //private decimal incomePlan;
    private decimal needsPlan;
    private decimal wantsPlan;
    private decimal savingsPlan;
    private decimal totalPlan;
    
    //Month & Year Selector
    private DateTime selectedMonth = DateTime.Today;
    
    //Transactions stuff
    private ObservableCollection<Transaction> _transactions;
    private readonly string dataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BudgetTracker", "AppData.json");
    
    //Actuals calculations
    private decimal needsActual;
    private decimal wantsActual;
    private decimal savingsActual;

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
        this.Close();
    }

    private void btnMinimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState =  WindowState.Minimized;
    }
    #endregion

    #region Budget Table colculations
    private void TxtIncomeGoal_LostFocus(object sender, RoutedEventArgs e)
    {
        var textBox = (TextBox)sender;

        if (decimal.TryParse(textBox.Text, out var newIncome))
        {
            incomeGoal = newIncome;
            needsGoal = incomeGoal * 0.5m;
            wantsGoal = incomeGoal * 0.3m;
            savingsGoal = incomeGoal * 0.2m;
            
            totalGoal = needsGoal + wantsGoal + savingsGoal;
        }
        else
        {
            MessageBox.Show("Income goal must be a number");
            textBox.Text = incomeGoal.ToString("F2");
        }
    }
    
    private void SavePlanBtn_Click(object sender, RoutedEventArgs e)
    {
        CalculateActuals();
    }

    private readonly string[] needsCategories = ["Bills", "Transport", "Food"];
    private readonly string[] wantsCategories = ["Shopping"];
    private readonly  string[] savingsCategories = ["Other"];

    private void CalculateActuals()
    {
        var currentMonth = selectedMonth;

        var monthTransactions = _transactions
            .Where(t => t.Date.Month == currentMonth.Month &&
                                t.Date.Year == currentMonth.Year)
            .ToList();
        needsActual = monthTransactions.Where(t => needsCategories.Contains(t.Category)).Sum(t => t.Amount);
        wantsActual = monthTransactions.Where(t => wantsCategories.Contains(t.Category)).Sum(t => t.Amount);
        savingsActual = monthTransactions.Where(t => savingsCategories.Contains(t.Category)).Sum(t => t.Amount);
        
        var totalActual = needsActual + wantsActual + savingsActual;
        
        CalculateBudgetPercentages();
    }
    #endregion

    #region plans form window open

    private void btnEditPlan_Click(object sender, RoutedEventArgs e)
    {
        var planEditor = new PlanEditorWindow();

        if (planEditor.ShowDialog() == true)
        {
            var plannedNeedsTotal = planEditor.apartmentTotal + planEditor.carTotal + planEditor.healthTotal;
            needsPlan = plannedNeedsTotal;
        }
    }

    #endregion
    
    #region percentage calculations
    private void CalculateBudgetPercentages()
    {
        var needsPercent = CalculatePercentage(needsActual, needsGoal);
        var wantsPercent = CalculatePercentage(wantsActual, wantsGoal);
        var savingsPercent = CalculatePercentage(savingsActual, savingsGoal);
        
        //the percentages currently are calculated comparing actual vs goal of each category, I believe the whole point
        //is to calculate the percentage against the whole planned budget, and maybe compared to the income or smth
    }

    private static decimal CalculatePercentage(decimal actual, decimal goal)
    {
        if (goal == 0) return 0;

        return (actual / goal); //this used to have *100 what because I'm using the percentage format there's not need for it
    }
    #endregion

    #region Adding a Transaction to DataGrid
    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        var category = ((ComboBoxItem)cmbCategory.SelectedItem).Content.ToString() ?? string.Empty;

        if (dpDate.SelectedDate != null)
        {
            var newTransaction = new Transaction
            {
                TransactionName = txtName.Text,
                Date = dpDate.SelectedDate.Value,
                Amount = decimal.Parse(txtAmount.Text),
                Category = category
            };

            _transactions.Add(newTransaction);
        }
        
        txtName.Clear();
        txtAmount.Clear();
        dpDate.SelectedDate = DateTime.Today;
        cmbCategory.SelectedIndex = 0;
        
        CalculateActuals();
    }
    #endregion
}