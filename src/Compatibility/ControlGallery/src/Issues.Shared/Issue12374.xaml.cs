using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12374, "[Bug] [iOS] CollectionView EmptyView causes the application to crash",
		PlatformAffected.iOS)]
	public partial class Issue12374 : TestContentPage
	{
		int _itemNumber = 0;
		bool _isRefreshing;

		public Issue12374()
		{
#if APP
			InitializeComponent();

			AddItems();

			BindingContext = this;
#endif
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

#if APP
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
#endif

#if UITEST
        [Test]
        public void Issue12374Test()
        {
            RunningApp.WaitForElement("TestReady");
            RunningApp.Tap("RemoveItems");
  		    RunningApp.Tap("AddItems");
            RunningApp.Tap("RemoveItems");
            RunningApp.Screenshot("CollectionViewWithEmptyView");
        }
#endif
	}

	[Preserve(AllMembers = true)]
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