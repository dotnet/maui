using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.EmptyViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EmptyViewTemplateGallery : ContentPage
	{
		readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource();
		readonly EmptyViewGalleryFilterInfo _emptyViewGalleryFilterInfo = new EmptyViewGalleryFilterInfo();

		public EmptyViewTemplateGallery ()
		{
			InitializeComponent ();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();

			CollectionView.ItemsSource = _demoFilteredItemSource.Items;
			CollectionView.EmptyView = _emptyViewGalleryFilterInfo;

			SearchBar.TextChanged += SearchBarOnTextChanged;
		}

		void SearchBarOnTextChanged(object sender, TextChangedEventArgs e)
		{
			_demoFilteredItemSource.FilterItems(e.NewTextValue);
			_emptyViewGalleryFilterInfo.Filter = e.NewTextValue;
		}
	}

	[Preserve(AllMembers = true)]
	public class EmptyViewGalleryFilterInfo : INotifyPropertyChanged
	{
		string _filter;

		public string Filter
		{
			get => _filter;
			set
			{
				_filter = value; 
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}