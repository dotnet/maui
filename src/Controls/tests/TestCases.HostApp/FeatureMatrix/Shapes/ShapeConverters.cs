using System.Globalization;
using Microsoft.Maui.Controls.Shapes;


namespace Maui.Controls.Sample;

public class StringToPointCollectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string pointsString && !string.IsNullOrWhiteSpace(pointsString))
        {
            var points = new PointCollection();
            var pointPairs = pointsString.Split(' ');

            foreach (var pointPair in pointPairs)
            {
                var coords = pointPair.Split(',');
                if (coords.Length == 2 &&
                    double.TryParse(coords[0], out double x) &&
                    double.TryParse(coords[1], out double y))
                {
                    points.Add(new Point(x, y));
                }
            }
            return points;
        }
        return new PointCollection();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PointCollection points)
        {
            var pointStrings = new List<string>();
            foreach (Point point in points)
            {
                pointStrings.Add($"{point.X},{point.Y}");
            }
            return string.Join(" ", pointStrings);
        }
        return string.Empty;
    }
}

public class StringToPathGeometryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string pathData && !string.IsNullOrWhiteSpace(pathData))
        {
            try
            {
                return (Geometry)new PathGeometryConverter().ConvertFromInvariantString(pathData);
            }
            catch
            {
                // Return a simple default path if parsing fails
                return (Geometry)new PathGeometryConverter().ConvertFromInvariantString("M 10,100 L 100,100");
            }
        }
        return (Geometry)new PathGeometryConverter().ConvertFromInvariantString("M 10,100 L 100,100");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() ?? string.Empty;
    }
}

public class StringToDoubleCollectionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string dashArray && !string.IsNullOrWhiteSpace(dashArray))
        {
            var collection = new DoubleCollection();
            var values = dashArray.Split(',');

            foreach (var val in values)
            {
                if (double.TryParse(val.Trim(), out double dashValue))
                {
                    collection.Add(dashValue);
                }
            }
            return collection;
        }
        return new DoubleCollection();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DoubleCollection collection)
        {
            return string.Join(",", collection);
        }
        return string.Empty;
    }
}

