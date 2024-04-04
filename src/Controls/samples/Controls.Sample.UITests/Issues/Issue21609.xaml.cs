using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 21609, "After rotating the emulator from horizontal to vertical orientation, the card of page My Recipes is truncated", PlatformAffected.Android)]
	public partial class Issue21609 : ContentPage
	{
		public Issue21609()
		{
			InitializeComponent();

			BindingContext = new Issue21609ViewModel();
		}
	}

	public class Issue21609Data
	{
		public Color Color { get; set; }
		public string Name { get; set; }
	}

	public class Issue21609ViewModel : BindableObject
	{
		Color[] colors = new Color[5] 
		{
			Colors.Red,
			Colors.Blue,
			Colors.Orange,
			Colors.Green,
			Colors.Pink,
		};

		ObservableCollection<Issue21609Data> _items;

		public Issue21609ViewModel()
		{
			SetSource();
		}

		readonly Random _random = new Random();

		void SetSource()
		{
			var source = new List<Issue21609Data>();

			for (int n = 0; n < 5; n++)
			{
				source.Add(GetItem(n));
			}

			Items = new ObservableCollection<Issue21609Data>(source);
		}

		public Issue21609Data GetItem(int currentCount)
		{
			return new Issue21609Data
			{
				Color = colors[currentCount],
				Name = $"{currentCount + 1}"
			};
		}

		public ObservableCollection<Issue21609Data> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged(nameof(Items));
			}
		}
	}
}