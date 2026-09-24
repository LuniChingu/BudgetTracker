namespace BudgetTracker.Models;

public class AppData
{
    public List<Transaction>? transactionsTable { get; set; }
    public decimal IncomeGoal {get; set;}
    public decimal NeedsGoal { get; set; }
    public decimal WantsGoal {get; set;}
    public decimal SavingsGoal {get; set;}
}