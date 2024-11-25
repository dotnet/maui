#nullable disable
using System;
using Android.Content.Res;
using Android.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using AndroidResource = Android.Resource;

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.Android.FontNamedSizeService))]
#pragma warning restore CS0612 // Type or member is obsolete

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[Obsolete]
	class FontNamedSizeService : IFontNamedSizeService
	{
		double _buttonDefaultSize;
		double _editTextDefaultSize;
		double _labelDefaultSize;
		double _largeSize;
		double _mediumSize;

		double _microSize;
		double _smallSize;

		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			if (_smallSize == 0)
			{
				_smallSize = ConvertTextAppearanceToSize(AndroidResource.Attribute.TextAppearanceSmall, AndroidResource.Style.TextAppearanceDeviceDefaultSmall, 12);
				_mediumSize = ConvertTextAppearanceToSize(AndroidResource.Attribute.TextAppearanceMedium, AndroidResource.Style.TextAppearanceDeviceDefaultMedium, 14);
				_largeSize = ConvertTextAppearanceToSize(AndroidResource.Attribute.TextAppearanceLarge, AndroidResource.Style.TextAppearanceDeviceDefaultLarge, 18);
				_buttonDefaultSize = ConvertTextAppearanceToSize(AndroidResource.Attribute.TextAppearanceButton, AndroidResource.Style.TextAppearanceDeviceDefaultWidgetButton, 14);
				_editTextDefaultSize = ConvertTextAppearanceToSize(AndroidResource.Style.TextAppearanceWidgetEditText, AndroidResource.Style.TextAppearanceDeviceDefaultWidgetEditText, 18);
				_labelDefaultSize = _smallSize;
				// as decreed by the android docs, ALL HAIL THE ANDROID DOCS, ALL GLORY TO THE DOCS, PRAISE HYPNOTOAD
				_microSize = Math.Max(1, _smallSize - (_mediumSize - _smallSize));
			}

			if (useOldSizes)
			{
				switch (size)
				{
					case NamedSize.Default:
						if (typeof(Button).IsAssignableFrom(targetElementType))
							return _buttonDefaultSize;
						if (typeof(Label).IsAssignableFrom(targetElementType))
							return _labelDefaultSize;
						if (typeof(Editor).IsAssignableFrom(targetElementType) || typeof(Entry).IsAssignableFrom(targetElementType) || typeof(SearchBar).IsAssignableFrom(targetElementType))
							return _editTextDefaultSize;
						return 14;
					case NamedSize.Micro:
						return 10;
					case NamedSize.Small:
						return 12;
					case NamedSize.Medium:
						return 14;
					case NamedSize.Large:
						return 18;
					case NamedSize.Body:
						return 16;
					case NamedSize.Caption:
						return 12;
					case NamedSize.Header:
						return 14;
					case NamedSize.Subtitle:
						return 16;
					case NamedSize.Title:
						return 24;
					default:
						throw new ArgumentOutOfRangeException(nameof(size));
				}
			}
			switch (size)
			{
				case NamedSize.Default:
					if (typeof(Button).IsAssignableFrom(targetElementType))
						return _buttonDefaultSize;
					if (typeof(Label).IsAssignableFrom(targetElementType))
						return _labelDefaultSize;
					if (typeof(Editor).IsAssignableFrom(targetElementType) || typeof(Entry).IsAssignableFrom(targetElementType))
						return _editTextDefaultSize;
					return _mediumSize;
				case NamedSize.Micro:
					return _microSize;
				case NamedSize.Small:
					return _smallSize;
				case NamedSize.Medium:
					return _mediumSize;
				case NamedSize.Large:
					return _largeSize;
				case NamedSize.Body:
					return 16;
				case NamedSize.Caption:
					return 12;
				case NamedSize.Header:
					return 14;
				case NamedSize.Subtitle:
					return 16;
				case NamedSize.Title:
					return 24;
				default:
					throw new ArgumentOutOfRangeException(nameof(size));
			}
		}

		double ConvertTextAppearanceToSize(int themeDefault, int deviceDefault, double defaultValue)
		{
			double myValue;

			if (TryGetTextAppearance(themeDefault, out myValue) && myValue > 0)
				return myValue;
			if (TryGetTextAppearance(deviceDefault, out myValue) && myValue > 0)
				return myValue;
			return defaultValue;
		}

		global::Android.Content.Context _applicationContext;
		bool TryGetTextAppearance(int appearance, out double val)
		{
			val = 0;
			try
			{
				using (var value = new TypedValue())
				{
					_applicationContext ??= global::Android.App.Application.Context.ApplicationContext;
					if (_applicationContext.Theme.ResolveAttribute(appearance, value, true))
					{
						var textSizeAttr = new[] { AndroidResource.Attribute.TextSize };
						const int indexOfAttrTextSize = 0;
						using (TypedArray array = _applicationContext.ObtainStyledAttributes(value.Data, textSizeAttr))
						{
							val = _applicationContext.FromPixels(array.GetDimensionPixelSize(indexOfAttrTextSize, -1));
							return true;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<FontNamedSizeService>()?
					.LogWarning(ex, "Error retrieving text appearance");
			}
			return false;
		}
	}
}