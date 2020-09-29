using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class BindableLayoutGalleryPage : ContentPage
	{
		public BindableLayoutGalleryPage()
		{
			InitializeComponent();
			BindingContext = new PageViewModel();
		}

		[Preserve(AllMembers = true)]
		class PageViewModel
		{
			public ObservableCollection<object> ItemsSource { get; set; }
			public ICommand AddItemCommand { get; }
			public ICommand RemoveItemCommand { get; }
			public ICommand ReplaceItemCommand { get; }
			public ICommand MoveItemCommand { get; }
			public ICommand ClearCommand { get; }

			public PageViewModel()
			{
				ItemsSource = new ObservableCollection<object>(Enumerable.Range(0, 10).Cast<object>().ToList());

				int i = ItemsSource.Count;
				AddItemCommand = new Command(() => ItemsSource.Add(i++));
				RemoveItemCommand = new Command(() =>
				{
					if (ItemsSource.Count > 0)
						ItemsSource.RemoveAt(0);
				});
				ReplaceItemCommand = new Command(() =>
				{
					// Switch between integers and character representation
					for (int i1 = 0; i1 < ItemsSource.Count; ++i1)
					{
						if (ItemsSource[i1] is int a)
						{
							ItemsSource[i1] = (char)('A' + a);
						}
						else
						{
							ItemsSource[i1] = (int)((char)ItemsSource[i1] - 'A');
						}
					}
				});
				MoveItemCommand = new Command(() =>
				{
					// Move first item to the last position
					if (ItemsSource.Count > 0)
					{
						ItemsSource.Move(0, ItemsSource.Count - 1);
					}
				});
				ClearCommand = new Command(() =>
				{
					ItemsSource.Clear();
				});
			}
		}
	}

	class BindableLayoutItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate IntTemplate { get; set; }
		public DataTemplate CharTemplate { get; set; }

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			return item is int ? IntTemplate : CharTemplate;
		}
	}
}