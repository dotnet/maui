using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8262, "[Android] ImageRenderer still being accessed after control destroyed",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.LifeCycle)]
	[NUnit.Framework.Category(UITestCategories.CollectionView)]
#endif
	public class Issue8262 : TestContentPage
	{
		public View WithBounds(View v, double x, double y, double w, double h)
		{
			AbsoluteLayout.SetLayoutBounds(v, new Rectangle(x, y, w, h));
			return v;
		}

		protected override void Init()
		{
			IEnumerable<View> Select((string groupHeader, IEnumerable<int> items) t)
			{
				yield return new AbsoluteLayout
				{
					Children = {
						WithBounds(new Label {
							Text = t.groupHeader, HorizontalTextAlignment = TextAlignment.Center,
							TextColor = Color.FromUint(0xff5a5a5a), FontSize = 10
						}, 0, 21.1, 310, AbsoluteLayout.AutoSize) },
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.Start,
					HeightRequest = 46
				};
				foreach (var item in t.items)
				{
					yield return new AbsoluteLayout
					{
						Children = {

							WithBounds(new Image { Source = ImageSource.FromResource("Microsoft.Maui.Controls.ControlGallery.GalleryPages.crimson.jpg", System.Reflection.Assembly.GetCallingAssembly()) }, 23.6, 14.5, 14.9, 20.7),

							WithBounds(new Label { Text = item.ToString(), TextColor = Color.FromUint(0xff5a5a5a), FontSize = 10 }, 58, 18.2, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize)
						},
						HorizontalOptions = LayoutOptions.FillAndExpand,
						VerticalOptions = LayoutOptions.Start,
						HeightRequest = 49.7
					};
				}
			}

			Content = new AbsoluteLayout
			{
				Children = {
					new CollectionView {
						ItemsSource =
							new (string, Func<int, bool>)[] {
								("odd", i => i % 2 == 1),
								("even", i => i % 2 == 0),
								("triple", i => i % 3 == 0),
								("fives", i => i % 5 == 0) }
							.Select(t => (t.Item1, Enumerable.Range(1, 100).Where(t.Item2)))
							.SelectMany(Select),
						ItemTemplate = new DataTemplate(() => {
							var template = new ContentView();
							template.SetBinding(ContentView.ContentProperty, ".");
							return template;
						}),
						ItemsLayout = LinearItemsLayout.Vertical,
						ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem,
						AutomationId = "ScrollMe"
					}
				}
			};
		}

#if UITEST
		[Test]
		public void ScrollingQuicklyOnCollectionViewDoesntCrashOnDestroyedImage()
		{
			RunningApp.WaitForElement("ScrollMe");
			RunningApp.ScrollDown("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			RunningApp.ScrollUp("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			RunningApp.ScrollDown("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			RunningApp.ScrollUp("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			RunningApp.ScrollDown("ScrollMe", ScrollStrategy.Gesture, swipeSpeed: 20000);
			RunningApp.WaitForElement("ScrollMe");
		}
#endif
	}
}
