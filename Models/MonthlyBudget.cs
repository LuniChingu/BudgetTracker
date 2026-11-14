namespace BudgetTracker.Models;

public class MonthlyBudget
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal IncomeGoal { get; set; }
    public decimal IncomePlan { get; set; }
    public decimal IncomeActual { get; set; }
}