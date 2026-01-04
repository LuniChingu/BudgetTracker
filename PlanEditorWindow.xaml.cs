using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;

namespace BudgetTracker;

//TODO: save the input in the planned expense window so that when user edits the previous numbers are saved.

public partial class PlanEditorWindow : Window
{
    public decimal apartmentTotal { get; private set; }
    public decimal carTotal { get; private set; }
    public decimal healthTotal { get; private set; }
    
    public PlanEditorWindow()
    {
        InitializeComponent();
    }

    private void txtExpense_Changed(object sender, TextChangedEventArgs e)
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
        
        apartmentTotal = apartmentExpenses;
        carTotal = carExpenses;
        healthTotal = healthExpenses;
        
        txtApartmentTotal.Text = $"{apartmentExpenses:F2}";
        txtCarTotal.Text = $"{carExpenses:F2}";
        txtHealthTotal.Text = $"{healthExpenses:F2}";
    }
    
    private decimal GetTotalByTag(string categoryTag)
    {
        return FindVisualChildren<TextBox>(FixedExpenses)!
            .Where(textBox => textBox.Tag?.ToString() == categoryTag)
            .Sum(textBox => decimal.TryParse(textBox.Text, out var value) ? value : 0);
    }

    private IEnumerable<T> FindVisualChildren<T>(DependencyObject? parent) where T : DependencyObject
    {
        if (parent == null) yield break;

        var childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            
            if (child is T typedChild) yield return typedChild;

            foreach (var descendant in FindVisualChildren<T>(child)) 
                yield return  descendant;
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        UpdateFixedExpenses();

        this.DialogResult = true;
        this.Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = false;
        this.Close();
    }
}