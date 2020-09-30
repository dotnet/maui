using System;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class DatePickerCoreGalleryPage : CoreGalleryPage<DatePicker>
	{
		protected override bool SupportsTapGestureRecognizer => false;

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			var dateContainer = new ViewContainer<DatePicker>(Test.DatePicker.Date,
				new DatePicker { Date = new DateTime(1987, 9, 13) });

			var dateSelectedContainer = new EventViewContainer<DatePicker>(Test.DatePicker.DateSelected, new DatePicker());
			dateSelectedContainer.View.DateSelected += (sender, args) => dateSelectedContainer.EventFired();

			var formatDateContainer = new ViewContainer<DatePicker>(Test.DatePicker.Format, new DatePicker { Format = "ddd" });
			var minimumDateContainer = new ViewContainer<DatePicker>(Test.DatePicker.MinimumDate,
				new DatePicker { MinimumDate = new DateTime(1987, 9, 13) });
			var maximumDateContainer = new ViewContainer<DatePicker>(Test.DatePicker.MaximumDate,
				new DatePicker { MaximumDate = new DateTime(2087, 9, 13) });
			var textColorContainer = new ViewContainer<DatePicker>(Test.DatePicker.TextColor,
				new DatePicker { Date = new DateTime(1978, 12, 24), TextColor = Color.Lime });
			var fontAttributesContainer = new ViewContainer<DatePicker>(Test.DatePicker.FontAttributes,
				new DatePicker { FontAttributes = FontAttributes.Bold });

			var fontFamilyContainer = new ViewContainer<DatePicker>(Test.DatePicker.FontFamily,
				new DatePicker());
			// Set font family based on available fonts per platform
			switch (Device.RuntimePlatform)
			{
				case Device.Android:
					fontFamilyContainer.View.FontFamily = "sans-serif-thin";
					break;
				case Device.iOS:
					fontFamilyContainer.View.FontFamily = "Courier";
					break;
				case Device.WPF:
					fontFamilyContainer.View.FontFamily = "Comic Sans MS";
					break;
				default:
					fontFamilyContainer.View.FontFamily = "Garamond";
					break;
			}

			var fontSizeContainer = new ViewContainer<DatePicker>(Test.DatePicker.FontSize,
				new DatePicker { FontSize = 32 });

			Add(dateContainer);
			Add(dateSelectedContainer);
			Add(formatDateContainer);
			Add(minimumDateContainer);
			Add(maximumDateContainer);
			Add(textColorContainer);
			Add(fontAttributesContainer);
			Add(fontFamilyContainer);
			Add(fontSizeContainer);
		}
	}
}