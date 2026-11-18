namespace BudgetTracker.Models;

public enum CommitmentType {Fixed, Variable, Installment, Loan}
public enum CommitmentStatus {None, Active, Frozen, Cancelled}


public class Commitment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    
    public decimal Amount { get; set; }
    public string Category { get; set; } = "";
    
    public CommitmentType Type { get; set; }
    
    public DateTime FirstDueDate { get; set; }
    public DateTime LastDueDate { get; set; } = DateTime.Today;

    public int IntervalMonths { get; set; } = 1;
    public int InstallmentCount { get; set; }
    
    public bool IsAutoCharge { get; set; }
    public bool IsSubscription { get; set; }
    public int PaymentsAlreadyMade { get; set; }
    public CommitmentStatus Status { get; set; } = CommitmentStatus.Active;
}