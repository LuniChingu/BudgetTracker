using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
//using LiveChartsCore;
//using LiveChartsCore.SkiaSharpView;
//using LiveChartsCore.SkiaSharpView.Painting;
//using SkiaSharp;

//TODO: add calculating percentages of the actuals. (category / income = %%)
//TODO: add color coding based on percentages
//TODO: add fields to fill in planned income and actual income
//TODO: make the 'plans' form in a separate window/popup and a bit more intricate (like it is in my excel)
//TODO: make adding a transaction also a separate window or popup + add ability to edit and delete rows from the datagrid
//TODO: add an ability to pick a month to see in the dashboard (default is current month) + add ability to save the month's view into some sort of document?

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
    
    //Transactions stuff
    private ObservableCollection<Transaction> _transactions;
    

    public MainWindow()
    {
        InitializeComponent();
        this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        _transactions = []; // study note: this is the collection expression for "new ObservableCollection<Transaction>()"
        dgTransactions.ItemsSource = _transactions;
        dpDate.SelectedDate = DateTime.Today;
        
        CalculateActuals();
        var savedData = LoadData();
        if (savedData != null && savedData.transactionsTable != null)
        {

            foreach (var t in savedData.transactionsTable!)
            {
                foreach (var item in savedData.transactionsTable)
                {
                    dgTransactions.
                }
            }
                txtIncomeGoal.Text = savedData.IncomeGoal.ToString("F2");
                txtNeedsGoal.Text = savedData.NeedsGoal.ToString("F2");
                txtWantsGoal.Text = savedData.WantsGoal.ToString("F2");
                txtSavingsGoal.Text = savedData.SavingsGoal.ToString("F2");
        }
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
        
        SaveData(_transactions, incomeGoal, needsGoal, wantsGoal, savingsGoal);
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
        if (string.IsNullOrWhiteSpace(txtNeedsTotalPlan.Text) && string.IsNullOrWhiteSpace(txtWantsTotalPlan.Text) && string.IsNullOrWhiteSpace(txtSavingsTotalPlan.Text))
        {
            MessageBox.Show("Enter a number");
            return;
        }
        
        needsPlan = decimal.Parse(txtNeedsTotalPlan.Text);
        wantsPlan = decimal.Parse(txtWantsTotalPlan.Text);
        savingsPlan = decimal.Parse(txtSavingsTotalPlan.Text);
        
        totalPlan = needsPlan + wantsPlan + savingsPlan;
        
        UpdatePlannedBudget();
        CalculateActuals();
        SaveData(_transactions, incomeGoal, needsGoal, wantsGoal, savingsGoal);
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
        var currentMonth = DateTime.Today;

        var monthTransactions = _transactions
            .Where(t => t.Date.Month == currentMonth.Month &&
                        t.Date.Year == currentMonth.Year)
            .ToList();
        var needsActual = monthTransactions.Where(t => needsCategories.Contains(t.Category)).Sum(t => t.Amount);
        var wantsActual = monthTransactions.Where(t => wantsCategories.Contains(t.Category)).Sum(t => t.Amount);
        var savingsActual = monthTransactions.Where(t => savingsCategories.Contains(t.Category)).Sum(t => t.Amount);
        
        var totalActual = needsActual + wantsActual + savingsActual;
        
        txtNeedsActual.Text = $"{needsActual:F2}";
        txtWantsActual.Text = $"{wantsActual:F2}";
        txtSavingsActual.Text = $"{savingsActual:F2}";
        txtTotalActual.Text = $"{totalActual:F2}";
        
        CalculateBudgetPercentages(needsActual, wantsActual, savingsActual, totalActual);
    }
    #endregion
    
    #region percentage calculations
    private void CalculateBudgetPercentages(decimal needsActual, decimal wantsActual, decimal savingsActual,
        decimal totalActual)
    {
        var needsPercent = CalculatePercentage(needsActual, needsGoal);
        var wantsPercent = CalculatePercentage(wantsActual, wantsGoal);
        var savingsPercent = CalculatePercentage(savingsActual, savingsGoal);
        
    }

    private static decimal CalculatePercentage(decimal actual, decimal goal)
    {
        if (goal == 0) return 0;

        return (actual / goal) * 100;
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

    private void SaveData(ObservableCollection<Transaction> transactions, decimal income, decimal needs, decimal wants, decimal savings)
    {
        var dataToSave = new AppData
        {
            transactionsTable = transactions.ToList(),
            IncomeGoal = income,
            NeedsGoal =  needs,
            WantsGoal =  wants,
            SavingsGoal =  savings
        };

        var jsonString = JsonSerializer.Serialize(dataToSave);
        File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppData.json"), jsonString);

    }

    public static AppData LoadData()
    {
        if (!File.Exists("AppData.json")) return new AppData();
        string jsonString = File.ReadAllText("AppData.json");
        return JsonSerializer.Deserialize<AppData>(jsonString)!;
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
    /*
    private void UpdateChart(object sender, RoutedEventArgs e)
    {
        if (_transactions.Count == 0)
        {
            // Clear all charts if no data
            pieChart.Series = Array.Empty<ISeries>();
            barChart.Series = Array.Empty<ISeries>();
            lineChart.Series = Array.Empty<ISeries>();
            return;
        }

        // Decide which chart to show
        if (rbPieChart.IsChecked == true)
        {
            ShowPieChart();
        }
        else if (rbBarChart.IsChecked == true)
        {
            ShowBarChart();
        }
        else if (rbLineChart.IsChecked == true)
        {
            ShowLineChart();
        }
    }

    private void ShowPieChart()
        {
            // Show pie chart, hide others
            pieChart.Visibility = Visibility.Visible;
            barChart.Visibility = Visibility.Collapsed;
            lineChart.Visibility = Visibility.Collapsed;

            // Get grouped data
            Dictionary<string, decimal> groupedData = GetGroupedData();

            // Convert data to PieSeries format
            // This is the KEY part - instead of binding, we directly create the series
            var series = new List<ISeries>();
            
            foreach (var item in groupedData)
            {
                series.Add(new PieSeries<decimal>
                {
                    Values = [item.Value],  // The amount
                    Name = item.Key,                // The category/month name
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"{item.Key}\n${point.Coordinate.PrimaryValue:F2}"
                });
            }

            // Directly set the Series property - NO BINDING NEEDED!
            pieChart.Series = series;
        }

        private void ShowBarChart()
        {
            // Show bar chart, hide others
            pieChart.Visibility = Visibility.Collapsed;
            barChart.Visibility = Visibility.Visible;
            lineChart.Visibility = Visibility.Collapsed;

            // Get grouped data
            Dictionary<string, decimal> groupedData = GetGroupedData();

            // Create bar series
            var columnSeries = new ColumnSeries<decimal>
            {
                Values = groupedData.Values.ToArray(),  // The amounts
                Name = "Amount",
                Fill = new SolidColorPaint(SKColors.DodgerBlue),
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsSize = 12,
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                DataLabelsFormatter = point => $"${point.Coordinate.PrimaryValue:F2}"
            };

            // Set X-axis labels (categories/months)
            barChart.XAxes = new[]
            {
                new Axis
                {
                    Labels = groupedData.Keys.ToArray(),  // Category names
                    LabelsRotation = 15
                }
            };

            // Set Y-axis
            barChart.YAxes = new[]
            {
                new Axis
                {
                    Name = "Amount ($)",
                    NamePaint = new SolidColorPaint(SKColors.Black)
                }
            };

            // Directly set the Series - NO BINDING!
            barChart.Series = new ISeries[] { columnSeries };
        }

        private void ShowLineChart()
        {
            // Show line chart, hide others
            pieChart.Visibility = Visibility.Collapsed;
            barChart.Visibility = Visibility.Collapsed;
            lineChart.Visibility = Visibility.Visible;

            // For line chart, we need data by date (sorted)
            var sortedByDate = _transactions
                .OrderBy(t => t.Date)
                .GroupBy(t => t.Date.ToString("MMM yyyy"))
                .Select(g => new
                {
                    Month = g.Key,
                    Total = g.Sum(t => t.Amount)
                })
                .ToList();

            // Create line series
            var lineSeries = new LineSeries<decimal>
            {
                Values = sortedByDate.Select(x => x.Total).ToArray(),
                Name = "Spending Over Time",
                Fill = null,  // No fill underline
                Stroke = new SolidColorPaint(SKColors.Green) { StrokeThickness = 3 },
                GeometrySize = 10,  // Point size
                GeometryStroke = new SolidColorPaint(SKColors.DarkGreen) { StrokeThickness = 3 },
                DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                DataLabelsSize = 11,
                DataLabelsFormatter = point => $"${point.Coordinate.PrimaryValue:F2}"
            };

            // Set X-axis with month labels
            lineChart.XAxes = new[]
            {
                new Axis
                {
                    Labels = sortedByDate.Select(x => x.Month).ToArray(),
                    LabelsRotation = 15
                }
            };

            // Set Y-axis
            lineChart.YAxes = new[]
            {
                new Axis
                {
                    Name = "Amount ($)",
                    NamePaint = new SolidColorPaint(SKColors.Black)
                }
            };

            // Directly set the Series - NO BINDING!
            lineChart.Series = new ISeries[] { lineSeries };
        }

        // Helper method to get grouped data based on selected option
        private Dictionary<string, decimal> GetGroupedData()
        {
            if (rbCategory.IsChecked == true)
            {
                // Group by category
                return _transactions
                    .GroupBy(t => t.Category)
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
            }
            else
            {
                // Group by month
                return _transactions
                    .GroupBy(t => t.Date.ToString("MMM yyyy"))
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
            }
        }*/
}