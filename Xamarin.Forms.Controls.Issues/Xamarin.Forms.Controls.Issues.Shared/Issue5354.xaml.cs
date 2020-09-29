using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
using System.Linq;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5354, "[CollectionView] Updating the ItemsLayout type should refresh the layout", PlatformAffected.All)]
	public partial class Issue5354 : TestContentPage
	{
		int count = 0;

#if APP
		public Issue5354()
		{
			InitializeComponent();

			BindingContext = new ViewModel5354();
		}
#endif

		protected override void Init()
		{

		}

		void ButtonClicked(object sender, EventArgs e)
		{
			var button = sender as Button;
			var stackLayout = button.Parent as StackLayout;
			var grid = stackLayout.Parent as Grid;
			var collectionView = grid.Children[1] as CollectionView;

			if (count % 2 == 0)
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

			++count;
		}

#if UITEST
		[Test]
		public void CollectionViewItemsLayoutUpdate()
		{
			RunningApp.WaitForElement("CollectionView5354");
			RunningApp.WaitForElement("Button5354");
			var colView = RunningApp.Query("CollectionView5354").Single();
		
			for(var i=0; i<3; i++)
			{
				RunningApp.WaitForNoElement("NoElement", timeout: TimeSpan.FromSeconds(3));
				
				AppResult[] lastCellResults = null;

				RunningApp.QueryUntilPresent(() =>
				{
					 RunningApp.DragCoordinates(colView.Rect.CenterX, colView.Rect.Y + colView.Rect.Height - 50, colView.Rect.CenterX, colView.Rect.Y + 5);

					 lastCellResults = RunningApp.Query("Image49");

					 return lastCellResults;
				}, 100, 1);

				RunningApp.Tap("Button5354");
			}
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class ViewModel5354
	{
		public ObservableCollection<Model5354> Items { get; set; }

		public ViewModel5354()
		{
			var collection = new ObservableCollection<Model5354>();
			var pageSize = 50;

			for (var i = 0; i < pageSize; i++)
			{
				collection.Add(new Model5354
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
	public class Model5354
	{
		public string Text { get; set; }

		public string Source { get; set; }

		public string AutomationId { get; set; }

		public Model5354()
		{

		}
	}
}