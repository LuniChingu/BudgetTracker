using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BudgetTracker;

//TODO: save the input in the planned expense window so that when user edits the previous numbers are saved.

public partial class PlanEditorWindow : Window
{
    public decimal apartmentTotal { get; private set; }
    public decimal carTotal { get; private set; }
    public decimal healthTotal { get; private set; }
    
    private ObservableCollection<VaryingExpense> _varyingExpenses = new();
    
    public PlanEditorWindow()
    {
        InitializeComponent();
        
        VaryingExpensesList.ItemsSource = _varyingExpenses;

        _varyingExpenses.CollectionChanged += VaryingExpenses_CollectionChanged;
        
        CalculateVaryingTotals();
    }
    
    #region Fixed Expenses
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
    #endregion
    
    #region Varying Expenses

    private void BtnAddVaryingExpense_Click(object sender, RoutedEventArgs e)
    {
        _varyingExpenses.Add(new VaryingExpense());
    }

    private void VaryingExpenses_CollectionChanged(object? sender,
        NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (VaryingExpense expense in e.NewItems)
            {
                expense.PropertyChanged += VaryingExpenses_PropertyChanged;
            }
        }

        if (e.OldItems != null)
        {
            foreach (VaryingExpense expense in e.OldItems)
            {
                expense.PropertyChanged -= VaryingExpenses_PropertyChanged;
            }
        }
        
        CalculateVaryingTotals();
    }

    private void VaryingExpenses_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        CalculateVaryingTotals();
    }

    private void CalculateVaryingTotals()
    {
        var varNeedsTotal = _varyingExpenses
            .Where(x => x.Category == "NEED")
            .Sum(x => x.Total);
        var varWantsTotal = _varyingExpenses
            .Where(x => x.Category == "WANT")
            .Sum(x => x.Total);
        var varSavingsTotal = _varyingExpenses
            .Where(x => x.Category == "SAVING")
            .Sum(x => x.Total);

        txtVaryingNeedsTotal.Text = $"Needs Total: {varNeedsTotal:F2}";
        txtVaryingWantsTotal.Text = $"Wants Total: {varWantsTotal:F2}";
        txtVaryingSavingsTotal.Text = $"Savings Total: {varSavingsTotal:F2}";
    }
    #endregion
    
    #region Save/Load for the plans window & buttons
    
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
    
    #endregion
}

public class VaryingExpense : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string varExpenseName = string.Empty;
    public string VarExpenseName
    {
        get => varExpenseName;
        set { varExpenseName = value; OnPropertyChanged(nameof(VarExpenseName)); }
    }

    private decimal amountPerPayment;
    public decimal AmountPerPayment
    {
        get => amountPerPayment;
        set { amountPerPayment = value; 
            OnPropertyChanged(nameof(AmountPerPayment));
            OnPropertyChanged(nameof(Total));
        }
    }
    
    private int numberOfPayments;
    public int NumberOfPayments
    {
        get => numberOfPayments;
        set { numberOfPayments = value; 
            OnPropertyChanged(nameof(NumberOfPayments));
            OnPropertyChanged(nameof(Total));
        }
    }

    private string category = "WANT";
    public string Category
    {
        get => category;
        set { category = value; OnPropertyChanged(nameof(Category)); }
    }
    
    public decimal Total => AmountPerPayment * NumberOfPayments;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}