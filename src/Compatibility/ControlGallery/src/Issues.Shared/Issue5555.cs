using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5555, "Memory leak when SwitchCell or EntryCell", PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TableView)]
#endif
	public class Issue5555 : TestContentPage
	{
		[Preserve(AllMembers = true)]
		class LeakPage : ContentPage
		{
			public LeakPage()
			{
				Content = new StackLayout
				{
					Children = {
						new TableView
						{
							Root = new TableRoot
							{
								new TableSection
								{
									new SwitchCell { Text = "switch cell", On = true },
									new EntryCell { Text = "entry cell" }
								}
							}
						}
					}
				};
			}
		}

		protected override void Init()
		{
			var result = new Label
			{
				FontSize = 16,
				Text = "Click 'Push page' twice"
			};

			var list = new List<WeakReference>();

			var checkButton = new Button
			{
				Text = "Check Result",
				IsEnabled = false,
				Command = new Command(() =>
				{
					if (list.Count < 2)
					{
						result.Text = "Click 'Push page' again";
						return;
					}

					GarbageCollectionHelper.Collect();
					result.Text = list[list.Count - 2].IsAlive ? "Failed" : "Success";
				})
			};

			Content = new StackLayout
			{
				Children = {
					result,
					new Button
					{
						Text = "Push page",
						Command = new Command(async() => {
							var page = new LeakPage();
							var wref = new WeakReference(page);

							await Navigation.PushAsync(page);
							await page.Navigation.PopAsync();

							GarbageCollectionHelper.Collect();

							list.Add(wref);
							if (list.Count > 1)
							{
								checkButton.IsEnabled = true;
								result.Text = "You can check result";
							}
							else
							{
								result.Text = "Again";
							}
						})
					},
					checkButton
				}
			};
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
#if ANDROID
		[Compatibility.UITests.MovedToAppium]
#endif
		public void Issue5555Test()
		{
			RunningApp.Tap(q => q.Marked("Push page"));
			RunningApp.WaitForElement(q => q.Marked("Push page"));
			RunningApp.Tap(q => q.Marked("Push page"));
			RunningApp.WaitForElement(q => q.Marked("Push page"));

			RunningApp.WaitForElement(q => q.Marked("You can check result"));
			RunningApp.Tap(q => q.Marked("Check Result"));

			RunningApp.WaitForElement(q => q.Marked("Success"));
		}
#endif
	}
}