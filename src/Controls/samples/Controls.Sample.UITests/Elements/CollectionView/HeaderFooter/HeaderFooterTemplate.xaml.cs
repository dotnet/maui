using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Controls.Sample.UITests.Elements
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
			string _headerFooter;

			readonly DemoFilteredItemSource _demoFilteredItemSource = new DemoFilteredItemSource(3);

			public event PropertyChangedEventHandler PropertyChanged;

			public HeaderFooterDemoModel()
			{
				HeaderFooter = "Header Footer";
			}

			void OnPropertyChanged([CallerMemberName] string property = "")
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
			}

			public ObservableCollection<CollectionViewGalleryTestItem> Items => _demoFilteredItemSource.Items;

			public ICommand TapCommand => new Command(() => { HeaderFooter = "Updated"; });

			public string HeaderFooter
			{
				get => _headerFooter;
				set
				{
					if (value == _headerFooter)
					{
						return;
					}

					_headerFooter = value;
					OnPropertyChanged();
				}
			}
		}
	}
}