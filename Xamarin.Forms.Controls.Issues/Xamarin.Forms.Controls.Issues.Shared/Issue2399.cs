using System;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;
using Xamarin.Forms.Controls.Effects;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2399, "Label Renderer Dispose never called")]
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
									GC.Collect();
									GC.WaitForPendingFinalizers();
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
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		void OnAllEventsDetached(object sender, EventArgs args)
		{
			AttachedStateEffects.Clear();
			AttachedStateEffects.AllEventsDetached -= OnAllEventsDetached;
			GC.Collect();
			GC.WaitForPendingFinalizers();
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

#if UITEST && !__ANDROID__
		[Test]
		public void WaitForAllEffectsToDetach()
		{
			RunningApp.WaitForElement(q => q.Text("Success"));
		}
#endif

	}
}
