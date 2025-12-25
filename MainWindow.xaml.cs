using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace BudgetTracker;

public partial class MainWindow
{
    //private ObservableCollection<Transaction> _transactions;

    public MainWindow()
    {
        InitializeComponent();
        this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        //_transactions = new ObservableCollection<Transaction>();
        //dgTransactions.ItemsSource = _transactions;
        //dpDate.SelectedDate = DateTime.Today;
    }
    private void UpperBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed) DragMove();
    }
    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
    /*private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Please enter a name for the transaction");
            return;
        }

        if (!dpDate.SelectedDate.HasValue)
        {
            MessageBox.Show("Please select a date for the transaction");
            return;
        }

        if (!decimal.TryParse(txtAmount.Text, out decimal amount))
        {
            MessageBox.Show("Please enter a number for the amount");
            return;
        }

        string category = ((ComboBoxItem)cmbCategory.SelectedItem).Content.ToString() ?? string.Empty;
        string currency = ((ComboBoxItem)cmbCurrency.SelectedItem).Content.ToString() ?? string.Empty;

        Transaction newTransaction = new Transaction
        {
            Name = txtName.Text,
            Date = dpDate.SelectedDate.Value,
            Amount = amount,
            Category = category,
            Currency = currency
        };

        _transactions.Add(newTransaction);

        txtName.Clear();
        txtAmount.Clear();
        dpDate.SelectedDate = DateTime.Today;
        cmbCategory.SelectedIndex = 0;
        cmbCurrency.SelectedIndex = 0;

        UpdateChart(null, null);
    }

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
                Fill = null,  // No fill under line
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
        }
    

    public class Transaction
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; } = (decimal)0.0d;
        public string? Category { get; set; }
        public string? Currency { get; set; }
    }*/
    
}