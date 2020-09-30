using System;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10482, "CollectionView ItemsLayout Does Not Update in UWP", PlatformAffected.UWP)]
	public partial class Issue10482 : TestContentPage
	{
		int _count = 0;

#if APP
		public Issue10482()
		{
			InitializeComponent();

			BindingContext = new ViewModel10482();
		}
#endif

		protected override void Init()
		{

		}

		void OnButtonLayoutClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			var stackLayout = button.Parent as StackLayout;
			var grid = stackLayout.Parent as Grid;
			var collectionView = grid.Children[2] as CollectionView;

			if (_count % 2 == 0)
			{
				collectionView.ItemsLayout = new GridItemsLayout(ItemsLayoutOrientation.Vertical)
				{
					Span = 2,
					HorizontalItemSpacing = 5,
					VerticalItemSpacing = 5
				};

				button.Text = "Switch to linear layout";
			}
			else
			{
				collectionView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
				{
					ItemSpacing = 5
				};

				button.Text = "Switch to grid layout";
			}

			++_count;
		}

		void OnScrollButtonClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			var stackLayout = button.Parent as StackLayout;
			var grid = stackLayout.Parent as Grid;
			var collectionView = grid.Children[2] as CollectionView;

			collectionView.ScrollTo(10);
		}
	}

	[Preserve(AllMembers = true)]
	public class ViewModel10482
	{
		public ObservableCollection<Model10482> Items { get; set; }

		public ViewModel10482()
		{
			var collection = new ObservableCollection<Model10482>();
			var pageSize = 50;

			for (var i = 0; i < pageSize; i++)
			{
				collection.Add(new Model10482
				{
					Text = "Image" + i,
					Source = i % 2 == 0 ?
					"https://upload.wikimedia.org/wikipedia/commons/thumb/5/5d/Kamchatka_Brown_Bear_near_Dvuhyurtochnoe_on_2015-07-23.jpg/320px-Kamchatka_Brown_Bear_near_Dvuhyurtochnoe_on_2015-07-23.jpg" :
					"https://upload.wikimedia.org/wikipedia/commons/thumb/e/e4/Elephant_%40_kabini.jpg/180px-Elephant_%40_kabini.jpg",
					AutomationId = "Image" + i
				});
			}

			Items = collection;
		}
	}

	[Preserve(AllMembers = true)]
	public class Model10482
	{
		public string Text { get; set; }

		public string Source { get; set; }

		public string AutomationId { get; set; }
	}
}