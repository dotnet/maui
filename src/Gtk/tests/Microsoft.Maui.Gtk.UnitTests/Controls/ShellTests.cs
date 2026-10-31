#pragma warning disable CS0618 // SearchHandler.DisplayMemberName is obsolete but still honoured.
using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Gtk.Handlers;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Gtk.Platform;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>Shell: flyout of items, tabs of sections, pushed pages and behaviour.</summary>
	public class ShellTests
	{
		static ContentPage Page(string text) => new() { Title = text, Content = new Label { Text = text } };

		static FlyoutItem Item(string title, ContentPage page) => new()
		{
			Title = title,
			Items = { new ShellContent { Title = title, Content = page } },
		};

		static string FlyoutTitle(global::Gtk.ListBoxRow row) => GtkTestHost.Find<global::Gtk.Label>(row).Single().Text;

		[GtkFact]
		public void ShellUsesTheGtkHandlerAndListsTheFlyoutItems() => GtkTestHost.Run(() =>
		{
			var shell = new Shell { Items = { Item("Home", Page("home")), Item("Settings", Page("settings")) } };
			using var host = new ControlsWindowHost(shell, 800, 500);

			var handler = Assert.IsType<GtkShellHandler>(shell.Handler);
			var rows = Enumerable.Range(0, 2).Select(i => handler.FlyoutList.GetRowAtIndex(i)).ToArray();
			Assert.Equal(new[] { "Home", "Settings" }, rows.Select(FlyoutTitle));
			Assert.Null(handler.FlyoutList.GetRowAtIndex(2));

			// The current item's page fills the detail area.
			Assert.Equal(1, handler.Tabs.Count);
			Assert.False(handler.Tabs.Bar.Visible);
			Assert.Equal("home", GtkTestHost.Find<global::Gtk.Label>(handler.Tabs.HostAt(0)).First().Text);
			Assert.True(GtkTestHost.Find<global::Gtk.Label>(handler.Tabs.HostAt(0)).First().Width > 0);
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void ShellContentWithAContentTemplateShowsItsPage() => GtkTestHost.Run(() =>
		{
			// The project template's AppShell: ContentTemplate, so the page only exists once the handler asks for it.
			var shell = new Shell
			{
				Items =
				{
					new ShellContent { Title = "Home", ContentTemplate = new DataTemplate(() => Page("templated")) },
				},
			};
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = Assert.IsType<GtkShellHandler>(shell.Handler);

			Assert.Equal("templated", GtkTestHost.Find<global::Gtk.Label>(handler.Tabs.HostAt(0)).First().Text);
			Assert.True(GtkTestHost.Find<global::Gtk.Label>(handler.Tabs.HostAt(0)).First().Width > 0);
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void SelectingAFlyoutItemNavigatesToIt() => GtkTestHost.Run(() =>
		{
			var shell = new Shell { Items = { Item("Home", Page("home")), Item("Settings", Page("settings")) } };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;
			var settings = shell.Items[1];

			shell.FlyoutIsPresented = true;
			handler.FlyoutList.SelectRow(handler.FlyoutList.GetRowAtIndex(1));
			host.Pump();

			Assert.Same(settings, shell.CurrentItem);
			Assert.Equal("settings", GtkTestHost.Find<global::Gtk.Label>(handler.Tabs.HostAt(0)).First().Text);
			Assert.False(shell.FlyoutIsPresented);
		});

		[GtkFact]
		public void ChangingTheCurrentItemInCodeShowsItsPageAndSelectsTheFlyoutRow() => GtkTestHost.Run(() =>
		{
			var shell = new Shell { Items = { Item("Home", Page("home")), Item("Settings", Page("settings")) } };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;

			shell.CurrentItem = shell.Items[1];
			host.Pump();

			Assert.Equal("settings", GtkTestHost.Find<global::Gtk.Label>(handler.Tabs.HostAt(0)).First().Text);
			Assert.Equal(1, handler.FlyoutList.SelectedRow!.Index);
		});

		[GtkFact]
		public void SectionsOfATabBarBecomeTabs() => GtkTestHost.Run(() =>
		{
			var shell = new Shell
			{
				Items =
				{
					new TabBar
					{
						Items =
						{
							new ShellContent { Title = "One", Content = Page("one") },
							new ShellContent { Title = "Two", Content = Page("two") },
						},
					},
				},
			};
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;
			var tabs = handler.Tabs;

			Assert.Equal(2, tabs.Count);
			Assert.True(tabs.Bar.Visible);
			Assert.Equal("Two", GtkTestHost.Find<global::Gtk.Label>(tabs.ButtonAt(1)).Single().Text);

			tabs.ButtonAt(1).Active = true;
			host.Pump();
			Assert.Same(shell.CurrentItem.Items[1], shell.CurrentItem.CurrentItem);
			Assert.Equal("two", GtkTestHost.Find<global::Gtk.Label>(tabs.HostAt(1)).First().Text);

			shell.CurrentItem.CurrentItem = shell.CurrentItem.Items[0];
			host.Pump();
			Assert.Equal(0, tabs.SelectedIndex);
		});

		[GtkFact]
		public void PushedPagesReplaceTheSectionContentUntilPopped() => GtkTestHost.Run(() =>
		{
			var root = Page("root");
			var shell = new Shell { Items = { Item("Home", root) } };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;
			var section = shell.CurrentItem.CurrentItem;

			var pushed = Page("pushed");
			GtkTestHost.Await(section.Navigation.PushAsync(pushed, false));
			host.Pump();
			Assert.Same(pushed, handler.Tabs.HostAt(0).View);
			Assert.Equal("pushed", GtkTestHost.Find<global::Gtk.Label>(handler.Tabs.HostAt(0)).First().Text);

			GtkTestHost.Await(section.Navigation.PopAsync(false));
			host.Pump();
			Assert.Same(root, handler.Tabs.HostAt(0).View);
		});

		[GtkFact]
		public void FlyoutBehaviourAndPresentedStateReachThePanel() => GtkTestHost.Run(() =>
		{
			var shell = new Shell { Items = { Item("Home", Page("home")) }, FlyoutBehavior = FlyoutBehavior.Locked };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;
			var panel = handler.PlatformView;

			Assert.Equal(FlyoutBehavior.Locked, panel.Behavior);
			Assert.True(panel.FlyoutHost.Visible);

			shell.FlyoutBehavior = FlyoutBehavior.Flyout;
			panel.AnimationDuration = 0;
			host.Pump();
			Assert.Equal(FlyoutBehavior.Flyout, panel.Behavior);
			Assert.False(panel.FlyoutHost.Visible);

			// Shell's initial platform-initiated navigation resets FlyoutIsPresented when it lands late: let it settle first.
			for (int i = 0; i < 5; i++)
			{
				host.Pump();
				System.Threading.Thread.Sleep(20);
			}

			shell.FlyoutIsPresented = true;
			host.Pump();
			Assert.True(panel.FlyoutHost.Visible);

			shell.FlyoutBehavior = FlyoutBehavior.Disabled;
			host.Pump();
			Assert.False(panel.FlyoutHost.Visible);
		});

		// -------------------------------------------------------------- flyout parts

		static string[] TextsIn(global::Gtk.Widget widget) => GtkTestHost.Find<global::Gtk.Label>(widget).Select(l => l.Text).ToArray();

		[GtkFact]
		public void FlyoutHeaderAndFooterSurroundTheItemList() => GtkTestHost.Run(() =>
		{
			var shell = new Shell
			{
				Items = { Item("Home", Page("home")) },
				FlyoutHeader = new Label { Text = "the header" },
				FlyoutFooter = new Label { Text = "the footer" },
			};
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;

			Assert.True(handler.FlyoutHeaderHost.Visible);
			Assert.True(handler.FlyoutFooterHost.Visible);
			Assert.Contains("the header", TextsIn(handler.FlyoutHeaderHost));
			Assert.Contains("the footer", TextsIn(handler.FlyoutFooterHost));

			// Default header behaviour pins the header above the scrolling list.
			Assert.Same(handler.FlyoutHeaderHost, handler.FlyoutBox.FirstChild);
			Assert.Same(handler.FlyoutScroller, handler.FlyoutHeaderHost.NextSibling);
			Assert.Same(handler.FlyoutFooterHost, handler.FlyoutBox.LastChild);
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void FlyoutWithoutHeaderAndFooterHidesTheirHosts() => GtkTestHost.Run(() =>
		{
			var shell = new Shell { Items = { Item("Home", Page("home")) } };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;

			Assert.False(handler.FlyoutHeaderHost.Visible);
			Assert.False(handler.FlyoutFooterHost.Visible);

			shell.FlyoutHeader = new Label { Text = "late header" };
			host.Pump();
			Assert.True(handler.FlyoutHeaderHost.Visible);
			Assert.Contains("late header", TextsIn(handler.FlyoutHeaderHost));

			shell.FlyoutHeader = null;
			host.Pump();
			Assert.False(handler.FlyoutHeaderHost.Visible);
		});

		[GtkFact]
		public void ScrollingHeaderBehaviourPutsTheHeaderInTheScroller() => GtkTestHost.Run(() =>
		{
			var shell = new Shell
			{
				Items = { Item("Home", Page("home")) },
				FlyoutHeader = new Label { Text = "scrolling" },
				FlyoutHeaderBehavior = FlyoutHeaderBehavior.Scroll,
			};
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;

			Assert.NotSame(handler.FlyoutBox, handler.FlyoutHeaderHost.Parent);
			Assert.Same(handler.FlyoutScroller, handler.FlyoutBox.FirstChild);
			Assert.Same(handler.FlyoutHeaderHost, handler.FlyoutHeaderHost.Parent!.FirstChild);
		});

		[GtkFact]
		public void FlyoutHeaderTemplateIsAppliedToTheHeaderData() => GtkTestHost.Run(() =>
		{
			var template = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, new Binding(".", stringFormat: "hello {0}"));
				return label;
			});
			// Shell gives templated parts its own binding context.
			var shell = new Shell { Items = { Item("Home", Page("home")) }, FlyoutHeaderTemplate = template, BindingContext = "world" };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;

			Assert.Contains("hello world", TextsIn(handler.FlyoutHeaderHost));
		});

		[GtkFact]
		public void FlyoutContentReplacesTheItemList() => GtkTestHost.Run(() =>
		{
			var shell = new Shell
			{
				Items = { Item("Home", Page("home")) },
				FlyoutContent = new Label { Text = "custom flyout" },
				FlyoutFooter = new Label { Text = "footer stays" },
			};
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;

			Assert.Null(handler.FlyoutScroller.Parent);
			Assert.Same(handler.FlyoutBox, handler.FlyoutContentHost.Parent);
			Assert.Contains("custom flyout", TextsIn(handler.FlyoutContentHost));
			Assert.Contains("footer stays", TextsIn(handler.FlyoutFooterHost));
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void FlyoutBackgroundAndBackdropReachThePanel() => GtkTestHost.Run(() =>
		{
			var shell = new Shell
			{
				Items = { Item("Home", Page("home")) },
				FlyoutBackground = new SolidColorBrush(Colors.Red),
				FlyoutBackdrop = new SolidColorBrush(Color.FromRgba(0, 0, 255, 128)),
			};
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;

			Assert.Contains("background-color: rgba(255,0,0,1)", GtkStyle.For(handler.FlyoutBox).ToCss());
			Assert.Equal(Colors.Blue.Blue, handler.PlatformView.ScrimColor!.Blue);
			Assert.True(handler.PlatformView.ScrimColor!.Alpha < 1);
		});

		[GtkFact]
		public void FlyoutVerticalScrollModeMapsToThePolicy() => GtkTestHost.Run(() =>
		{
			var shell = new Shell { Items = { Item("Home", Page("home")) }, FlyoutVerticalScrollMode = ScrollMode.Disabled };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;

			Assert.Equal(global::Gtk.PolicyType.Never, handler.FlyoutVerticalPolicy);
			shell.FlyoutVerticalScrollMode = ScrollMode.Enabled;
			host.Pump();
			Assert.Equal(global::Gtk.PolicyType.Always, handler.FlyoutVerticalPolicy);
		});

		// -------------------------------------------------------------- flyout items

		[GtkFact]
		public void ItemTemplateRendersTheFlyoutRows() => GtkTestHost.Run(() =>
		{
			var shell = new Shell
			{
				Items = { Item("Home", Page("home")), Item("Settings", Page("settings")) },
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("Title", stringFormat: "> {0}"));
					return label;
				}),
			};
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;

			Assert.Equal(new[] { "> Home", "> Settings" }, Enumerable.Range(0, 2).Select(i => FlyoutTitle(handler.FlyoutList.GetRowAtIndex(i)!)));
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void DefaultFlyoutRowShowsTheTitleAndTheIcon() => GtkTestHost.Run(() =>
		{
			var path = TestImages.WritePng(8, 8, 0, 0, 255);
			try
			{
				var item = Item("Home", Page("home"));
				item.Icon = ImageSource.FromFile(path);
				var shell = new Shell { Items = { item } };
				using var host = new ControlsWindowHost(shell, 800, 500);
				var handler = (GtkShellHandler)shell.Handler!;

				var row = handler.FlyoutList.GetRowAtIndex(0)!;
				Assert.Equal("Home", FlyoutTitle(row));

				// The default cell has an Image next to the label; the icon loads into a picture/image widget of the row.
				Assert.True(GtkTestHost.PumpUntil(() => GtkTestHost.Find<global::Gtk.Widget>(row).Any(w => w is global::Gtk.Picture or global::Gtk.Image), host.PlatformWindow));
			}
			finally
			{
				System.IO.File.Delete(path);
			}
		});

		[GtkFact]
		public void GroupsOfFlyoutItemsAreSeparatedAndMenuItemsAreActivated() => GtkTestHost.Run(() =>
		{
			int clicks = 0;
			var menu = new MenuItem { Text = "Exit", Command = new Command(() => clicks++) };
			var home = new ShellContent { Title = "Home", Content = Page("home") };
			home.MenuItems.Add(menu);
			var multiple = new FlyoutItem { Title = "Multiple", FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems, Items = { home } };
			var shell = new Shell { Items = { multiple, Item("Settings", Page("settings")) } };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;

			var groups = ((IShellController)shell).GenerateFlyoutGrouping();
			Assert.True(groups.Count >= 2);
			int elements = groups.Sum(g => g.Count);

			var rows = Enumerable.Range(0, elements + groups.Count - 1).Select(i => handler.FlyoutList.GetRowAtIndex(i)!).ToArray();
			Assert.Null(handler.FlyoutList.GetRowAtIndex(rows.Length));
			var separators = rows.Where(r => !r.Selectable).ToArray();
			Assert.Equal(groups.Count - 1, separators.Length);
			Assert.All(separators, r => Assert.Single(GtkTestHost.Find<global::Gtk.Separator>(r)));

			var exit = rows.Single(r => r.Selectable && TextsIn(r).Contains("Exit"));
			handler.FlyoutList.SelectRow(exit);
			host.Pump();
			Assert.Equal(1, clicks);
		});

		// -------------------------------------------------------------- tabs

		[GtkFact]
		public void SectionIconsAppearOnTheTabs() => GtkTestHost.Run(() =>
		{
			var path = TestImages.WritePng(8, 8, 255, 0, 0);
			try
			{
				var shell = new Shell
				{
					Items =
					{
						new TabBar
						{
							Items =
							{
								new ShellContent { Title = "One", Icon = ImageSource.FromFile(path), Content = Page("one") },
								new ShellContent { Title = "Two", Content = Page("two") },
							},
						},
					},
				};
				using var host = new ControlsWindowHost(shell, 800, 500);
				var tabs = ((GtkShellHandler)shell.Handler!).Tabs;

				Assert.True(GtkTestHost.PumpUntil(() => GtkTestHost.Find<global::Gtk.Image>(tabs.ButtonAt(0)).Single().Visible, host.PlatformWindow));
				Assert.False(GtkTestHost.Find<global::Gtk.Image>(tabs.ButtonAt(1)).Single().Visible);
			}
			finally
			{
				System.IO.File.Delete(path);
			}
		});

		static FlyoutItem TwoContentItem(out ShellSection section, out ShellContent first, out ShellContent second)
		{
			first = new ShellContent { Title = "First", Content = Page("first") };
			second = new ShellContent { Title = "Second", ContentTemplate = new DataTemplate(() => Page("second")) };
			var tab = new Tab { Title = "Tab", Items = { first, second } };
			section = tab;
			return new FlyoutItem { Title = "Item", Items = { tab } };
		}

		[GtkFact]
		public void SeveralContentsOfASectionBecomeTopTabs() => GtkTestHost.Run(() =>
		{
			var item = TwoContentItem(out var section, out var first, out var second);
			var shell = new Shell { Items = { item } };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;

			// One section: the outer bar is hidden; the section shows nested tabs.
			Assert.False(handler.Tabs.Bar.Visible);
			var top = handler.TopTabsOf(section);
			Assert.NotNull(top);
			Assert.Equal(2, top!.Count);
			Assert.Equal("Second", GtkTestHost.Find<global::Gtk.Label>(top.ButtonAt(1)).Single().Text);
			Assert.Equal("first", GtkTestHost.Find<global::Gtk.Label>(top.HostAt(0)).First().Text);

			// The templated content is created when its tab is first selected.
			Assert.Null(top.HostAt(1).View);
			top.ButtonAt(1).Active = true;
			host.Pump();
			Assert.Same(second, section.CurrentItem);
			Assert.Equal("second", GtkTestHost.Find<global::Gtk.Label>(top.HostAt(1)).First().Text);

			section.CurrentItem = first;
			host.Pump();
			Assert.Equal(0, top.SelectedIndex);
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void TabBarIsVisibleHidesTheTabBar() => GtkTestHost.Run(() =>
		{
			var one = Page("one");
			var shell = new Shell
			{
				Items =
				{
					new TabBar
					{
						Items =
						{
							new ShellContent { Title = "One", Content = one },
							new ShellContent { Title = "Two", Content = Page("two") },
						},
					},
				},
			};
			using var host = new ControlsWindowHost(shell, 800, 500);
			var tabs = ((GtkShellHandler)shell.Handler!).Tabs;
			Assert.True(tabs.Bar.Visible);

			Shell.SetTabBarIsVisible(one, false);
			host.Pump();
			Assert.False(tabs.Bar.Visible);

			Shell.SetTabBarIsVisible(one, true);
			host.Pump();
			Assert.True(tabs.Bar.Visible);
		});

		[GtkFact]
		public void TabBarAppearanceColoursTheBar() => GtkTestHost.Run(() =>
		{
			var shell = new Shell
			{
				Items =
				{
					new TabBar
					{
						Items =
						{
							new ShellContent { Title = "One", Content = Page("one") },
							new ShellContent { Title = "Two", Content = Page("two") },
						},
					},
				},
			};
			Shell.SetTabBarBackgroundColor(shell, Colors.Green);
			Shell.SetTabBarTitleColor(shell, Colors.Yellow);
			Shell.SetTabBarUnselectedColor(shell, Colors.Gray);
			using var host = new ControlsWindowHost(shell, 800, 500);
			var tabs = ((GtkShellHandler)shell.Handler!).Tabs;

			var css = GtkStyle.For(tabs.Bar).ToCss();
			Assert.Contains("background-color: rgba(0,128,0,1)", css);
			Assert.Contains("rgba(255,255,0,1)", css);
			Assert.Contains("rgba(128,128,128,1)", css);

			// A change of the appearance later is followed.
			Shell.SetTabBarBackgroundColor(shell, Colors.Red);
			host.Pump();
			Assert.Contains("background-color: rgba(255,0,0,1)", GtkStyle.For(tabs.Bar).ToCss());
		});

		// -------------------------------------------------------------- toolbar

		[GtkFact]
		public void NavBarIsVisibleShowsAndHidesTheToolbar() => GtkTestHost.Run(() =>
		{
			var page = Page("home");
			var shell = new Shell { Items = { Item("Home", page) } };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var toolbar = Assert.IsType<GtkControlsToolbarHandler>(((IToolbarElement)shell).Toolbar!.Handler);

			Shell.SetNavBarIsVisible(page, false);
			host.Pump();
			Assert.False(toolbar.HeaderBar.Visible);

			Shell.SetNavBarIsVisible(page, true);
			host.Pump();
			Assert.True(toolbar.HeaderBar.Visible);
		});

		[GtkFact]
		public void ShellAppearanceColoursTheToolbar() => GtkTestHost.Run(() =>
		{
			var shell = new Shell { Items = { Item("Home", Page("home")) } };
			Shell.SetBackgroundColor(shell, Colors.Purple);
			using var host = new ControlsWindowHost(shell, 800, 500);
			var toolbar = (GtkControlsToolbarHandler)((IToolbarElement)shell).Toolbar!.Handler!;

			Assert.Contains("background-color: rgba(128,0,128,1)", GtkStyle.For(toolbar.HeaderBar).ToCss());
		});

		[GtkFact]
		public void FlyoutIconReplacesTheDrawerToggleIcon() => GtkTestHost.Run(() =>
		{
			var path = TestImages.WritePng(8, 8, 0, 255, 0);
			try
			{
				var shell = new Shell { Items = { Item("Home", Page("home")) }, FlyoutIcon = ImageSource.FromFile(path) };
				using var host = new ControlsWindowHost(shell, 800, 500);
				var toolbar = (GtkControlsToolbarHandler)((IToolbarElement)shell).Toolbar!.Handler!;

				Assert.True(GtkTestHost.PumpUntil(() => (toolbar.DrawerButton.Child as global::Gtk.Image)?.Paintable is Gdk.Texture, host.PlatformWindow));

				shell.FlyoutIcon = null;
				Assert.True(GtkTestHost.PumpUntil(() => (toolbar.DrawerButton.Child as global::Gtk.Image)?.Paintable is not Gdk.Texture, host.PlatformWindow));
			}
			finally
			{
				System.IO.File.Delete(path);
			}
		});

		// -------------------------------------------------------------- search

		sealed class Fruit
		{
			public Fruit(string name) => Name = name;

			public string Name { get; }

			public override string ToString() => "fruit " + Name;
		}

		[GtkFact]
		public void SearchHandlerShowsAnEntryThatFollowsTheQuery() => GtkTestHost.Run(() =>
		{
			var page = Page("home");
			var search = new SearchHandler { Placeholder = "Find", Query = "app" };
			Shell.SetSearchHandler(page, search);
			var shell = new Shell { Items = { Item("Home", page) } };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var gtkSearch = ((GtkShellHandler)shell.Handler!).Search;

			Assert.True(gtkSearch.Bar.Visible);
			Assert.Equal("Find", gtkSearch.Entry.PlaceholderText);
			Assert.Equal("app", gtkSearch.Entry.Text);

			// Typing updates the handler; the handler updates the entry.
			gtkSearch.Entry.Text = "pear";
			Assert.Equal("pear", search.Query);
			search.Query = "plum";
			Assert.Equal("plum", gtkSearch.Entry.Text);

			search.SearchBoxVisibility = SearchBoxVisibility.Hidden;
			host.Pump();
			Assert.False(gtkSearch.Bar.Visible);
			ControlsWindowHost.AssertNoComplaints();
		});

		[GtkFact]
		public void PageWithoutASearchHandlerHasNoSearchBar() => GtkTestHost.Run(() =>
		{
			var withSearch = Page("with");
			Shell.SetSearchHandler(withSearch, new SearchHandler());
			var shell = new Shell { Items = { Item("With", withSearch), Item("Without", Page("without")) } };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var handler = (GtkShellHandler)shell.Handler!;
			Assert.True(handler.Search.Bar.Visible);

			shell.CurrentItem = shell.Items[1];
			host.Pump();
			Assert.False(handler.Search.Bar.Visible);

			shell.CurrentItem = shell.Items[0];
			host.Pump();
			Assert.True(handler.Search.Bar.Visible);
		});

		[GtkFact]
		public void SearchHandlerSuggestionsFillTheResultListAndReportSelection() => GtkTestHost.Run(() =>
		{
			var page = Page("home");
			var fruits = new System.Collections.ObjectModel.ObservableCollection<Fruit>(new[] { new Fruit("apple"), new Fruit("pear") });
			var confirmed = 0;
			var search = new SearchHandler { ShowsResults = true, DisplayMemberName = "Name", Command = new Command(() => confirmed++) };
			Shell.SetSearchHandler(page, search);
			var shell = new Shell { Items = { Item("Home", page) } };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var gtkSearch = ((GtkShellHandler)shell.Handler!).Search;

			// A search handler needs the application's dispatcher to proxy its source: assign it once attached.
			search.ItemsSource = fruits;
			host.Pump();

			var rows = Enumerable.Range(0, 2).Select(i => gtkSearch.Results.GetRowAtIndex(i)!).ToArray();
			Assert.Null(gtkSearch.Results.GetRowAtIndex(2));
			Assert.Equal(new[] { "apple", "pear" }, rows.Select(r => TextsIn(r).Single()));

			// The list follows the source.
			fruits.Add(new Fruit("plum"));
			Assert.NotNull(gtkSearch.Results.GetRowAtIndex(2));

			// Enter confirms the query.
			GLib.Signal.Emit(gtkSearch.Entry, "activate");
			Assert.Equal(1, confirmed);

			gtkSearch.Results.SelectRow(gtkSearch.Results.GetRowAtIndex(1));
			Assert.Same(fruits[1], search.SelectedItem);
		});

		[GtkFact]
		public void SearchHandlerItemTemplateRendersTheSuggestions() => GtkTestHost.Run(() =>
		{
			var page = Page("home");
			var search = new SearchHandler
			{
				ShowsResults = true,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, new Binding("Name", stringFormat: "* {0}"));
					return label;
				}),
			};
			Shell.SetSearchHandler(page, search);
			var shell = new Shell { Items = { Item("Home", page) } };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var gtkSearch = ((GtkShellHandler)shell.Handler!).Search;
			search.ItemsSource = new[] { new Fruit("kiwi") };
			host.Pump();

			Assert.Equal("* kiwi", TextsIn(gtkSearch.Results.GetRowAtIndex(0)!).Single());
		});

		[GtkLinuxFact]
		public void SearchSuggestionsPopUpWhileTheQueryHasMatches() => GtkTestHost.Run(() =>
		{
			var page = Page("home");
			var search = new SearchHandler { ShowsResults = true, DisplayMemberName = "Name" };
			Shell.SetSearchHandler(page, search);
			var shell = new Shell { Items = { Item("Home", page) } };
			using var host = new ControlsWindowHost(shell, 800, 500);
			var gtkSearch = ((GtkShellHandler)shell.Handler!).Search;
			search.ItemsSource = new[] { new Fruit("kiwi") };

			Assert.False(gtkSearch.Popover?.Visible ?? false);

			search.Query = "ki";
			host.Pump();
			Assert.True(gtkSearch.Popover?.Visible ?? false);

			search.Query = string.Empty;
			host.Pump();
			Assert.False(gtkSearch.Popover?.Visible ?? false);
		});
	}
}
