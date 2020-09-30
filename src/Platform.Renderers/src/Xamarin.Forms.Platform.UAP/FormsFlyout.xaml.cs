using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.UWP
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

			if (options.Cancel != null)
			{
				RightBtn.Content = options.Cancel;

				if (options.Destruction != null)
					LeftBtn.Content = options.Destruction;
			}
			else if (options.Destruction != null)
				RightBtn.Content = options.Destruction;

			LeftBtn.Visibility = LeftBtn.Content == null ? Visibility.Collapsed : Visibility.Visible;
			RightBtn.Visibility = RightBtn.Content == null ? Visibility.Collapsed : Visibility.Visible;
		}

		void ListItemSelected (object sender, ItemClickEventArgs e)
		{
			var selection = (string)e.ClickedItem;
			options.SetResult(selection);

			OptionSelected?.Invoke(this, null);
		}

		void ActionButtonClicked(object sender, RoutedEventArgs e)
		{
			var button = (Windows.UI.Xaml.Controls.Button)sender;
			var selection = (string)button.Content;
			options.SetResult(selection);

			OptionSelected?.Invoke(this, null);
		}
	}
}
