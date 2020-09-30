using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST && __ANDROID__
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
using System.Linq;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST && __ANDROID__
	[Category(UITestCategories.CollectionView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7993, "[Bug] CollectionView.Scrolled event offset isn't correctly reset when items change", PlatformAffected.Android)]
	public partial class Issue7993 : TestContentPage
	{
#if APP
		public Issue7993()
		{
			InitializeComponent();

			BindingContext = new ViewModel7993();
		}

		void CollectionView_OnScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			Label1.Text = "VerticalOffset: " + e.VerticalOffset;
		}

		void NewItemsSourceClicked(object sender, EventArgs e)
		{
			BindingContext = new ViewModel7993();
		}
#endif

		protected override void Init()
		{

		}

#if UITEST && __ANDROID__
		[Test]
		public void CollectionViewVerticalOffset()
		{
			var colView = RunningApp.WaitForElement("CollectionView7993")[0];

			RunningApp.WaitForElement(x => x.Marked("VerticalOffset: 0"));

			AppResult[] lastCellResults = null;

			RunningApp.QueryUntilPresent(() =>
			{
				RunningApp.DragCoordinates(colView.Rect.CenterX, colView.Rect.Y + colView.Rect.Height - 50, colView.Rect.CenterX, colView.Rect.Y + 5);

				lastCellResults = RunningApp.Query("19");

				return lastCellResults;
			}, 20, 1);

			Assert.IsTrue(lastCellResults?.Any() ?? false);

			RunningApp.Tap(x => x.Marked("NewItemsSource"));
			RunningApp.WaitForElement(x => x.Marked("VerticalOffset: 0"));
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class ViewModel7993
	{
		public ObservableCollection<Model7993> Items { get; set; }

		public ViewModel7993()
		{
			var collection = new ObservableCollection<Model7993>();

			for (var i = 0; i < 20; i++)
			{
				collection.Add(new Model7993()
				{
					Text = i.ToString()
				});
			}

			Items = collection;
		}
	}

	[Preserve(AllMembers = true)]
	public class Model7993
	{
		public string Text { get; set; }

		public Model7993()
		{

		}
	}
}