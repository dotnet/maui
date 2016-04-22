using System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	internal sealed class WindowsResourcesProvider : ISystemResourcesProvider
	{
		public IResourceDictionary GetSystemResources()
		{
			Windows.UI.Xaml.ResourceDictionary windowsResources = Windows.UI.Xaml.Application.Current.Resources;

			var resources = new ResourceDictionary();
			resources[Device.Styles.TitleStyleKey] = GetStyle("HeaderTextBlockStyle");
			resources[Device.Styles.SubtitleStyleKey] = GetStyle("SubheaderTextBlockStyle");
			resources[Device.Styles.BodyStyleKey] = GetStyle("BodyTextBlockStyle");
			resources[Device.Styles.CaptionStyleKey] = GetStyle("CaptionTextBlockStyle");
#if WINDOWS_UWP
			resources[Device.Styles.ListItemTextStyleKey] = GetStyle("BaseTextBlockStyle");
#else
			resources[Device.Styles.ListItemTextStyleKey] = GetStyle("TitleTextBlockStyle");
#endif
			resources[Device.Styles.ListItemDetailTextStyleKey] = GetStyle("BodyTextBlockStyle");
			return resources;
		}

		Style GetStyle(object nativeKey)
		{
			var style = (Windows.UI.Xaml.Style)Windows.UI.Xaml.Application.Current.Resources[nativeKey];

			var formsStyle = new Style(typeof(Label));
			foreach (SetterBase b in style.Setters)
			{
				var setter = b as Windows.UI.Xaml.Setter;
				if (setter == null)
					continue;

				// TODO: Need to implement a stealth pass-through for things we don't support

				try
				{
					if (setter.Property == TextBlock.FontSizeProperty)
						formsStyle.Setters.Add(Label.FontSizeProperty, setter.Value);
					else if (setter.Property == TextBlock.FontFamilyProperty)
						formsStyle.Setters.Add(Label.FontFamilyProperty, setter.Value);
					else if (setter.Property == TextBlock.FontWeightProperty)
						formsStyle.Setters.Add(Label.FontAttributesProperty, ToAttributes(Convert.ToUInt16(setter.Value)));
					else if (setter.Property == TextBlock.TextWrappingProperty)
						formsStyle.Setters.Add(Label.LineBreakModeProperty, ToLineBreakMode((TextWrapping)setter.Value));
				}
				catch (NotImplementedException)
				{
					// see https://bugzilla.xamarin.com/show_bug.cgi?id=33135
					// WinRT implementation of Windows.UI.Xaml.Setter.get_Value is not implemented.
				}
			}

			return formsStyle;
		}

		static FontAttributes ToAttributes(ushort uweight)
		{
			if (uweight == FontWeights.Bold.Weight || uweight == FontWeights.SemiBold.Weight || uweight == FontWeights.ExtraBold.Weight)
				return FontAttributes.Bold;

			return FontAttributes.None;
		}

		static LineBreakMode ToLineBreakMode(TextWrapping value)
		{
			switch (value)
			{
				case TextWrapping.Wrap:
					return LineBreakMode.CharacterWrap;
				case TextWrapping.WrapWholeWords:
					return LineBreakMode.WordWrap;
				default:
				case TextWrapping.NoWrap:
					return LineBreakMode.NoWrap;
			}
		}
	}
}