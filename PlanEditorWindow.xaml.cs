using System.Windows;
using System.Windows.Controls;

namespace BudgetTracker;

public partial class PlanEditorWindow : Window
{
    public decimal apartmentTotal { get; set; }
    public decimal carTotal { get; set; }
    public decimal healthTotal { get; set; }
    
    public PlanEditorWindow()
    {
        InitializeComponent();
        UpdateFixedExpenses();
    }

    private void txtExpense_Changed(object sender, RoutedEventArgs routedEventArgs)
    {
        if (!this.IsInitialized) return;
        
        UpdateFixedExpenses();
    }

    private void UpdateFixedExpenses()
    {
        if (FixedExpenses == null) return;
        
        var apartmentExpenses = GetTotalByTag("Apartment");
        var carExpenses = GetTotalByTag("Car");
        var healthExpenses = GetTotalByTag("Health");
        
        txtApartmentTotal.Text = $"{apartmentExpenses:F2}";
        txtCarTotal.Text = $"{carExpenses:F2}";
        txtHealthTotal.Text = $"{healthExpenses:F2}";
    }
    
    private decimal GetTotalByTag(string categoryTag)
    {
        return FixedExpenses.Children.OfType<TextBox>()
            .Where(textBox => textBox.Tag?.ToString() == categoryTag)
            .Sum(textBox => decimal.TryParse(textBox.Text, out var value) ? value : 0);
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        apartmentTotal = GetTotalByTag("Apartment");
        carTotal = GetTotalByTag("Car");
        healthTotal = GetTotalByTag("Health");

        this.DialogResult = true;
        this.Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}