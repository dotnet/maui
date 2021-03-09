#if __MOBILE__
using UIKit;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else

using Microsoft.Maui.Controls.Compatibility.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	[Preserve(AllMembers = true)]
	internal class ResourcesProvider : ISystemResourcesProvider
	{
		ResourceDictionary _dictionary;

		public ResourcesProvider()
		{
#if __MOBILE__
			UIApplication.Notifications.ObserveContentSizeCategoryChanged((sender, args) => UpdateStyles());
#endif
		}

		public IResourceDictionary GetSystemResources()
		{
			_dictionary = new ResourceDictionary();
			UpdateStyles();

			return _dictionary;
		}

#if __MOBILE__
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

			_dictionary[Device.Styles.TitleStyleKey] = GenerateStyle(UIFont.PreferredHeadline);
			_dictionary[Device.Styles.SubtitleStyleKey] = GenerateStyle(UIFont.PreferredSubheadline);
			_dictionary[Device.Styles.BodyStyleKey] = GenerateStyle(UIFont.PreferredBody);
			_dictionary[Device.Styles.CaptionStyleKey] = GenerateStyle(UIFont.PreferredCaption1);

			_dictionary[Device.Styles.ListItemTextStyleKey] = GenerateListItemTextStyle();
			_dictionary[Device.Styles.ListItemDetailTextStyleKey] = GenerateListItemDetailTextStyle();
		}
#else
		void UpdateStyles()
		{
		}
#endif

	}
}