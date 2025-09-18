using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListDynamicCellSizeChangePage : ContentPage
	{
		public VerticalListDynamicCellSizeChangePage()
		{
			InitializeComponent();
			BindingContext = this;
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			Device.StartTimer(TimeSpan.FromSeconds(3), () =>
			{
				UpdateStuff();
				return false;
			});
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
		}
		ObservableCollection<SomeItem> _items;
		public ObservableCollection<SomeItem> Items
		{
			get
			{
				if (_items == null)
				{
					_items = new ObservableCollection<SomeItem>(Enumerable.Range(0, 5).Select(c =>
					{
						return new SomeItem() { Name = string.Format("Item {0}", c) };
					}));
				}

				return _items;
			}
		}

		public class SomeItem : INotifyPropertyChanged
		{
			string name;
			public string Name
			{
				get => name;
				set
				{
					name = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;
		}

		void UpdateStuff()
		{
			System.Diagnostics.Debug.WriteLine($"UpdateStuff");
			var bigString = "The .NET Multi-platform App UI (.NET MAUI) CollectionView is a view for presenting lists of data using different layout specifications. It aims to provide a more flexible, and performant alternative to ListView.";
			foreach (var item in Items)
			{
				item.Name = bigString;
			}
		}
	}
}



