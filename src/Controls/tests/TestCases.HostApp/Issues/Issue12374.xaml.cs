﻿using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 12374, "[Bug] iOS XF 5.0-pre1 crash with CollectionView when using EmptyView",
		PlatformAffected.iOS)]
	public partial class Issue12374 : TestContentPage
	{
		int _itemNumber = 0;
		bool _isRefreshing;

		public Issue12374()
		{
			InitializeComponent();

			AddItems();

			BindingContext = this;
		}

		protected override void Init()
		{

		}

		public bool IsRefreshing
		{
			get { return _isRefreshing; }
			set
			{
				_isRefreshing = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<Issue12374Model> Items { get; private set; } = new ObservableCollection<Issue12374Model>();

		public ICommand RefreshCommand => new Command(async () => await RefreshItemsAsync());

		async Task RefreshItemsAsync()
		{
			IsRefreshing = true;
			await Task.Delay(TimeSpan.FromSeconds(1));
			AddItems();
			IsRefreshing = false;
		}

		void AddItems()
		{
			var random = new Random();
			for (int i = 0; i < 2; i++)
			{
				Items.Add(new Issue12374Model()
				{
					Guid = Guid.NewGuid(),
					Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
					Name = $"Item {_itemNumber++}"
				});
			}
		}

		public void Item_SwipeInvoked(object sender, EventArgs e)
		{
			if (sender is SwipeItem swipeItem && swipeItem.BindingContext is Issue12374Model item)
			{
				Items.Remove(item);
			}
		}

		void AddItemsButton_Clicked(object sender, EventArgs e)
		{
			AddItems();
		}

		void RemoveItemsButton_Clicked(object sender, EventArgs e)
		{
			Items.Clear();
		}

		void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.CurrentSelection.Count == 0)
				return;

			if (sender is CollectionView collectionView)
			{
				collectionView.SelectedItems = null;
			}

			if (e.CurrentSelection[0] is Issue12374Model item)
			{
				Items.Remove(item);
			}
		}
	}


	public class Issue12374Model : IEquatable<Issue12374Model>
	{
		public Guid Guid { get; set; }
		public string Name { get; set; }
		public Color Color { get; set; }

		public bool Equals(Issue12374Model other)
		{
			return other.Guid.Equals(Guid);
		}
	}
}