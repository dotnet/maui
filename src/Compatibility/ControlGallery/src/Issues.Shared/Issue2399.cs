using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Effects;
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
	[Issue(IssueTracker.Github, 2399, "Label Renderer Dispose never called")]

#if __WINDOWS__
	// this test works fine when ran manually but when executed through the test runner
	// it fails. Not sure the difference
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.ManualReview)]
#endif
	public class Issue2399 : TestNavigationPage
	{
		static AttachedStateEffectList AttachedStateEffects = new AttachedStateEffectList();
		Label AllEffectsHaveDetached;
		protected override void Init()
		{
			AllEffectsHaveDetached = new Label()
			{
				AutomationId = "AllEventsHaveDetached",
				Text = "If this text doesn't change then all effects haven't detached"
			};

			var newPage = new ContentPage
			{
				Content =
					new StackLayout()
					{
						Children =
						{
							new Button
							{
								Text = "Show New ListView",
								Command = new Command(async o => await Navigation.PushAsync(new ListPage())),
							},

							new Button
							{
								Text = "Garbage Collection Things",
								Command = new Command(() =>
								{
									GarbageCollectionHelper.Collect();
									AttachedStateEffects.Clear();
								}),
							},
							AllEffectsHaveDetached
						}
					},
				AutomationId = "ContentPage"

			};

			AttachedStateEffects.AllEventsAttached += OnAllEventsAttached;
			AttachedStateEffects.AllEventsDetached += OnAllEventsDetached;

			var listPage = new ListPage();
			Navigation.PushAsync(listPage);
			Navigation.InsertPageBefore(newPage, listPage);
		}

		async void OnAllEventsAttached(object sender, EventArgs args)
		{
			AttachedStateEffects.AllEventsAttached -= OnAllEventsAttached;
			// needed otherwise UWP crashes
			await Task.Delay(100);
			await Navigation.PopAsync();
			GarbageCollectionHelper.Collect();
		}

		void OnAllEventsDetached(object sender, EventArgs args)
		{
			AttachedStateEffects.Clear();
			AttachedStateEffects.AllEventsDetached -= OnAllEventsDetached;
			GarbageCollectionHelper.Collect();
			AllEffectsHaveDetached.Text = "Success";
		}

		[Preserve(AllMembers = true)]
		public class ListPage : ContentPage
		{
			protected override void OnAppearing()
			{
				base.OnAppearing();
			}

			public ListPage()
			{
				AutomationId = "ListPage";
				Content = new ListView()
				{
					ItemTemplate = new DataTemplate(() => new Cell()),
					ItemsSource = Enumerable.Range(0, 1),
				};
			}
		}

		[Preserve(AllMembers = true)]
		class Cell : ViewCell
		{
			string guid = Guid.NewGuid().ToString();

			public Cell()
			{
				AutomationId = "ViewCell";
				Console.WriteLine("Constructor " + guid);
				var internalLabel = new Label { Text = guid, AutomationId = "Label" };

				AttachedStateEffects.Add(internalLabel);

				View = new StackLayout
				{
					AutomationId = "StackLayout",
					Children =
					{
						new ContentView
						{
							AutomationId = "ContentView",
							Content = internalLabel,
						}
					}
				};

				AttachedStateEffects.Add(View);
				AttachedStateEffects.Add((View as StackLayout).Children[0]);
			}
		}

#if UITEST && __IOS__
		[Test]
		public void WaitForAllEffectsToDetach()
		{
			RunningApp.WaitForElement(q => q.Text("Success"));
		}
#endif

	}
}
