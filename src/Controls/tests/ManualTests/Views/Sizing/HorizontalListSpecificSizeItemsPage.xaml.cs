using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class HorizontalListSpecificSizeItemsPage : ContentPage
	{
		public HorizontalListSpecificSizeItemsPage()
		{
			InitializeComponent();
			BindingContext = new SpecificItemsViewModel();
		}
	}
	public partial class ItemViewModel : ObservableObject
	{
		[ObservableProperty]
		private Color _color;
		[ObservableProperty]
		private int _width;
		[ObservableProperty]
		private int _index;
	}

	public partial class SpecificItemsViewModel : ObservableObject
	{
		[ObservableProperty]
		private ObservableCollection<ItemViewModel> _items;

		[ObservableProperty]
		private ObservableCollection<ItemViewModel> _items2;

		public SpecificItemsViewModel()
		{
			_items = new ObservableCollection<ItemViewModel>();
			_items.Add(new ItemViewModel { Index = 0, Color = GetColor(0), Width = 100 });
			for (int i = 1; i < 16; i++)
			{
				_items.Add(new ItemViewModel { Index = i, Color = GetColor(i), Width = 50 });
			}

			_items.Add(new ItemViewModel { Index = 16, Color = GetColor(16 + 1), Width = 100 });
			_items.Add(new ItemViewModel { Index = 17, Color = GetColor(17 + 1), Width = 100 });


			_items2 = new ObservableCollection<ItemViewModel>();

			_items2.Add(new ItemViewModel { Index = 0, Color = GetColor(0), Width = 50 });
			for (int i = 1; i < 16; i++)
			{
				_items2.Add(new ItemViewModel { Index = i, Color = GetColor(i + 1), Width = 50 });
			}
			_items2.Add(new ItemViewModel { Index = 16, Color = GetColor(16 + 1), Width = 100 });
			_items2.Add(new ItemViewModel { Index = 17, Color = GetColor(17 + 1), Width = 100 });
		}

		private Color GetColor(int i)
		{
			switch (i % 4)
			{
				case 0:
					return Color.FromRgb(90, 140, 115);
				case 1:
					return Color.FromRgb(243, 226, 148);
				case 2:
					return Color.FromRgb(240, 175, 115);
				case 3:
					return Color.FromRgb(217, 100, 90);
				default:
					return Color.FromRgb(0, 0, 0);
			}
		}
	}
}

