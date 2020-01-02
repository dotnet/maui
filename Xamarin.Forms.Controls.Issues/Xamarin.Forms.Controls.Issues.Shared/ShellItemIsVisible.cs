using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using System.Threading;
using System.ComponentModel;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
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
						}
					}
				}
			};

			var pageItem1 = createPage("Item 1");
			var item1 = AddContentPage(pageItem1);
			var pageItem2 = createPage("Item 2");
			var item2 = AddContentPage(pageItem2);

			item1.Title = "Item1 Flyout";
			item1.Route = "Item1";
			item2.Title = "Item2 Flyout";
			item2.Route = "Item2";

			pageItem1.SetBinding(Page.IsVisibleProperty, "Item1");
			pageItem2.SetBinding(Page.IsVisibleProperty, "Item2");

			this.Items.Add(new MenuShellItem(new MenuItem()
			{
				Text = "Hide Flyout",
				Command = new Command(() =>
				{
					this.FlyoutIsPresented = false;
				})
			}));

		}

		[Preserve(AllMembers = true)]
		public class ShellViewModel : INotifyPropertyChanged
		{
			bool _item1 = true;
			bool _item2 = true;

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

			void OnPropertyChanged(string name) =>
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}

#if UITEST && (__SHELL__)

		[Test]
		public void HideFlyoutItem()
		{
			RunningApp.WaitForElement("ToggleItem1");
			ShowFlyout();
			RunningApp.WaitForElement("Item1 Flyout");
			RunningApp.Tap("Hide Flyout");
			RunningApp.Tap("AllVisible");
			RunningApp.Tap("ToggleItem1");
			ShowFlyout();
			RunningApp.WaitForNoElement("Item1 Flyout");
		}
#endif
	}
}
