using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Controls.Internals;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public sealed partial class FormsFlyout : UserControl
	{
		ActionSheetArguments options;
		public event EventHandler OptionSelected;

		public FormsFlyout(ActionSheetArguments sheetOptions)
		{
			this.InitializeComponent();

			options = sheetOptions;

			TitleBlock.Text = options.Title ?? string.Empty;
			OptionsList.ItemsSource = options.Buttons.ToList();

			if (options.FlowDirection == Microsoft.Maui.Controls.FlowDirection.RightToLeft)
			{
				TitleBlock.FlowDirection = Microsoft.UI.Xaml.FlowDirection.RightToLeft;
				OptionsList.FlowDirection = Microsoft.UI.Xaml.FlowDirection.RightToLeft;
			}
			else if (options.FlowDirection == Microsoft.Maui.Controls.FlowDirection.LeftToRight)
			{
				TitleBlock.FlowDirection = Microsoft.UI.Xaml.FlowDirection.LeftToRight;
				OptionsList.FlowDirection = Microsoft.UI.Xaml.FlowDirection.LeftToRight;
			}

			if (options.FlowDirection == Microsoft.Maui.Controls.FlowDirection.RightToLeft)
			{
				if (options.Cancel != null)
				{
					LeftBtn.Content = options.Cancel;
					if (options.Destruction != null)
						RightBtn.Content = options.Destruction;
				}
				else if (options.Destruction != null)
					LeftBtn.Content = options.Destruction;
			}
			else
			{
				if (options.Cancel != null)
				{
					RightBtn.Content = options.Cancel;
					if (options.Destruction != null)
						LeftBtn.Content = options.Destruction;
				}
				else if (options.Destruction != null)
					RightBtn.Content = options.Destruction;
			}

			LeftBtn.Visibility = LeftBtn.Content == null ? WVisibility.Collapsed : WVisibility.Visible;
			RightBtn.Visibility = RightBtn.Content == null ? WVisibility.Collapsed : WVisibility.Visible;
		}

		void ListItemSelected (object sender, ItemClickEventArgs e)
		{
			var selection = (string)e.ClickedItem;
			options.SetResult(selection);

			OptionSelected?.Invoke(this, null);
		}

		void ActionButtonClicked(object sender, RoutedEventArgs e)
		{
			var button = (Microsoft.UI.Xaml.Controls.Button)sender;
			var selection = (string)button.Content;
			options.SetResult(selection);

			OptionSelected?.Invoke(this, null);
		}
	}
}
