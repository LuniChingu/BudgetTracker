using System.Collections.ObjectModel;
using System.Text.Json;
using System.IO;
using BudgetTracker.Models;

namespace BudgetTracker.Services;

public class DataStore
{
    #region Fields
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    private readonly string dataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BudgetTracker", "AppData.json");
    private const decimal DefaultIncomeGoal = 10000m;
    #endregion
    
    #region Data
    public ObservableCollection<Transaction> Transactions { get; } = [];
    public List<MonthlyBudget>  MonthlyBudgets { get; } = [];
    public ObservableCollection<Commitment> Commitments { get; } = [];
    #endregion
    //public decimal IncomeGoal { get; set; } = 1500m;
    
    #region Monthly Budgets

    public MonthlyBudget GetorCreateBudget(int year, int month)
    {
        var existing = MonthlyBudgets.FirstOrDefault(x => x.Year == year && x.Month == month);
        if (existing != null) return existing;
        
        var previous = MonthlyBudgets
            .Where(x => x.Year * 12 + x.Month < year * 12 + month)
            .OrderByDescending(x => x.Year * 12 + x.Month)
            .FirstOrDefault();

        var created = new MonthlyBudget
        {
            Year = year,
            Month = month,
            IncomeGoal = previous?.IncomeGoal ?? DefaultIncomeGoal
        };
        
        MonthlyBudgets.Add(created);
        return created;
    }
    #endregion
    
    #region Save & Load
    internal void Save()
    {
        var data = new AppData
        {
            transactionsTable = Transactions.ToList(),
            monthlyBudget = MonthlyBudgets,
            commitments = Commitments.ToList()
        };

        Directory.CreateDirectory(Path.GetDirectoryName(dataFilePath)!);
        File.WriteAllText(dataFilePath, JsonSerializer.Serialize(data, _jsonOptions));
    }

    public void Load()
    {
        if (!File.Exists(dataFilePath)) return;

        var data = JsonSerializer.Deserialize<AppData>(File.ReadAllText(dataFilePath));
        if (data == null) return;
        
        Transactions.Clear();
        foreach (var transaction in data.transactionsTable ?? [])
        {
            Transactions.Add(transaction);
        }
        
        Commitments.Clear();
        foreach (var commitment in data.commitments ?? [])
        {
            Commitments.Add(commitment);
        }
        
        MonthlyBudgets.Clear();
        MonthlyBudgets.AddRange(data.monthlyBudget ?? []);

        if (MonthlyBudgets.Count == 0 && data.IncomeGoal > 0)
        {
            var today = DateTime.Today;
            GetorCreateBudget(today.Year, today.Month).IncomeGoal = data.IncomeGoal;
        }
    }
    #endregion
}