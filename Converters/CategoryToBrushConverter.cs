using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BudgetTracker.Models;

namespace BudgetTracker.Converters;

public class CategoryToBrushConverter : IValueConverter
{
    private static readonly Dictionary<string, Brush> CategoryBrushes = new()
    {
        ["Housing"] = MakeBrush("#788DA5"),
        ["Transport"] = MakeBrush("#D0B16F"),
        ["Groceries"] = MakeBrush("#A0AD9A"),
        ["Essentials"] = MakeBrush("#8DA994"),

        ["Shopping"] = MakeBrush("#C48791"),
        ["Entertainment"] = MakeBrush("#A99BC9"),
        ["Travel"] = MakeBrush("#8C9DB8"),
        ["Electronics"] = MakeBrush("#79A6A3"),
        ["Miscellaneous"] = MakeBrush("#AC8BA8"),

        ["Savings"] = MakeBrush("#6F9A7D"),
        ["Emergency"] = MakeBrush("#D0A052"),
        ["Debt"] = MakeBrush("#B87578"),
        ["Other"] = MakeBrush("#B98B78"),
    };
    
    private static readonly Brush UnknownBrush = MakeBrush("#D5D1CB");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is string category && CategoryBrushes.TryGetValue(category, out var brush)
            ? brush
            : UnknownBrush;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static Brush MakeBrush(string hex)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        brush.Freeze(); // frozen brushes are read-only and slightly faster
        return brush;
    }
}