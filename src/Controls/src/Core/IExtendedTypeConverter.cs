using System;

namespace Microsoft.Maui.Controls;

public interface IExtendedTypeConverter
{
	object? ConvertFromInvariantString(string value, IServiceProvider serviceProvider);
}