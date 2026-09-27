namespace BudgetTracker.Models;

public static class Categories
{
    public static readonly string[] Needs = ["Housing", "Transport", "Groceries", "Essentials"];
    public static readonly string[] Wants = ["Shopping", "Entertainment", "Travel", "Electronics", "Miscellaneous"];
    public static readonly string[] Savings = ["Savings", "Emergency", "Debt", "Other"];

    public static readonly string[] All = [.. Needs, .. Wants, .. Savings];
}