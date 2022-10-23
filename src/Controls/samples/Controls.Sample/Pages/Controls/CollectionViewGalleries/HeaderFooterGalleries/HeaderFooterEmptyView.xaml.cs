using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.HeaderFooterGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HeaderFooterEmptyView : ContentPage
	{
		public HeaderFooterEmptyView()
		{
			InitializeComponent();
			UpdateEmptyView();
		}

		LayoutOptions _horizontalOptions = LayoutOptions.Center;
		LayoutOptions _verticalOptions = LayoutOptions.Center;

		void SetHorizontalOptions(object sender, CheckedChangedEventArgs _)
		{
			var radioButton = (RadioButton)sender;
			if (radioButton.IsChecked)
			{
				_horizontalOptions = (LayoutOptions)radioButton.Value;
				UpdateEmptyView();
			}
		}

		void SetVerticalOptions(object sender, CheckedChangedEventArgs _)
		{
			var radioButton = (RadioButton)sender;
			if (radioButton.IsChecked)
			{
				_verticalOptions = (LayoutOptions)radioButton.Value;
				UpdateEmptyView();
			}
		}

		void UpdateEmptyView()
		{
			Collection.EmptyView = new Label
			{
				Text = "Nothing to show",
				HorizontalOptions = _horizontalOptions,
				VerticalOptions = _verticalOptions
			};
		}
	}
}