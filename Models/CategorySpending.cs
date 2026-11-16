namespace BudgetTracker.Models;

public class CategorySpending
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public double Share { get; set; }

    public double Percentage => Share * 100;
    public double BarValue => Percentage > 0 ? Math.Max(Percentage, 2) : 0;
}