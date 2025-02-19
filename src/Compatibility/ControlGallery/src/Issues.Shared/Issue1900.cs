using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[Category(UITestCategories.ListView)] 
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1900, "Xamarin ios ListView ObservableCollection<myClass>. Collection.Add() throwing 'Index # is greater than the number of rows #' exception", PlatformAffected.iOS)]
	public class Issue1900 : TestContentPage
	{
		public ObservableCollection<string> Items { get; set; } = new ObservableCollection<string>(Enumerable.Range(0, 25).Select(i => $"Initial {i}"));

		public void AddItemsToList(IEnumerable<string> items)
		{
			foreach (var item in items)
			{
				Items.Add(item);
			}
		}

		protected override void Init()
		{
			var listView = new ListView(ListViewCachingStrategy.RecycleElement) { AutomationId = "ListView", ItemsSource = Items };
			listView.ItemAppearing += ItemList_ItemAppearing;
			Content = new StackLayout { Children = { new Label { Text = "If this test crashes when it loads or when you scroll the list, then this test has failed. Obviously." }, listView } };
		}

		void ItemList_ItemAppearing(object sender, ItemVisibilityEventArgs e)
		{
			if (e.Item.ToString() == Items.Last())
			{
				AddItemsToList(Enumerable.Range(0, 10).Select(i => $"Item {i}"));
			}
		}


#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue1900Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("ListView"));
		}
#endif
	}
}
