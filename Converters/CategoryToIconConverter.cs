using System.Globalization;
using System.Windows.Data;
using FontAwesome.Sharp;

namespace BudgetTracker.Converters;

public class CategoryToIconConverter : IValueConverter
{
    private static readonly Dictionary<string, IconChar> Icons = new()
    {
        ["Housing"] = IconChar.HouseChimney,
        ["Transport"] = IconChar.CarSide,
        ["Groceries"] = IconChar.CartShopping,
        ["Essentials"] = IconChar.BuildingColumns,
        ["Shopping"] = IconChar.BagShopping,
        ["Entertainment"] = IconChar.Gamepad,
        ["Travel"] = IconChar.SuitcaseRolling,
        ["Electronics"] = IconChar.Laptop,
        ["Miscellaneous"] = IconChar.Tags,
        ["Savings"] = IconChar.PiggyBank,
        ["Emergency"] = IconChar.TriangleExclamation,
        ["Debt"] = IconChar.CreditCard,
        ["Other"] = IconChar.Ellipsis,
    };
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string name && Icons.TryGetValue(name, out var icon) ? icon : IconChar.Tag;
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}