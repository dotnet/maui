using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github5000)]
	[Category(UITestCategories.ListView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1851, "ObservableCollection in ListView gets Index out of range when removing item", PlatformAffected.Android)]
	public class Issue1851 : TestContentPage
	{
		protected override void Init()
		{
			var grouping = new Grouping1851<string, string>("number", new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9" });
			var groupings = new ObservableCollection<Grouping1851<string, string>>
			{
				new Grouping1851<string, string>("letters", new List<string> {"a", "b", "c", "d", "e", "f", "g", "h", "i"}),
				new Grouping1851<string, string>("colours", new List<string> {"red", "green", "blue", "white", "orange", "purple", "grey", "mauve", "pink"}),
				grouping,
			};

			var listview = new ListView
			{
				HasUnevenRows = true,
				IsGroupingEnabled = true,
				ItemsSource = groupings,
				ItemTemplate = new DataTemplate(typeof(CellTemplate1851)),
				GroupDisplayBinding = new Binding("Key")
			};
			var groupbtn = new Button() { AutomationId = "btn", Text = "add/remove group" };
			bool group = true;
			groupbtn.Clicked += (sender, args) =>
			{
				listview.GroupShortNameBinding = new Binding("Key");
				if (group)
				{
					group = false;

					// ***** Crashes here
					groupings.Remove(grouping);
				}
				else
				{
					group = true;
					groupings.Add(grouping);
				}
			};

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						groupbtn,
						listview,
					}
				}
			};
		}

		[Preserve(AllMembers = true)]
		class CellTemplate1851 : ViewCell
		{
			protected override void OnBindingContextChanged()
			{
				base.OnBindingContextChanged();

				var text = BindingContext as string;
				if (text == null)
					return;

				View = new Label { Text = text };
			}
		}

		[Preserve(AllMembers = true)]
		class Grouping1851<TKey, TElement> : ObservableCollection<TElement>
		{
			public Grouping1851(TKey key, IEnumerable<TElement> items)
			{
				Key = key;
				foreach (var item in items)
					Items.Add(item);
			}

			public TKey Key { get; private set; }
		}

#if UITEST
		[Test]
		public void Issue1851Test() 
		{
			RunningApp.WaitForElement(q => q.Marked("btn"));
			RunningApp.Tap("btn");
			RunningApp.WaitForElement(q => q.Marked("btn"));
			RunningApp.Tap("btn");
			RunningApp.WaitForElement(q => q.Marked("btn"));
		}
#endif
	}
}