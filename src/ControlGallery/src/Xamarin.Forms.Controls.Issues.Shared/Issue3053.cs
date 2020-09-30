using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3053, "Moving items around on an Observable Collection causes the last item to disappear", PlatformAffected.UWP)]
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(UITestCategories.ListView)]
#endif
	public class Issue3053 : TestContentPage
	{
		const string _instructions = "Click me once. Item 2 should remain on bottom";

		[Preserve(AllMembers = true)]
		public class Item
		{
			public string Name { get; set; }
		}

		protected override void Init()
		{
			var listView = new ListView
			{
				ItemsSource = new ObservableCollection<Item>(Enumerable.Range(0, 3).Select(x => new Item() { Name = $"Item {x}" })),
				ItemTemplate = new DataTemplate(() =>
				{
					Label nameLabel = new Label();
					nameLabel.SetBinding(Label.TextProperty, new Binding("Name"));
					var cell = new ViewCell
					{
						View = new Frame()
						{
							Content = nameLabel
						},
					};
					return cell;
				})
			};
			Content = new StackLayout
			{
				Children =
				{
					new Button()
					{
						Text = _instructions,
						Command = new Command(() =>
						{
							var collection = listView.ItemsSource as ObservableCollection<Item>;
							collection.Add(new Item(){ Name =  Guid.NewGuid().ToString() });
							collection.Move(3,1);
						})
					},
					listView
				}
			};
		}

#if UITEST
		[Test]
		public void MovingItemInObservableCollectionBreaksListView()
		{
			RunningApp.WaitForElement(_instructions);
			RunningApp.Tap(_instructions);
			RunningApp.WaitForElement("Item 2");
		}
#endif
	}
}