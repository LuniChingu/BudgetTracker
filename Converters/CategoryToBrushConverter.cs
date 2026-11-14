using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using BudgetTracker.Models;

namespace BudgetTracker.Converters;

public class CategoryToBrushConverter : IValueConverter
{
    private static readonly Brush NeedsBrush   = MakeBrush("#b09bba");
    private static readonly Brush WantsBrush   = MakeBrush("#a6ad9d");
    private static readonly Brush SavingsBrush = MakeBrush("#dca28e");
    private static readonly Brush UnknownBrush = MakeBrush("#D5D1CB");

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var category = value as string ?? "";

        if (Categories.Needs.Contains(category))   return NeedsBrush;
        if (Categories.Wants.Contains(category))   return WantsBrush;
        if (Categories.Savings.Contains(category)) return SavingsBrush;
        return UnknownBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static Brush MakeBrush(string hex)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        brush.Freeze(); // frozen brushes are read-only and slightly faster
        return brush;
    }
}