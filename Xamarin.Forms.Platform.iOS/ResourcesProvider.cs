using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	internal class ResourcesProvider : ISystemResourcesProvider
	{
		ResourceDictionary _dictionary;

		public ResourcesProvider()
		{
			if (Forms.IsiOS7OrNewer)
				UIApplication.Notifications.ObserveContentSizeCategoryChanged((sender, args) => UpdateStyles());
		}

		public IResourceDictionary GetSystemResources()
		{
			_dictionary = new ResourceDictionary();
			UpdateStyles();

			return _dictionary;
		}

		Style GenerateListItemDetailTextStyle()
		{
			var font = new UITableViewCell(UITableViewCellStyle.Subtitle, "Foobar").DetailTextLabel.Font;
			return GenerateStyle(font);
		}

		Style GenerateListItemTextStyle()
		{
			var font = new UITableViewCell(UITableViewCellStyle.Subtitle, "Foobar").TextLabel.Font;
			return GenerateStyle(font);
		}

		Style GenerateStyle(UIFont font)
		{
			var result = new Style(typeof(Label));

			result.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = (double)font.PointSize });

			result.Setters.Add(new Setter { Property = Label.FontFamilyProperty, Value = font.Name });

			return result;
		}

		void UpdateStyles()
		{
			if (Forms.IsiOS7OrNewer)
			{
				_dictionary[Device.Styles.TitleStyleKey] = GenerateStyle(UIFont.PreferredHeadline);
				_dictionary[Device.Styles.SubtitleStyleKey] = GenerateStyle(UIFont.PreferredSubheadline);
				_dictionary[Device.Styles.BodyStyleKey] = GenerateStyle(UIFont.PreferredBody);
				_dictionary[Device.Styles.CaptionStyleKey] = GenerateStyle(UIFont.PreferredCaption1);
			}
			else
			{
				_dictionary[Device.Styles.TitleStyleKey] = GenerateStyle(UIFont.BoldSystemFontOfSize(17));
				_dictionary[Device.Styles.SubtitleStyleKey] = GenerateStyle(UIFont.SystemFontOfSize(15));
				_dictionary[Device.Styles.BodyStyleKey] = GenerateStyle(UIFont.SystemFontOfSize(17));
				_dictionary[Device.Styles.CaptionStyleKey] = GenerateStyle(UIFont.SystemFontOfSize(12));
			}

			_dictionary[Device.Styles.ListItemTextStyleKey] = GenerateListItemTextStyle();
			_dictionary[Device.Styles.ListItemDetailTextStyleKey] = GenerateListItemDetailTextStyle();
		}
	}
}