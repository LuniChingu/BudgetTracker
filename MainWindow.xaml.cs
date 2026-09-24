using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

//TODO: calculate percentages correctly (category / income = %%)
//TODO: add color coding based on percentages
//TODO: add fields to fill in planned income and actual income
//TODO: make adding a transaction also a separate window or popup + add ability to delete rows from the data grid (editing is already possible since the data grid is not readonly)
//TODO: add an ability to pick a month to see in the dashboard (default is current month) + add ability to save the month's view into some sort of document?

//TODO: currently the budget table will only show current month's (according to computer time) transactions totals

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
    
    //pie chart
    public ISeries[] SpendingSeries { get; set; } = Array.Empty<ISeries>();

    public MainWindow()
    {
        InitializeComponent();
        
        this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        _transactions = []; // study note: this is the collection expression for "new ObservableCollection<Transaction>()"
        dgTransactions.ItemsSource = _transactions;
        dpDate.SelectedDate = DateTime.Today;
        
        LoadData();
        ShowDashboard();
        CalculateActuals();
        
        Console.WriteLine(txtIncomePlan.FontFamily.Source);
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
    
    #region Side bar menu
    private void rbDashboard_Checked(object sender, RoutedEventArgs e)
    {
        if (dashboardView != null)
        {
            ShowDashboard();
        }
    }

    private void rbTransactions_Checked(object sender, RoutedEventArgs e)
    {
        if (transactionsView != null)
        {
            ShowTransactions();
        }
    }

    private void ShowDashboard()
    {
        dashboardView.Visibility = Visibility.Visible;
        transactionsView.Visibility = Visibility.Collapsed;
        CalculateActuals();
    }
    private void ShowTransactions()
    {
        dashboardView.Visibility = Visibility.Collapsed;
        transactionsView.Visibility = Visibility.Visible;
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
            UpdateGoals();
        }
        else
        {
            MessageBox.Show("Income goal must be a number");
            textBox.Text = incomeGoal.ToString("F2");
        }
        SaveData();
    }

    private void UpdateGoals()
    {
        txtIncomeGoal.Text = $"{incomeGoal:F2}";
        txtNeedsGoal.Text = $"{needsGoal:F2}";
        txtWantsGoal.Text = $"{wantsGoal:F2}";
        txtSavingsGoal.Text = $"{savingsGoal:F2}";
        txtTotalGoal.Text = $"{totalGoal:F2}";
    }
    
    private void SavePlanBtn_Click(object sender, RoutedEventArgs e)
    {
        SaveData();
        CalculateActuals();
    }

    private void UpdatePlannedBudget()
    {
        txtNeedsPlan.Text = $"{needsPlan:F2}";
        txtWantsPlan.Text = $"{wantsPlan:F2}";
        txtSavingsPlan.Text = $"{savingsPlan:F2}";
        txtTotalPlan.Text = $"{totalPlan:F2}";
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
        
        txtNeedsActual.Text = $"{needsActual:F2}";
        txtWantsActual.Text = $"{wantsActual:F2}";
        txtSavingsActual.Text = $"{savingsActual:F2}";
        txtTotalActual.Text = $"{totalActual:F2}";
        
        CalculateBudgetPercentages();
        UpdateChart();
        UpdateChartLabels();
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
            UpdatePlannedBudget();
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
        
        txtNeedsPercentage.Text = $"{needsPercent:P}";
        txtWantsPercentage.Text = $"{wantsPercent:P}";
        txtSavingsPercentage.Text = $"{savingsPercent:P}";
    }

    private static decimal CalculatePercentage(decimal actual, decimal goal)
    {
        if (goal == 0) return 0;

        return (actual / goal); //this used to have *100 what because I'm using the percentage format there's not need for it
    }
    #endregion
    
    #region piechart data

    private void UpdateChart()
    {
        NeedsSeries.Values = new [] { (double)needsActual };
        WantsSeries.Values = new [] { (double)wantsActual };
        SavingsSeries.Values = new [] { (double)savingsActual };
    }

    private void UpdateChartLabels()
    {
        NeedsSeries.DataLabelsFormatter = point => $"{point.StackedValue!.Share:P1}";
        WantsSeries.DataLabelsFormatter = point => $"{point.StackedValue!.Share:P1}";
        SavingsSeries.DataLabelsFormatter = point => $"{point.StackedValue!.Share:P1}";
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
                Name = txtName.Text,
                Date = dpDate.SelectedDate.Value,
                Amount = decimal.Parse(txtAmount.Text),
                Category = category
            };

            _transactions.Add(newTransaction);
        }
        
        SaveData();
        
        txtName.Clear();
        txtAmount.Clear();
        dpDate.SelectedDate = DateTime.Today;
        cmbCategory.SelectedIndex = 0;
        
        CalculateActuals();
    }
    
    
    public class Transaction
    {
        public string? Name { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; } = (decimal)0.0d;
        public string? Category { get; set; }
        //public string? Currency { get; set; }
    }
    #endregion

    #region Save/Load System
    private void SaveData()
    {
        var dataToSave = new AppData
        {
            transactionsTable = _transactions.ToList(),
            IncomeGoal = incomeGoal,
            NeedsGoal = needsGoal,
            WantsGoal = wantsGoal,
            SavingsGoal = savingsGoal
        };

        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(dataToSave, options);

            Directory.CreateDirectory(Path.GetDirectoryName(dataFilePath)!);
            File.WriteAllText(dataFilePath, jsonString);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void LoadData()
    {
        if (!File.Exists(dataFilePath))
        {
            incomeGoal = 1500m;
            needsGoal = 1500m;
            wantsGoal = 1500m;
            savingsGoal = 1500m;
            return;
        }

        try
        {
            var jsonString = File.ReadAllText(dataFilePath);
            var data = JsonSerializer.Deserialize<AppData>(jsonString);

            if (data != null)
            {
                incomeGoal = data.IncomeGoal;
                needsGoal = data.NeedsGoal;
                wantsGoal = data.WantsGoal;
                savingsGoal = data.SavingsGoal;

                _transactions.Clear();
                foreach (var transaction in data.transactionsTable ?? new List<Transaction>())
                {
                    _transactions.Add(transaction);
                }
                
                UpdateGoals();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            _transactions = [];
        }
    }
    
    public class AppData
    {
        public List<Transaction>? transactionsTable { get; set; }
        public decimal IncomeGoal {get; set;}
        public decimal NeedsGoal { get; set; }
        public decimal WantsGoal {get; set;}
        public decimal SavingsGoal {get; set;}
    }

    #endregion
}