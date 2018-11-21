using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using System.Collections.Specialized;

#if UITEST
using Xamarin.Forms.Core.UITests;
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1675, "Bottom Tabbed Page Basic Test", PlatformAffected.All)]
	public class BottomTabbedPageTests : TestTabbedPage
	{
		Label pageCountLabel = null;
		public BottomTabbedPageTests() : base()
		{
		}

		protected override void Init()
		{
			On<Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);

			pageCountLabel = new Label() { AutomationId = "PageCount" };
			var popButton1 = new Button() { Text = "Pop", BackgroundColor = Color.Blue };
			popButton1.Clicked += (s, a) => Navigation.PopModalAsync();

			var popButton2 = new Button() { Text = "Pop 2", BackgroundColor = Color.Blue };
			popButton2.Clicked += (s, a) => Navigation.PopModalAsync();

			var longerTest = new Button() { Text = "Manual Color Tests", BackgroundColor = Color.Blue };

			Children.Add(new ContentPage() { Title = "Page 1", Content = popButton1, Icon = "coffee.png" });
			Children.Add(new ContentPage() { Title = "Page 2", Content = popButton2, Icon = "bank.png" });
			Button btnChangeBarText = null;
			Button btnChangeBarItemColorText = null;
			Button btnChangeBarSelectedItemColorText = null;
			Button btnAddPage = null;
			Button btnRemovePage = null;
			Label lblSuccess = new Label() { AutomationId = "Outcome" };

			btnChangeBarText = new Button()
			{
				Text = "Change Bar Text",
				Command = new Command(() =>
				{
					if (BarTextColor == Color.Default)
					{
						BarTextColor = Color.HotPink;
						btnChangeBarText.Text = $"Bar Text: HotPink";
					}
					else
					{
						BarTextColor = Color.Default;
						btnChangeBarText.Text = $"Bar Text: Default";
					}
				})
			};


			btnChangeBarItemColorText = new Button()
			{
				Text = "Change Item Color",
				Command = new Command(() =>
				{
					if (On<Android>().GetBarItemColor() == Color.Default)
					{
						On<Android>().SetBarItemColor(new Color(0, 255, 0, 128));
						btnChangeBarItemColorText.Text = $"Item Color: Less Green";
					}
					else
					{
						On<Android>().SetBarItemColor(Color.Default);
						btnChangeBarItemColorText.Text = $"Item Color: Default";
					}
				})
			};


			btnChangeBarSelectedItemColorText = new Button()
			{
				Text = "Change Selected Item Color",
				Command = new Command(() =>
				{
					if (On<Android>().GetBarSelectedItemColor() == Color.Default)
					{
						On<Android>().SetBarSelectedItemColor(Color.Green);
						btnChangeBarSelectedItemColorText.Text = $"Selected Item Color: Green";
					}
					else
					{
						On<Android>().SetBarSelectedItemColor(Color.Default);
						btnChangeBarSelectedItemColorText.Text = $"Selected Item Color: Default";
					}
				})
			};

			btnAddPage = new Button()
			{
				Text = $"Add Page (more than {On<Android>().GetMaxItemCount()} will crash)",
				Command = new Command(() =>
				{
					Children.Add(new ContentPage()
					{
						Content = new Label() { Text = (Children.Count + 1).ToString() },
						Title = (Children.Count + 1).ToString(),
						Icon = "calculator.png"
					});
					btnRemovePage.IsEnabled = true;
				}),
				AutomationId = "AddPage"
			};

			btnRemovePage = new Button()
			{
				Text = "Remove Page",
				Command = new Command(() =>
				{
					Children.Remove(Children.Last());
					if (Children.Count == 3)
					{
						btnRemovePage.IsEnabled = false;
					}
				}),
				IsEnabled = false,
				AutomationId = "RemovePage"
			};

			var layout = new StackLayout()
			{
				Children =
					{
						btnChangeBarText,
						new Button()
						{
							Text = "Change Bar Background Color",
							Command = new Command(()=>
							{
								if(BarBackgroundColor == Color.Default)
									BarBackgroundColor = Color.Fuchsia;
								else
									BarBackgroundColor = Color.Default;
							})
						},
						btnAddPage,
						btnRemovePage,
						new Button()
						{
							Text = "Page Add/Remove Permutations",
							Command = new Command(() =>
							{
								while(Children.Count > 3)
								{
									Children.Remove(Children.Last());
								}

								Children.Insert(1, new ContentPage(){ Icon = "bank.png" });
								Children.Insert(1, new ContentPage(){ Icon = "bank.png" });
								int i = 0;
								Device.StartTimer(TimeSpan.FromSeconds(3), () =>
								{
									if(i == 0)
									{
										// Ensure inserting didn't change current page
										if (CurrentPage != Children[4])
										{
											throw new Exception("Inserting page caused Current Page to Change");
										}
										Children.RemoveAt(1);
									}
									else if(i == 1)
									{
										// Ensure removing didn't change current page
										if (CurrentPage != Children[3])
										{
											throw new Exception("Removing page caused Current Page to Change");
										}
										Children.Insert(1, new ContentPage(){ Icon = "bank.png" });
										CurrentPage = Children[1];
									}
									else if(i == 2)
									{
										if (CurrentPage != Children[1])
										{
											throw new Exception("Current Page not correctly set to new page inserted");
										}

										Children.RemoveAt(1);
										Children.RemoveAt(1);
									}
									else if(i == 3)
									{
										if(CurrentPage != Children[0])
										{
											throw new Exception("Current Page not reset to Page one after Current Page was Removed");
										}
										CurrentPage = Children.Last();
										lblSuccess.Text = "Success";
									}
									else if(i >= 4)
									{
										return false;
									}

									i++;
									return true;
								});
							})
						},
						pageCountLabel,
						lblSuccess
					},
			};

			if (Device.RuntimePlatform == Device.Android)
			{
				layout.Children.Insert(1, btnChangeBarItemColorText);
				layout.Children.Insert(2, btnChangeBarSelectedItemColorText);
			}

			Children.Add(new ContentPage()
			{
				Title = "Test",
				Content = layout,
				Icon = "calculator.png"
			});
		}

		protected override void OnCurrentPageChanged()
		{
			base.OnCurrentPageChanged();
			pageCountLabel.Text = $"{Children.Count} Pages";
		}

		protected override void OnPagesChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnPagesChanged(e);
			pageCountLabel.Text = $"{Children.Count} Pages";
		}

