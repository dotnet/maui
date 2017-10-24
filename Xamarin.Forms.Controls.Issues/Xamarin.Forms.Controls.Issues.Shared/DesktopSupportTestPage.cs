using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 9090, "Desktop Support", PlatformAffected.All)]
	public class DesktopSupportTestPage : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(Menu());
		}

		ContentPage Menu()
		{
			var layout = new StackLayout();

			layout.Children.Add(new Label { Text = "Select a test below" });

			foreach (var test in GenerateTests)
			{
				layout.Children.Add(MenuButton(test));
			}

			return new ContentPage { Content = layout };
		}

		Button MenuButton(TestDesktop test)
		{
			var button = new Button { Text = test.TestName, AutomationId = test.AutomationId };

			button.Clicked += (sender, args) => test.Command();

			return button;
		}

		Page RightClickSupportPage()
		{
			var layout = new StackLayout();
			var label = new Label { Text = "Click the box with left and right click" };
			var box = new BoxView { BackgroundColor = Color.Red, WidthRequest = 100, HeightRequest = 100 };

			var btn = new Button { Text = "Clear", Command = new Command(() => label.Text = "") };
			var btnNormal = new Button
			{
				Text = "Normal Click",
				Command = new Command(() =>
				{
					label.Text = "";
					box.GestureRecognizers.Clear();
					box.GestureRecognizers.Add(new ClickGestureRecognizer
					{
						Command = new Command((obj) =>
						{
							label.Text = "Clicked";
						})
					});
				})
			};
			var btnRight = new Button
			{
				Text = "Right Click",
				Command = new Command(() =>
				{
					label.Text = "";
					box.GestureRecognizers.Clear();
					box.GestureRecognizers.Add(new ClickGestureRecognizer
					{
						Buttons = ButtonsMask.Secondary,
						Command = new Command((obj) =>
						{
							label.Text = "Right Clicked";
						})
					});
				})
			};
			var btnBoth = new Button
			{
				Text = "Both Clicks",
				Command = new Command(() =>
				{
					box.GestureRecognizers.Clear();
					label.Text = "";
					box.GestureRecognizers.Add(new ClickGestureRecognizer
					{
						Buttons = ButtonsMask.Primary | ButtonsMask.Secondary,
						Command = new Command((obj) =>
						{
							label.Text = "Left and Right Clicked";
						})
					});
				})
			};

			layout.Children.Add(label);
			layout.Children.Add(box);
			layout.Children.Add(btn);
			layout.Children.Add(btnNormal);
			layout.Children.Add(btnRight);
			layout.Children.Add(btnBoth);
			return new ContentPage { Content = layout };
		}

		Page ContextMenuSupportPage()
		{
			var layout = new StackLayout();

			var label = new Label { Text = "Click here to show context menu" };
			var menu = new Menu();
			AddMenu(1, true, 2, false, menu);
			SetMenu(label, menu);
			layout.Children.Add(label);

			var box = new BoxView { Color = Color.Red, WidthRequest = 100, HeightRequest = 100 };
			var menuBox = new Menu();
			AddMenu(4, true, 2, false, menuBox, true);
			SetMenu(box, menuBox);
			layout.Children.Add(box);

			return new ContentPage { Title = "Context Menu", Content = layout };
		}

		Page MenusSupportPage()
		{
			var layout = new StackLayout();
			var label = new Label { Text = "Adding menus" };
			var btn = new Button { Text = "Clear", Command = new Command(() => GetMenu(Application.Current).Clear()) };
			var btnAdd = new Button { Text = "Add Menu Hello", Command = new Command(() => AddMenu(1)) };
			var btnAdd3 = new Button { Text = "Add 3 Menu Hello", Command = new Command(() => AddMenu(3)) };
			var btnAdd3Add2 = new Button { Text = "Add Menu Hello with 2 Subitems", Command = new Command(() => AddMenu(3, true, 2)) };
			var btnAddImage = new Button { Text = "Add Menu Hello With Icon", Command = new Command(() => AddMenu(1, true, 1, withImage: true)) };
			var btnAddSubmenus = new Button { Text = "Add Menu Hello With submenu", Command = new Command(() => AddMenu(1, true, 1, false, null, true)) };
			var btnAddChangeText = new Button
			{
				Text = "Add Menu Change Text and disable after 3 seconds",
				Command = new Command(async () =>
						{
							AddMenu(1, true);
							await Task.Delay(3000);
							var mainMenu = GetMenu(Application.Current);
							mainMenu[0].Items[0].Text = "hello changed";
							mainMenu[0].Items[0].IsEnabled = false;
						})
			};
			var btnAddSubmenusWithShortcut = new Button { Text = "Add Menu Hello With submenu And shortcut", Command = new Command(() => AddMenu(1, true, 7, false, null, true, true)) };

			layout.Children.Add(btn);
			layout.Children.Add(btnAdd);
			layout.Children.Add(btnAdd3);
			layout.Children.Add(btnAdd3Add2);
			layout.Children.Add(btnAddImage);
			layout.Children.Add(btnAddSubmenus);
			layout.Children.Add(btnAddChangeText);
			layout.Children.Add(btnAddSubmenusWithShortcut);
			layout.Children.Add(label);
			return new ContentPage { Title = "Menus", Content = layout };
		}

		void AddMenu(int count, bool addMenuItems = false, int countMenuItems = 1, bool withImage = false, Menu menuHolder = null, bool addSubMenu = false, bool addShortcut = false)
		{
			for (int i = 0; i < count; i++)
			{
				var menu = new Forms.Menu { Text = $"hello {i}" };
				if (addMenuItems)
				{
					for (int j = 0; j < countMenuItems; j++)
					{
						var item = new MenuItem { Text = $"hello menu item {i}.{j}" };
						if (withImage)
						{
							item.Icon = Icon = "bank.png";
						}
						if (addShortcut)
						{
							var shourtCutKeyBinding = $"{j}";
							if (j == 1)
								shourtCutKeyBinding = $"shift+{j}";
							if (j == 2)
								shourtCutKeyBinding = $"ctrl+{j}";
							if (j == 3)
								shourtCutKeyBinding = $"alt+{j}";
							if (j == 4)
								shourtCutKeyBinding = $"cmd+{j}";
							if (j == 5)
								shourtCutKeyBinding = $"fn+{j}";
							if (j == 6)
								shourtCutKeyBinding = $"cmd+alt+{j}";

							item.Text = shourtCutKeyBinding;
							MenuItem.SetAccelerator(item, Accelerator.FromString(shourtCutKeyBinding));
						}
						menu.Items.Add(item);
					}
				}
				if (addSubMenu)
				{
					var submenu = new Forms.Menu { Text = $"submenu {i}" };
					var item = new MenuItem { Text = $"submenu item {i}" };
					submenu.Items.Add(item);
					menu.Add(submenu);
				}
				if (menuHolder == null)
				{
					var mainMenu = new Menu();
					SetMenu(Application.Current, mainMenu);
					menuHolder = GetMenu(Application.Current);
				}
					
				menuHolder.Add(menu);
			}
		}

		IEnumerable<TestDesktop> GenerateTests
		{
			get
			{
				var testList = new List<TestDesktop>();
				testList.Add(new TestDesktop("Quit") { Command = () => { Application.Current.Quit(); } });
				testList.Add(new TestDesktop("RightClick") { Command = async () => { await Navigation.PushAsync(RightClickSupportPage()); } });
				testList.Add(new TestDesktop("Menus") { Command = async () => { await Navigation.PushAsync(MenusSupportPage()); } });
				testList.Add(new TestDesktop("Context Menu") { Command = async () => { await Navigation.PushAsync(ContextMenuSupportPage()); } });

				return testList;
			}
		}

		public class TestDesktop
		{
			public TestDesktop(string name)
			{
				TestName = name;
			}

			public string TestName
			{
				get;
				set;
			}

			public string AutomationId => $"desktoptest_{TestName}";

			public Action Command
			{
				get;
				set;
			}

		}

	}
}
