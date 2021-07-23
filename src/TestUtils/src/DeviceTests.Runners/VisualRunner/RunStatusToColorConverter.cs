using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
    class RunStatusToColorConverter : IValueConverter
    {
        internal static readonly Color NoTestColor = Color.FromArgb("#ff7f00");
        internal static readonly Color SkippedColor = Color.FromArgb("#ff7700");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RunStatus status)
            {
                switch (status)
                {
                    case RunStatus.Ok:
                        return Colors.Green;
                    case RunStatus.Failed:
                        return Colors.Red;
                    case RunStatus.NoTests:
                        return NoTestColor;
                    case RunStatus.Skipped:
                        return SkippedColor;
                    case RunStatus.NotRun:
                        return Colors.DarkGray;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return Colors.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}