using System.Windows;
using System.Windows.Controls;

namespace BudgetTracker;

public partial class PlanEditorWindow : Window
{
    public PlanEditorWindow()
    {
        InitializeComponent();
    }

    private void txtExpense_Changed(object sender, RoutedEventArgs routedEventArgs)
    {
        
    }

    private void UpdateFixedExpenses()
    {
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
}