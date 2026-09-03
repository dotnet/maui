using System;
using Microsoft.Maui.Controls.Internals;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.WPF.FontNamedSizeService))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
{
	class FontNamedSizeService : IFontNamedSizeService
	{
		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			switch (size)
			{
				case NamedSize.Default:
					if (typeof(Label).IsAssignableFrom(targetElementType))
						return (double)System.Windows.Application.Current.Resources["FontSizeNormal"];
					return (double)System.Windows.Application.Current.Resources["FontSizeMedium"];
				case NamedSize.Micro:
					return (double)System.Windows.Application.Current.Resources["FontSizeSmall"] - 3;
				case NamedSize.Small:
					return (double)System.Windows.Application.Current.Resources["FontSizeSmall"];
				case NamedSize.Medium:
					if (useOldSizes)
						goto case NamedSize.Default;
					return (double)System.Windows.Application.Current.Resources["FontSizeMedium"];
				case NamedSize.Large:
					return (double)System.Windows.Application.Current.Resources["FontSizeLarge"];
				case NamedSize.Body:
					return (double)System.Windows.Application.Current.Resources["FontSizeBody"];
				case NamedSize.Caption:
					return (double)System.Windows.Application.Current.Resources["FontSizeCaption"];
				case NamedSize.Header:
					return (double)System.Windows.Application.Current.Resources["FontSizeHeader"];
				case NamedSize.Subtitle:
					return (double)System.Windows.Application.Current.Resources["FontSizeSubtitle"];
				case NamedSize.Title:
					return (double)System.Windows.Application.Current.Resources["FontSizeTitle"];
				default:
					throw new ArgumentOutOfRangeException("size");
			}
		}
	}
}