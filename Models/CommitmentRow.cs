namespace BudgetTracker.Models;

public class CommitmentRow
{
    public CommitmentRow(Commitment commitment, IEnumerable<Transaction> transactions, DateTime month)
    {
        Commitment = commitment;
        
        PaidCount = commitment.PaymentsAlreadyMade + transactions.Count(t => t.CommitmentId == commitment.Id);
        PaymentsLeft = Math.Max(0, commitment.InstallmentCount - PaidCount);
        Progress = commitment.InstallmentCount > 0 ? Math.Min(100.0, (double)PaidCount / Commitment.InstallmentCount * 100) : 0;
        FrequencyText = GetFrequencyText();
        IsDueThisMonth = CheckDueThisMonth(month);
    }
    
    public Commitment Commitment { get; }
    
    public int PaidCount { get; }
    public int PaymentsLeft { get; }
    public double Progress { get; }
    public string FrequencyText { get; }
    public bool IsDueThisMonth { get; }

    private string GetFrequencyText() => Commitment.IntervalMonths switch
    {
        <= 0 => "Not set",
        1 => "Monthly",
        2 => "Bimonthly",
        3 => "Quarterly",
        6 => "Biannually",
        12 => "Annually",
        _ => $"Every {Commitment.IntervalMonths} months"
    };

    private bool CheckDueThisMonth(DateTime month)
    {
        if (Commitment.IntervalMonths <= 0) return false;
        int start = Commitment.FirstDueDate.Year * 12 + Commitment.FirstDueDate.Month;
        int current = month.Year * 12 + month.Month;
        if (current < start) return false;

        return (current - start) % Commitment.IntervalMonths == 0;
    }
}