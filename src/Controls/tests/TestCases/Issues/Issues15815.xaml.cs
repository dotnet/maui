using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{

	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 15815, "Horizontal CollectionView does not show the last element under some condition", PlatformAffected.iOS)]
	public partial class Issues15815 : ContentPage
	{
		public Issues15815()
		{
			InitializeComponent();
			col.ItemsSource = new ObservableCollection<ItemViewModel>
				{
					new ItemViewModel { Index = 0, Color = Colors.Red, Width = 50 },
					new ItemViewModel { Index = 1, Color = Colors.Green, Width = 100 },
					new ItemViewModel { Index = 2, Color = Colors.Blue, Width = 100 },
				};
		}

		class ItemViewModel
		{
			public Color Color { get; set; }
			public int Width { get; set; }
			public int Index { get; set; }
			public string Id => $"id-{Index}";
			public string Name => $"Item {Index}";
		}
	}
}