#if UITEST && __ANDROID__
		[Test]
		public async Task AddAndRemovePages()
		{
			RunningApp.WaitForElement(q => q.Marked("Test"));
			RunningApp.Tap(q => q.Marked("Test"));
			RunningApp.WaitForElement(q => q.Marked("3 Pages"));
			RunningApp.Tap(q => q.Button("AddPage"));
			RunningApp.WaitForElement(q => q.Marked("4 Pages"));
			RunningApp.Tap(q => q.Button("AddPage"));
			RunningApp.WaitForElement(q => q.Marked("5 Pages"));
			RunningApp.Tap(q => q.Button("RemovePage"));
			RunningApp.WaitForElement(q => q.Marked("4 Pages"));
			RunningApp.Tap(q => q.Button("RemovePage"));
			RunningApp.WaitForElement(q => q.Marked("3 Pages"));
			RunningApp.Tap(q => q.Button("Page Add/Remove Permutations"));
			// This test cakes about 12 seconds so just adding a delay so WaitForElement doesn't time out
			await Task.Delay(10000);
			RunningApp.WaitForElement(q => q.Marked("Success"));
		}

		[Test]
		public void BottomTabbedPageWithModalIssueTestsAllElementsPresent()
		{
			RunningApp.WaitForElement(q => q.Marked("Page 1"));
			RunningApp.WaitForElement(q => q.Marked("Page 2"));
			RunningApp.WaitForElement(q => q.Button("Pop"));

			RunningApp.Screenshot("All elements present");
		}

		[Test]
		public void BottomTabbedPageWithModalIssueTestsPopFromFirstTab()
		{
			RunningApp.Tap(q => q.Button("Pop"));
			RunningApp.WaitForElement(q => q.Marked("Bug Repro's"));

			RunningApp.Screenshot("Popped from first tab");
		}

		[Test]
		public void BottomTabbedPageWithModalIssueTestsPopFromSecondTab()
		{
			RunningApp.Tap(q => q.Marked("Page 2"));
			RunningApp.WaitForElement(q => q.Button("Pop 2"));
			RunningApp.Screenshot("On second tab");

			RunningApp.Tap(q => q.Button("Pop 2"));
			RunningApp.WaitForElement(q => q.Marked("Bug Repro's"));
			RunningApp.Screenshot("Popped from second tab");
		}
#endif
	}
}
