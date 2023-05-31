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

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Items IsVisible Test",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class ShellItemIsVisible : TestShell
	{

		protected override void Init()
		{
			var vm = new ShellViewModel();
			this.BindingContext = vm;

			SetupShell();
		}

		void SetupShell()
		{
			var vm = BindingContext as ShellViewModel;
			Func<string, ContentPage> createPage = (title) => new ContentPage()
			{
				Title = title,
				Content = new StackLayout()
				{
					Children =
					{
						new Button()
						{
							Text = "All Visible",
							AutomationId = "AllVisible",
							Command = new Command(() =>
							{
								vm.Item1 = vm.Item2 = true;
							})
						},
						new Button()
						{
							Text = "Toggle Item1",
							AutomationId = "ToggleItem1",
							Command = new Command(() =>
							{
								vm.Item1 = !vm.Item1;
							})
						},
						new Button()
						{
							Text = "Toggle Item2",
							AutomationId = "ToggleItem2",
							Command = new Command(() =>
							{
								vm.Item2 = !vm.Item2;
							})
						},
						new Button()
						{
							Text = "Goto Item1",
							AutomationId = "GotoItem1",
							Command = new Command(() =>
							{
								GoToAsync("//Item1");
							})
						},
						new Button()
						{
							Text = "Goto Item2",
							AutomationId = "GotoItem2",
							Command = new Command(() =>
							{
								GoToAsync("//Item2");
							})
						},
						new Button()
						{
							Text = "Toggle Flyout Item 3",
							AutomationId = "ToggleFlyoutItem3",
							Command = new Command(() =>
							{
								vm.Item3 = !vm.Item3;
							})
						},
						new Button()
						{
							Text = "Clear and Recreate",
							AutomationId = "ClearAndRecreate",
							Command = new Command(async () =>
							{
								this.Items[0].Items[0].Items.Clear();
								await Task.Delay(10);
								this.Items[0].Items.Clear();
								await Task.Delay(10);
								this.Items.Clear();
								SetupShell();
							})
						},
						new Button()
						{
							Text = "Clear and Recreate Shell Content",
							AutomationId = "ClearAndRecreateShellContent",
							Command = new Command(() =>
							{
								Items[0].Items[0].Items.Clear();
								AddTopTabs();
							})
						}
					}
				}
			};

			var pageItem1 = createPage("Item Title Page");
			var item1 = AddContentPage(pageItem1);
			var pageItem2 = createPage("Item 2");
			var item2 = AddContentPage(pageItem2);
			var pageItem3 = createPage("Item 3");
			var item3 = AddContentPage(pageItem3);

			item1.Title = "Item1 Flyout";
			item1.Route = "Item1";
			item2.Title = "Item2 Flyout";
			item2.Route = "Item2";
			item3.Title = "Item3 Flyout";
			item3.Route = "Item3";

			AddTopTabs();

			pageItem1.SetBinding(Page.IsVisibleProperty, "Item1");
			pageItem2.SetBinding(Page.IsVisibleProperty, "Item2");
			item3.SetBinding(Shell.FlyoutItemIsVisibleProperty, "Item3");

			this.Items.Add(new MenuShellItem(new MenuItem()
			{
				Text = "Hide Flyout",
				Command = new Command(() =>
				{
					this.FlyoutIsPresented = false;
				})
			}));

			void AddTopTabs()
			{
				AddTopTab($"Top Tab 1").Content = new StackLayout() { Children = { new Label { Text = "Welcome to Tab 1" } } };
				AddTopTab($"Top Tab 2").Content = new StackLayout() { Children = { new Label { Text = "Welcome to Tab 2" } } };
			}
		}

		[Preserve(AllMembers = true)]
		public class ShellViewModel : INotifyPropertyChanged
		{
			bool _item1 = true;
			bool _item2 = true;
			bool _item3 = true;

			public event PropertyChangedEventHandler PropertyChanged;

			public bool Item1
			{
				get => _item1;
				set
				{
					_item1 = value;
					OnPropertyChanged(nameof(Item1));
				}
			}

			public bool Item2
			{
				get => _item2;
				set
				{
					_item2 = value;
					OnPropertyChanged(nameof(Item2));
				}
			}

			public bool Item3
			{
				get => _item3;
				set
				{
					_item3 = value;
					OnPropertyChanged(nameof(Item3));
				}
			}

			void OnPropertyChanged(string name) =>
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

#if UITEST && (__SHELL__)

		[Test]
		public void FlyoutItemVisible()
		{
			RunningApp.Tap("ToggleFlyoutItem3");
			ShowFlyout();
			RunningApp.WaitForElement("Item2 Flyout");
			RunningApp.WaitForNoElement("Item3 Flyout");
		}

		[Test]
		public void HideActiveShellContent()
		{
			RunningApp.Tap("ToggleItem1");
			RunningApp.WaitForElement("Welcome to Tab 1");
			RunningApp.WaitForNoElement("ToggleItem1");
		}

		[Test]
		public void HideFlyoutItem()
		{
			RunningApp.WaitForElement("ToggleItem1");
			ShowFlyout();
			RunningApp.WaitForElement("Item2 Flyout");
			RunningApp.Tap("Item2 Flyout");
			RunningApp.Tap("AllVisible");
			RunningApp.Tap("ToggleItem2");
			ShowFlyout();
			RunningApp.WaitForElement("Item1 Flyout");
			RunningApp.WaitForNoElement("Item2 Flyout");
		}

		[Test]
		public void ClearAndRecreateShellElements()
		{
			RunningApp.WaitForElement("ClearAndRecreate");
			RunningApp.Tap("ClearAndRecreate");
			RunningApp.WaitForElement("ClearAndRecreate");
			RunningApp.Tap("ClearAndRecreate");
		}
		

		[Test]
		public void ClearAndRecreateFromSecondaryPage()
		{
			RunningApp.WaitForElement("ClearAndRecreate");
			ShowFlyout();
			RunningApp.Tap("Item2 Flyout");
			RunningApp.Tap("ToggleItem1");
			RunningApp.Tap("ClearAndRecreate");
			RunningApp.Tap("Top Tab 2");
			RunningApp.Tap("Top Tab 1");
		}

		[Test]
		public void ClearAndRecreateShellContent()
		{
			RunningApp.WaitForElement("ClearAndRecreateShellContent");
			RunningApp.Tap("ClearAndRecreateShellContent");
			RunningApp.WaitForElement("ClearAndRecreate");
			RunningApp.Tap("ClearAndRecreate");
		}
#endif
	}
}
