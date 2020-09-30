using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.HeaderFooterGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HeaderFooterTemplate : ContentPage
	{
		public HeaderFooterTemplate()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();

			BindingContext = new HeaderFooterDemoModel();
		}

		[Preserve(AllMembers = true)]
		class HeaderFooterDemoModel : INotifyPropertyChanged
		{
			readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource(3);
			DateTime _currentTime;

			public event PropertyChangedEventHandler PropertyChanged;

			public HeaderFooterDemoModel()
			{
				CurrentTime = DateTime.Now;
			}

			void OnPropertyChanged([CallerMemberName] string property = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
			}

			public ObservableCollection<CollectionViewGalleryTestItem> Items => _demoFilteredItemSource.Items;

			public ICommand TapCommand => new Command(() => { CurrentTime = DateTime.Now; });

			public DateTime CurrentTime
			{
				get => _currentTime;
				set
				{
					if (value == _currentTime)
					{
						return;
					}

					_currentTime = value;
					OnPropertyChanged();
				}
			}
		}
	}
}