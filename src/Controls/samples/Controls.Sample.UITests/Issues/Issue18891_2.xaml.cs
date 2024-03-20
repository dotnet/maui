using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "18891_2", "CollectionView with many items (10,000+) hangs or crashes on iOS", PlatformAffected.iOS)]
	public partial class Issue18891_2 : ContentPage
	{
		readonly Stopwatch _stopwatch;

		public Issue18891_2()
		{
			_stopwatch = Stopwatch.StartNew();
			InitializeComponent();
			LoadData();
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if(propertyName == HeightProperty.PropertyName || propertyName == WidthProperty.PropertyName)
			{
				_stopwatch.Stop();
				TestLabel.Text = $"{_stopwatch.ElapsedMilliseconds}";
			}
		}

		void LoadData()
		{
			List<Item> items = new List<Item>();

			for(int i = 0; i < 200000; i++)
			{
				if ((i % 2) == 0)
					items.Add(new Item1($"{i + 1}"));
				else
					items.Add(new Item2($"{i + 1}"));
			}

			TestCollectionView.ItemsSource = items;
		}
	}
	class ItemTemplateSelector : DataTemplateSelector
	{
		public DataTemplate Item1Template { get; set; }
		public DataTemplate Item2Template { get; set; }
		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			if (Item1Template == null || Item2Template == null)
				throw new ArgumentNullException();

			if (item is Item1)
				return Item1Template;

			if (item is Item2)
				return Item2Template;

			throw new ArgumentOutOfRangeException();
		}
	}

	public abstract class Item
	{
		protected Item(string name) => Name = name;

		public string Name { get; set; }
	}

	class Item1 : Item
	{
		public Item1(string name) : base(name) { }
	}

	class Item2 : Item
	{
		public Item2(string name) : base(name) { }
	}
}