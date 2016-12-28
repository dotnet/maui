using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 38723, "Update Content in Picker's SelectedIndexChanged event causes NullReferenceException", PlatformAffected.iOS)]
	public class Bugzilla38723 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		Picker _datePicker;
		Label _dateLabel;

		protected override void Init()
		{
			_datePicker = new Picker
			{
				//HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.Center,
				Title = "Pick a Date",
			};
			_datePicker.SelectedIndexChanged += DatePickerSelected;

			for (var i = 0; i < 7; i++)
			{
				_datePicker.Items.Add(DateTime.Now.AddDays(i).ToString("dd, MMM, yyyy(dddd)"));
			}

			var stackLayout = new StackLayout
			{
				Padding = new Thickness(10, 10)
			};

			_dateLabel = new Label
			{
				HorizontalOptions = LayoutOptions.StartAndExpand,
				VerticalOptions = LayoutOptions.Center,
				Text = "Placeholder"
			};

			stackLayout.Children.Add(_datePicker);
			stackLayout.Children.Add(_dateLabel);
			// Update current page's UI would cause NullReferenceException
			Content = stackLayout;
		}

		void DatePickerSelected(object sender, EventArgs args)
		{
			_dateLabel.Text = args.ToString();
			Content = _dateLabel;
		}
	}
}