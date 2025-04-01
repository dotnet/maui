using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.CollectionViewGalleries.HeaderFooterGalleries
{
	public partial class HeaderFooterTemplate : ContentPage
	{
		public HeaderFooterTemplate()
		{
			InitializeComponent();

			CollectionView.ItemTemplate = ExampleTemplates.PhotoTemplate();

			BindingContext = new HeaderFooterDemoModel();
		}


		class HeaderFooterDemoModel : INotifyPropertyChanged
		{
			readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource(3);
			DateTime _currentTime;

			public event PropertyChangedEventHandler PropertyChanged;

			public HeaderFooterDemoModel()
			{
				CurrentTime = new DateTime(2023, 1, 1);
			}

			void OnPropertyChanged([CallerMemberName] string property = "")
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
			}

			public ObservableCollection<CollectionViewGalleryTestItem> Items => _demoFilteredItemSource.Items;

			public ICommand TapCommand => new Command(() => { CurrentTime = new DateTime(2024, 1, 1); });

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