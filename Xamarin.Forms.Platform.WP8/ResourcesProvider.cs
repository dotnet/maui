using System.Windows.Controls;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WinPhone
{
	internal class ResourcesProvider : ISystemResourcesProvider
	{
		ResourceDictionary _dictionary;

		public IResourceDictionary GetSystemResources()
		{
			_dictionary = new ResourceDictionary();

			UpdateStyles();

			return _dictionary;
		}

		Style GetListItemDetailTextStyle()
		{
			var result = new Style(typeof(Label));

			result.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = 32 });

			return result;
		}

		Style GetListItemTextStyle()
		{
			var result = new Style(typeof(Label));

			result.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = 48 });

			return result;
		}

		Style GetStyle(System.Windows.Style style, TextBlock hackbox)
		{
			hackbox.Style = style;

			var result = new Style(typeof(Label));
			result.Setters.Add(new Setter { Property = Label.FontFamilyProperty, Value = hackbox.FontFamily });

			result.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = hackbox.FontSize });

			return result;
		}

		void UpdateStyles()
		{
			var textBlock = new TextBlock();
			_dictionary[Device.Styles.TitleStyleKey] = GetStyle((System.Windows.Style)System.Windows.Application.Current.Resources["PhoneTextTitle1Style"], textBlock);
			_dictionary[Device.Styles.SubtitleStyleKey] = GetStyle((System.Windows.Style)System.Windows.Application.Current.Resources["PhoneTextTitle2Style"], textBlock);
			_dictionary[Device.Styles.BodyStyleKey] = GetStyle((System.Windows.Style)System.Windows.Application.Current.Resources["PhoneTextNormalStyle"], textBlock);
			_dictionary[Device.Styles.CaptionStyleKey] = GetStyle((System.Windows.Style)System.Windows.Application.Current.Resources["PhoneTextSmallStyle"], textBlock);
			_dictionary[Device.Styles.ListItemTextStyleKey] = GetListItemTextStyle();
			_dictionary[Device.Styles.ListItemDetailTextStyleKey] = GetListItemDetailTextStyle();
		}
	}
}