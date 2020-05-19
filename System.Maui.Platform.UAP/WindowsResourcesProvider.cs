using global::Windows.UI.Text;
using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Controls;
using System.Maui.Internals;
using WStyle = global::Windows.UI.Xaml.Style;

namespace System.Maui.Platform.UWP
{
	internal sealed class WindowsResourcesProvider : ISystemResourcesProvider
	{
		public IResourceDictionary GetSystemResources()
		{
			var prototype = new TextBlock();

			return new ResourceDictionary
			{
				[Device.Styles.TitleStyleKey] = GetStyle("HeaderTextBlockStyle", prototype),
				[Device.Styles.SubtitleStyleKey] = GetStyle("SubheaderTextBlockStyle", prototype),
				[Device.Styles.BodyStyleKey] = GetStyle("BodyTextBlockStyle", prototype),
				[Device.Styles.CaptionStyleKey] = GetStyle("CaptionTextBlockStyle", prototype),
				[Device.Styles.ListItemDetailTextStyleKey] = GetStyle("BodyTextBlockStyle", prototype),

				[Device.Styles.ListItemTextStyleKey] = GetStyle("BaseTextBlockStyle", prototype),
			};
		}

		Style GetStyle(object nativeKey, TextBlock prototype)
		{
			var style = (WStyle)global::Windows.UI.Xaml.Application.Current.Resources[nativeKey];

			prototype.Style = style;

			var formsStyle = new Style(typeof(Label));

			formsStyle.Setters.Add(Label.FontSizeProperty, prototype.FontSize);
			formsStyle.Setters.Add(Label.FontFamilyProperty, prototype.FontFamily.Source);
			formsStyle.Setters.Add(Label.FontAttributesProperty, ToAttributes(prototype.FontWeight));			

			return formsStyle;
		}

		static FontAttributes ToAttributes(FontWeight fontWeight)
		{
			if (fontWeight.Weight == FontWeights.Bold.Weight || fontWeight.Weight == FontWeights.SemiBold.Weight 
				|| fontWeight.Weight == FontWeights.ExtraBold.Weight)
			{
				return FontAttributes.Bold;
			}

			return FontAttributes.None;
		}
	}
}