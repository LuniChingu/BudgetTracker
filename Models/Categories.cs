namespace BudgetTracker.Models;

public static class Categories
{
    public static readonly string[] Needs = ["Bills", "Transport", "Food"];
    public static readonly string[] Wants = ["Shopping"];
    public static readonly string[] Savings = ["Other"];

    public static readonly string[] All = [.. Needs, .. Wants, .. Savings];
}