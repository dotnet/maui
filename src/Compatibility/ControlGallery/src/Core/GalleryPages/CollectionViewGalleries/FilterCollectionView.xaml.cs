using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.CollectionViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FilterCollectionView : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();

		public FilterCollectionView()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();

			CollectionView.ItemsSource = _demoFilteredItemSource.Items;

			SearchBar.TextChanged += SearchBarOnTextChanged;
			UseEmptyView.Toggled += UseEmptyViewOnToggled;

			UpdateEmptyView();
		}

		void UseEmptyViewOnToggled(object sender, ToggledEventArgs e)
		{
			UpdateEmptyView();
		}

		void UpdateEmptyView()
		{
			if (UseEmptyView.IsToggled)
			{
				CollectionView.EmptyView = new Label
				{
					Text = "Nothing to see here",
					TextColor = Colors.Coral,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center,
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill
				};
			}
			else
			{
				CollectionView.EmptyView = null;
			}
		}

		void SearchBarOnTextChanged(object sender, TextChangedEventArgs e)
		{
			_demoFilteredItemSource.FilterItems(e.NewTextValue);
		}
	}
}