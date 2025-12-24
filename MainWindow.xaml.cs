using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BudgetTracker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
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

    private void OnRightClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is DataGridRow row)
        {
            row.IsSelected = true;
        }
    }

    private void OnDeleteTransaction(object sender, RoutedEventArgs e)
    {
        if (dgTransactions.SelectedItem is Transaction selectedRow)
        {
            _transactions.Remove(selectedRow);
        }
    }

    private void UpdateChart(object sender, RoutedEventArgs e)
    {
        chartCanvas.Children.Clear();
        if (_transactions.Count == 0)
        {
            TextBlock noDataText = new TextBlock
            {
                Text = "No data to display. Add some transactions!",
                FontSize = 16,
                Foreground = Brushes.Gray
            };
            Canvas.SetLeft(noDataText, 20);
            Canvas.SetTop(noDataText, 20);
            chartCanvas.Children.Add(noDataText);
            return;
        }

        Dictionary<string, decimal> groupedData;

        if (rbCategory.IsChecked == true)
        {
            groupedData = _transactions.
                GroupBy(t => t.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(t => t.Amount)
                );
        }
        else
        {
            groupedData = _transactions.
                GroupBy(t => t.Date.ToString("MMM yyyy"))
                .ToDictionary(
                    g => g.Key, 
                    g => g.Sum(t => t.Amount)
                );
        }

        decimal total = groupedData.Values.Sum();

        var colors = new Brush[]
        {
            Brushes.LightBlue, Brushes.LightGreen, Brushes.LightCoral,
            Brushes.LightGoldenrodYellow, Brushes.LightPink, Brushes.LightSalmon
        };

        var centerX = chartCanvas.ActualWidth > 0 ? chartCanvas.ActualWidth / 2 : 300;
        var centerY = chartCanvas.ActualWidth > 0 ? chartCanvas.ActualHeight / 2 : 125;
        var radius = Math.Min(centerX, centerY) - 50;
        
        if (radius <= 0) radius = 100;

        double startAngle = -90;
        var colorIndex = 0;

        foreach (var item in groupedData)
        {
            var percentage = (item.Value / total) * 100;
            var angle = (double)(percentage / 100 * 360);

            DrawPieSlice(centerX, centerY, radius, startAngle, angle, colors[colorIndex % colors.Length], item.Key,
                percentage);

            startAngle += angle;
            colorIndex++;
        }
    }

    private void DrawPieSlice(double centerX, double centerY, double radius,
        double startAngle, double angle, Brush color, string label, decimal percentage)
    {
        Path path = new Path
        {
            Fill = color,
            Stroke = Brushes.White,
            StrokeThickness = 2
        };

        PathGeometry geometry = new PathGeometry();
        PathFigure figure = new PathFigure()
        {
            StartPoint = new Point(centerX, centerY),
            IsClosed = true
        };

        double endAngle = startAngle + angle;

        double x1 = centerX + radius * Math.Cos(startAngle * Math.PI / 180);
        double y1 = centerY + radius * Math.Sin(startAngle * Math.PI / 180);

        double x2 = centerX + radius * Math.Cos(endAngle * Math.PI / 180);
        double y2 = centerY + radius * Math.Sin(endAngle * Math.PI / 180);

        figure.Segments.Add(new LineSegment(new Point(x1, y1), true));
        figure.Segments.Add(new ArcSegment
        {
            Point = new Point(x2, y2),
            Size = new Size(radius, radius),
            SweepDirection = SweepDirection.Clockwise,
            IsLargeArc = angle > 180
        });

        geometry.Figures.Add(figure);
        path.Data = geometry;
        chartCanvas.Children.Add(path);

        double labelAngle = startAngle + angle / 2;
        double labelRadius = radius * 0.7;

        double labelX = centerX + labelRadius * Math.Cos(labelAngle * Math.PI / 180);
        double labelY = centerY + labelRadius * Math.Sin(labelAngle * Math.PI / 180);

        TextBlock text = new TextBlock
        {
            Text = $"{label}\n{percentage:F1}%",
            TextAlignment = TextAlignment.Center,
            Foreground = Brushes.Black
        };

        Canvas.SetLeft(text, labelX - 30);
        Canvas.SetTop(text, labelY - 15);
        chartCanvas.Children.Add(text);
    }

    public class Transaction
    {
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Category { get; set; }
        public string? Currency { get; set; }
    }*/
}