using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.WindowsSpecific;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1705, "[Enhancement] Icon on TabbedPage UWP",
		PlatformAffected.UWP, issueTestNumber: 2)]

	class Issue1705_2 : TabbedPage
	{
		ContentPage _page1;
		ContentPage _page2;
		ContentPage _page3;

		public Issue1705_2()
		{
			_page1 = new ContentPage { Title = "TabPage1", Icon = "bank.png" };
			_page1.Content = new StackLayout { Padding = new Thickness(0, 16), Children = { new Label { Text = "This is TabPage1 using bank.png icon.", FontAttributes = FontAttributes.Bold } } };
			_page2 = new ContentPage { Title = "TabPage2", Icon = "coffee.png" };
			_page2.Content = new StackLayout { Padding = new Thickness(0, 16), Children = { new Label { Text = "This is TabPage2 using coffee.png icon.", FontAttributes = FontAttributes.Bold } } };
			_page3 = new ContentPage { Title = "TabPage3" };
			_page3.Content = new StackLayout { Padding = new Thickness(0, 16), Children = { new Label { Text = "This is TabPage3 without icon.", FontAttributes = FontAttributes.Bold } } };

			Children.Add(_page1);
			Children.Add(_page2);
			Children.Add(_page3);

			if (Device.RuntimePlatform == Device.UWP)
				Children.Add(new HeaderIconsControlPage(this) { Title = "UWPSpecifics" });
		}
	}

	class HeaderIconsControlPage : ContentPage
	{
		TabbedPage _target;
		Button _toggleIconsButton;
		Entry _iconWidthEntry;
		Entry _iconHeightEntry;
		Button _changeIconsSizeButton;
		Button _getCurrentIconsSizeButton;

		public HeaderIconsControlPage(TabbedPage target)
		{
			_target = target;

			_toggleIconsButton = new Button();
			_toggleIconsButton.Text = "Show Header Icons";
			_toggleIconsButton.Clicked += (object sender, EventArgs e) => {
				if (_target.On<Windows>().IsHeaderIconsEnabled())
				{
					_target.On<Windows>().DisableHeaderIcons();
					_toggleIconsButton.Text = "Show Header Icons";
				}
				else
				{
					_target.On<Windows>().EnableHeaderIcons();
					_toggleIconsButton.Text = "Hide Header Icons";
				}
			};

			var iconWidthLabel = new Label { Text = "Head Icons Width:" };
			var iconHeightLabel = new Label { Text = "Head Icons Height:" };

			_iconWidthEntry = new Entry { Text = "16" };
			_iconHeightEntry = new Entry { Text = "16" };

			_changeIconsSizeButton = new Button();
			_changeIconsSizeButton.Text = "Change Header Icons Size";
			_changeIconsSizeButton.Clicked += (object sender, EventArgs e) => {
				int width;
				int height;

				if (!Int32.TryParse(_iconWidthEntry.Text, out width))
					width = 16;
				if (!Int32.TryParse(_iconHeightEntry.Text, out height))
					height = 16;

				var currentSize = _target.On<Windows>().GetHeaderIconsSize();
				if (currentSize.Width != width || currentSize.Height != height)
					_target.On<Windows>().SetHeaderIconsSize(new Size(width, height));
			};

			_getCurrentIconsSizeButton = new Button();
			_getCurrentIconsSizeButton.Text = "Load Current Header Icons Size";
			_getCurrentIconsSizeButton.Clicked += (object sender, EventArgs e) => {

				var currentSize = _target.On<Windows>().GetHeaderIconsSize();
				_iconWidthEntry.Text = currentSize.Width.ToString();
				_iconHeightEntry.Text = currentSize.Height.ToString();
			};

			Content = new StackLayout { Padding = new Thickness(0, 16), Children = {
					new Label { Text = "Control page for header icons on UWP.", FontAttributes = FontAttributes.Bold },
					_toggleIconsButton, iconWidthLabel, _iconWidthEntry, iconHeightLabel, _iconHeightEntry, _changeIconsSizeButton,
					_getCurrentIconsSizeButton
				}
			};
		}
	}
}
