using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Flyout Content",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	public class ShellFlyoutContent : TestShell
	{
		protected override void Init()
		{
			var page = new ContentPage();

			this.BindingContext = this;
			AddFlyoutItem(page, "Flyout Item Top");
			for (int i = 0; i < 50; i++)
			{
				AddFlyoutItem($"Flyout Item :{i}");
				Items[i].AutomationId = "Flyout Item";
			}

			Items.Add(new MenuItem() { Text = "Menu Item" });
			AddFlyoutItem("Flyout Item Bottom");

			var layout = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "Open the Flyout and Toggle the Content, Header and Footer. If it changes after each click test has passed",
						AutomationId = "PageLoaded"
					}
				}
			};

			page.Content = layout;

			layout.Children.Add(new Button()
			{
				Text = "Toggle Flyout Content Template",
				Command = new Command(() =>
				{
					if (FlyoutContentTemplate == null)
					{
						FlyoutContentTemplate = new DataTemplate(() =>
						{
							var collectionView = new CollectionView();

							collectionView.SetBinding(CollectionView.ItemsSourceProperty, "FlyoutItems");
							collectionView.IsGrouped = true;

							collectionView.ItemTemplate =
								new DataTemplate(() =>
								{
									var label = new Label();

									label.SetBinding(Label.TextProperty, "Title");

									var button = new Button()
									{
										Text = "Click to Reset",
										AutomationId = "ContentView",
										Command = new Command(() =>
										{
											FlyoutContentTemplate = null;
										})
									};

									return new StackLayout()
									{
										Children =
										{
											label,
											button
										}
									};
								});

							return collectionView;
						});
					}
					else if (FlyoutContentTemplate != null)
					{
						FlyoutContentTemplate = null;
					}
				}),
				AutomationId = "ToggleFlyoutContentTemplate"
			});

			layout.Children.Add(new Button()
			{
				Text = "Toggle Flyout Content",
				Command = new Command(() =>
				{
					if (FlyoutContent != null)
					{
						FlyoutContent = null;
					}
					else
					{
						var stackLayout = new StackLayout()
						{
							Background = SolidColorBrush.Green
						};

						FlyoutContent = new ScrollView()
						{
							Content = stackLayout
						};

						AddButton("Top Button");

						for (int i = 0; i < 50; i++)
						{
							AddButton("Content View");
						}

						AddButton("Bottom Button");

						void AddButton(string text)
						{
							stackLayout.Children.Add(new Button()
							{
								Text = text,
								AutomationId = "ContentView",
								Command = new Command(() =>
								{
									FlyoutContent = null;
								}),
								TextColor = Color.White
							});
						}
					}
				}),
				AutomationId = "ToggleContent"
			});

			layout.Children.Add(new Button()
			{
				Text = "Toggle Header/Footer View",
				Command = new Command(() =>
				{
					if (FlyoutHeader != null)
					{
						FlyoutHeader = null;
						FlyoutFooter = null;
					}
					else
					{
						FlyoutHeader = new StackLayout()
						{
							Children = {
								new Label() { Text = "Header" }
							},
							AutomationId = "Header View",
							Background = SolidColorBrush.Yellow
						};

						FlyoutFooter = new StackLayout()
						{
							Background = SolidColorBrush.Orange,
							Orientation = StackOrientation.Horizontal,
							Children = {
								new Label() { Text = "Footer" }
							},
							AutomationId = "Footer View"
						};
					}
				}),
				AutomationId = "ToggleHeaderFooter"
			});
		}


#if UITEST

		[Test]
		public void FlyoutContentTests()
		{
			RunningApp.WaitForElement("PageLoaded");
			TapInFlyout("Flyout Item");
			RunningApp.Tap("ToggleContent");
			TapInFlyout("ContentView");
			TapInFlyout("Flyout Item");
			RunningApp.Tap("ToggleFlyoutContentTemplate");
			TapInFlyout("ContentView");
			TapInFlyout("Flyout Item");
		}
#endif
	}
}
