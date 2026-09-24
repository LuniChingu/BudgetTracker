using System.Collections.ObjectModel;
using System.Text.Json;
using System.IO;
using BudgetTracker.Models;

namespace BudgetTracker.Services;


public class DataStore
{
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    private readonly string dataFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BudgetTracker", "AppData.json");

    public ObservableCollection<Transaction> Transactions { get; } = [];
    public decimal IncomeGoal { get; set; } = 1500m;

    internal void Save()
    {
        var data = new AppData
        {
            transactionsTable = Transactions.ToList(),
            IncomeGoal = IncomeGoal
        };

        Directory.CreateDirectory(Path.GetDirectoryName(dataFilePath)!);
        File.WriteAllText(dataFilePath, JsonSerializer.Serialize(data, _jsonOptions));
    }

    public void Load()
    {
        if (!File.Exists(dataFilePath)) return;

        var data = JsonSerializer.Deserialize<AppData>(File.ReadAllText(dataFilePath));
        if (data == null) return;
        
        IncomeGoal = data.IncomeGoal;
        Transactions.Clear();
        foreach (var transaction in data.transactionsTable ?? [])
        {
            Transactions.Add(transaction);
        }
    }
}