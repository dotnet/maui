using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.CollectionViewGalleries.HeaderFooterGalleries
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
				CurrentTime = new DateTime(2023,1,1);
			}

			void OnPropertyChanged([CallerMemberName] string property = "")
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
			}

			public ObservableCollection<CollectionViewGalleryTestItem> Items => _demoFilteredItemSource.Items;

			public ICommand TapCommand => new Command(() => { CurrentTime = new DateTime(2024,1,1); });

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