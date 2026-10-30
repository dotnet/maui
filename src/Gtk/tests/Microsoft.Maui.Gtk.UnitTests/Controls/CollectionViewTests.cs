using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Gtk.Items;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>CollectionView on <c>Gtk.ListView</c>/<c>Gtk.GridView</c> over a C# GListModel, on real widgets.</summary>
	public class CollectionViewTests
	{
		static DataTemplate LabelTemplate(string? automationPrefix = null) => new(() =>
		{
			var label = new Label { HeightRequest = 30 };
			label.SetBinding(Label.TextProperty, ".");
			return label;
		});

		static GtkItemsController Controller(CollectionView view) => ((GtkCollectionViewHandler)view.Handler!).Controller!;

		static IEnumerable<string> Texts(GtkItemsController controller) =>
			controller.BoundHosts.Select(h => ((Label)h.View!).Text).OrderBy(t => t, StringComparer.Ordinal);

		static (CollectionView View, ControlsWindowHost Host) Create(Action<CollectionView> configure, int width = 400, int height = 300)
		{
			var view = new CollectionView { ItemTemplate = LabelTemplate() };
			configure(view);
			var host = new ControlsWindowHost(new ContentPage { Content = view }, width, height);
			host.Pump(10);
			return (view, host);
		}

		[GtkFact]
		public void UsesTheGtkHandlerAndAListViewForALinearLayout() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v => v.ItemsSource = new[] { "a", "b" });
			using (host)
			{
				Assert.IsType<GtkCollectionViewHandler>(view.Handler);
				var controller = Controller(view);
				Assert.IsType<global::Gtk.ListView>(controller.List);
				Assert.Equal(global::Gtk.Orientation.Vertical, controller.List!.Orientation);
				Assert.Equal(2, controller.Source!.Count);
			}
		});

		[GtkFact]
		public void OnlyTheVisibleRowsHaveWidgets() => GtkTestHost.Run(() =>
		{
			var items = Enumerable.Range(0, 500).Select(i => "item " + i).ToList();
			var (view, host) = Create(v => v.ItemsSource = items);
			using (host)
			{
				var controller = Controller(view);
				Assert.Equal(500, controller.Source!.Count);
				Assert.InRange(controller.BoundHosts.Count, 3, 250);
				Assert.Contains("item 0", Texts(controller));
				Assert.DoesNotContain("item 499", Texts(controller));

				// Rows are laid out by their content (30 high), one below the other.
				var first = controller.BoundHosts.First(h => ((Label)h.View!).Text == "item 0");
				var second = controller.BoundHosts.First(h => ((Label)h.View!).Text == "item 1");
				Assert.Equal(30, first.Height);
				// GTK adds its row padding around the content.
				Assert.InRange(GtkTestHost.BoundsIn(second, first.Parent).Y - GtkTestHost.BoundsIn(first, first.Parent).Y, 30, 40);
				GtkTestHost.AssertNoComplaints();
			}
		});

		[GtkFact]
		public void ScrollingRecyclesTheRowViews() => GtkTestHost.Run(() =>
		{
			int created = 0;
			var template = new DataTemplate(() =>
			{
				created++;
				var label = new Label { HeightRequest = 30 };
				label.SetBinding(Label.TextProperty, ".");
				return label;
			});
			var items = Enumerable.Range(0, 1000).Select(i => "item " + i).ToList();
			var (view, host) = Create(v =>
			{
				v.ItemTemplate = template;
				v.ItemsSource = items;
			});
			using (host)
			{
				var controller = Controller(view);
				int afterFirstLayout = created;

				var adjustment = controller.Scroller.Vadjustment;
				for (int i = 1; i <= 20; i++)
				{
					adjustment.Value = i * 250;
					host.Pump(3);
				}

				Assert.Contains(Texts(controller), t => int.Parse(t.AsSpan(5)) > 150);
				// The rows scrolled through (about 600 of them) did not each create a view.
				Assert.True(created < afterFirstLayout + 120, $"created {created} views, {afterFirstLayout} after the first layout");
			}
		});

		[GtkFact]
		public void ObservableChangesUpdateTheRows() => GtkTestHost.Run(() =>
		{
			var items = new ObservableCollection<string> { "one", "two", "three" };
			var (view, host) = Create(v => v.ItemsSource = items);
			using (host)
			{
				var controller = Controller(view);
				Assert.Equal(new[] { "one", "three", "two" }, Texts(controller));

				items.Insert(0, "zero");
				host.Pump();
				Assert.Equal(4, controller.Source!.Count);
				Assert.Equal("zero", controller.Source.RowAt(0).Data);
				Assert.Contains("zero", Texts(controller));

				items.RemoveAt(1);
				host.Pump();
				Assert.Equal(new[] { "three", "two", "zero" }, Texts(controller));

				items[0] = "ZERO";
				host.Pump();
				Assert.Contains("ZERO", Texts(controller));
				Assert.DoesNotContain("zero", Texts(controller));

				items.Move(0, 2);
				host.Pump();
				Assert.Equal(new[] { "two", "three", "ZERO" }, Enumerable.Range(0, controller.Source.Count).Select(i => (string)controller.Source.RowAt(i).Data!));

				items.Clear();
				host.Pump();
				Assert.Equal(0, controller.Source.Count);
				Assert.Empty(controller.BoundHosts);
			}
		});

		[GtkFact]
		public void ReplacingTheItemsSourceRebuildsTheRows() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v => v.ItemsSource = new[] { "a" });
			using (host)
			{
				view.ItemsSource = new[] { "x", "y", "z" };
				host.Pump();
				var controller = Controller(view);
				Assert.Equal(3, controller.Source!.Count);
				Assert.Equal(new[] { "x", "y", "z" }, Texts(controller));
			}
		});

		[GtkFact]
		public void TemplateSelectorChoosesPerItem() => GtkTestHost.Run(() =>
		{
			var selector = new TestSelector
			{
				Even = new DataTemplate(() => new Label { Text = "even", HeightRequest = 20 }),
				Odd = new DataTemplate(() => new Label { Text = "odd", HeightRequest = 20 }),
			};
			var (view, host) = Create(v =>
			{
				v.ItemTemplate = selector;
				v.ItemsSource = new[] { 0, 1, 2, 3 };
			});
			using (host)
			{
				var texts = Controller(view).BoundHosts.OrderBy(h => h.ListItem!.Position).Select(h => ((Label)h.View!).Text);
				Assert.Equal(new[] { "even", "odd", "even", "odd" }, texts);
			}
		});

		sealed class TestSelector : DataTemplateSelector
		{
			public DataTemplate? Even { get; set; }
			public DataTemplate? Odd { get; set; }

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container) => ((int)item % 2 == 0 ? Even : Odd)!;
		}

		[GtkFact]
		public void WithoutATemplateTheItemsToStringIsShown() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v =>
			{
				v.ItemTemplate = null;
				v.ItemsSource = new object[] { 1, "two" };
			});
			using (host)
				Assert.Equal(new[] { "1", "two" }, Texts(Controller(view)));
		});

		[GtkFact]
		public void GridLayoutUsesAGridViewWithTheSpan() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v =>
			{
				v.ItemsLayout = new GridItemsLayout(3, ItemsLayoutOrientation.Vertical);
				v.ItemsSource = Enumerable.Range(0, 9).Select(i => i.ToString()).ToList();
			}, 450, 300);
			using (host)
			{
				var controller = Controller(view);
				var grid = Assert.IsType<global::Gtk.GridView>(controller.List);
				Assert.Equal(3u, grid.MinColumns);
				Assert.Equal(3u, grid.MaxColumns);

				// Three items per row: the first three share a vertical position.
				var top = controller.BoundHosts.Where(h => int.Parse(((Label)h.View!).Text) < 3).Select(h => GtkTestHost.BoundsIn(h, controller.Scroller).Y).Distinct();
				Assert.Single(top);
			}
		});

		[GtkFact]
		public void SwitchingTheLayoutRecreatesTheNativeList() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v => v.ItemsSource = new[] { "a", "b", "c", "d" });
			using (host)
			{
				var controller = Controller(view);
				Assert.IsType<global::Gtk.ListView>(controller.List);

				view.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical);
				host.Pump();
				Assert.IsType<global::Gtk.GridView>(controller.List);
				Assert.Equal(4, controller.Source!.Count);

				view.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);
				host.Pump();
				Assert.IsType<global::Gtk.ListView>(controller.List);
				Assert.Equal(global::Gtk.Orientation.Horizontal, controller.List!.Orientation);
			}
		});

		[GtkFact]
		public void ItemSpacingBecomesRowMargins() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v =>
			{
				v.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical) { ItemSpacing = 10 };
				v.ItemsSource = new[] { "a", "b" };
			});
			using (host)
			{
				var row = Controller(view).BoundHosts.First();
				Assert.Equal(5, row.MarginTop);
				Assert.Equal(5, row.MarginBottom);
			}
		});

		[GtkFact]
		public void HeaderFooterAndEmptyViewAreShownAroundTheList() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v =>
			{
				v.Header = "the header";
				v.Footer = new Label { Text = "the footer" };
				v.EmptyView = "nothing here";
				v.ItemsSource = new ObservableCollection<string>();
			});
			using (host)
			{
				var controller = Controller(view);
				Assert.Equal("the header", ((Label)controller.HeaderHost!.View!).Text);
				Assert.Equal("the footer", ((Label)controller.FooterHost!.View!).Text);

				// Empty: the empty view replaces the list.
				Assert.Same(controller.EmptyHost, controller.Root.FirstChild!.NextSibling is { } ? FindStackChild(controller) : null);
				Assert.Equal("nothing here", ((Label)controller.EmptyHost.View!).Text);

				((ObservableCollection<string>)view.ItemsSource).Add("now there is one");
				host.Pump();
				Assert.Same(controller.Scroller, FindStackChild(controller));

				// Header above, list in the middle, footer below.
				Assert.True(GtkTestHost.BoundsIn(controller.HeaderHost, controller.Root).Y < GtkTestHost.BoundsIn(controller.Scroller, controller.Root).Y);
				Assert.True(GtkTestHost.BoundsIn(controller.FooterHost, controller.Root).Y > GtkTestHost.BoundsIn(controller.Scroller, controller.Root).Y);

				view.Header = null;
				host.Pump();
				Assert.Null(controller.HeaderHost);
			}
		});

		static global::Gtk.Widget? FindStackChild(GtkItemsController controller) =>
			GtkTestHost.Find<global::Gtk.Stack>(controller.Root).First().VisibleChild;

		[GtkFact]
		public void HeaderTemplateGetsTheHeaderAsBindingContext() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v =>
			{
				v.Header = "bound";
				v.HeaderTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, ".");
					return label;
				});
				v.ItemsSource = new[] { "a" };
			});
			using (host)
				Assert.Equal("bound", ((Label)Controller(view).HeaderHost!.View!).Text);
		});

		[GtkFact]
		public void SingleSelectionFlowsBothWays() => GtkTestHost.Run(() =>
		{
			var items = new[] { "a", "b", "c" };
			var (view, host) = Create(v =>
			{
				v.SelectionMode = SelectionMode.Single;
				v.ItemsSource = items;
			});
			using (host)
			{
				var controller = Controller(view);
				var selection = Assert.IsType<global::Gtk.SingleSelection>(controller.Selection);

				var changes = new List<object?>();
				view.SelectionChanged += (_, e) => changes.Add(e.CurrentSelection.FirstOrDefault());

				// From the user: native selection -> SelectedItem.
				selection.SelectItem(1, true);
				host.Pump();
				Assert.Equal("b", view.SelectedItem);
				Assert.Equal(new object?[] { "b" }, changes);

				// From the app: SelectedItem -> native selection.
				view.SelectedItem = "c";
				host.Pump();
				Assert.Equal(2u, selection.Selected);

				view.SelectedItem = null;
				host.Pump();
				Assert.Equal(uint.MaxValue, selection.Selected);
			}
		});

		[GtkFact]
		public void SelectedRowsGetTheSelectedVisualState() => GtkTestHost.Run(() =>
		{
			var template = new DataTemplate(() =>
			{
				var label = new Label { HeightRequest = 30 };
				label.SetBinding(Label.TextProperty, ".");
				var selected = new VisualState { Name = VisualStateManager.CommonStates.Selected };
				selected.Setters.Add(new Setter { Property = VisualElement.BackgroundColorProperty, Value = Colors.Red });
				var normal = new VisualState { Name = VisualStateManager.CommonStates.Normal };
				var group = new VisualStateGroup { Name = "CommonStates" };
				group.States.Add(normal);
				group.States.Add(selected);
				VisualStateManager.SetVisualStateGroups(label, new VisualStateGroupList { group });
				return label;
			});
			var (view, host) = Create(v =>
			{
				v.ItemTemplate = template;
				v.SelectionMode = SelectionMode.Single;
				v.ItemsSource = new[] { "a", "b" };
				v.SelectedItem = "b";
			});
			using (host)
			{
				var controller = Controller(view);
				var b = (Label)controller.BoundHosts.Single(h => ((Label)h.View!).Text == "b").View!;
				var a = (Label)controller.BoundHosts.Single(h => ((Label)h.View!).Text == "a").View!;
				Assert.Equal(Colors.Red, b.BackgroundColor);
				Assert.NotEqual(Colors.Red, a.BackgroundColor);

				view.SelectedItem = "a";
				host.Pump();
				Assert.Equal(Colors.Red, a.BackgroundColor);
				Assert.NotEqual(Colors.Red, b.BackgroundColor);
			}
		});

		[GtkFact]
		public void MultipleSelectionFlowsBothWays() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v =>
			{
				v.SelectionMode = SelectionMode.Multiple;
				v.ItemsSource = new[] { "a", "b", "c", "d" };
			});
			using (host)
			{
				var controller = Controller(view);
				var selection = Assert.IsType<global::Gtk.MultiSelection>(controller.Selection);

				selection.SelectItem(0, false);
				selection.SelectItem(2, false);
				host.Pump();
				Assert.Equal(new object[] { "a", "c" }, view.SelectedItems.OrderBy(i => i).ToArray());

				view.SelectedItems.Clear();
				view.SelectedItems.Add("d");
				host.Pump();
				Assert.True(selection.IsSelected(3));
				Assert.False(selection.IsSelected(0));
			}
		});

		[GtkFact]
		public void SelectionModeNoneUsesNoSelection() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v => v.ItemsSource = new[] { "a" });
			using (host)
			{
				var controller = Controller(view);
				Assert.IsType<global::Gtk.NoSelection>(controller.Selection);

				view.SelectionMode = SelectionMode.Single;
				host.Pump();
				Assert.IsType<global::Gtk.SingleSelection>(controller.Selection);
				Assert.Equal(1, controller.Source!.Count);
			}
		});

		public class Animal
		{
			public Animal(string name) => Name = name;
			public string Name { get; }
			public override string ToString() => Name;
		}

		public class AnimalGroup : ObservableCollection<Animal>
		{
			public AnimalGroup(string name, params Animal[] animals)
				: base(animals) => Name = name;

			public string Name { get; }
			public override string ToString() => Name;
		}

		[GtkFact]
		public void GroupsGetHeaderRowsAndTheirItems() => GtkTestHost.Run(() =>
		{
			var groups = new ObservableCollection<AnimalGroup>
			{
				new("Cats", new Animal("Tom"), new Animal("Felix")),
				new("Dogs", new Animal("Rex")),
			};
			var (view, host) = Create(v =>
			{
				v.IsGrouped = true;
				v.ItemsSource = groups;
				v.ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label { HeightRequest = 24 };
					label.SetBinding(Label.TextProperty, "Name");
					return label;
				});
				v.GroupHeaderTemplate = new DataTemplate(() =>
				{
					var label = new Label { HeightRequest = 24, FontAttributes = FontAttributes.Bold };
					label.SetBinding(Label.TextProperty, "Name");
					return label;
				});
			});
			using (host)
			{
				var controller = Controller(view);
				var kinds = Enumerable.Range(0, controller.Source!.Count).Select(i => controller.Source.RowAt(i).Kind).ToArray();
				Assert.Equal(new[] { GtkRowKind.GroupHeader, GtkRowKind.Item, GtkRowKind.Item, GtkRowKind.GroupHeader, GtkRowKind.Item }, kinds);

				var texts = controller.BoundHosts.OrderBy(h => h.ListItem!.Position).Select(h => ((Label)h.View!).Text);
				Assert.Equal(new[] { "Cats", "Tom", "Felix", "Dogs", "Rex" }, texts);

				// A change inside a group rebuilds the rows.
				groups[1].Add(new Animal("Fido"));
				host.Pump();
				Assert.Equal(6, controller.Source.Count);
			}
		});

		[GtkFact]
		public void ScrollToMakesTheRowVisible() => GtkTestHost.Run(() =>
		{
			var items = Enumerable.Range(0, 300).Select(i => "item " + i).ToList();
			var (view, host) = Create(v => v.ItemsSource = items);
			using (host)
			{
				var controller = Controller(view);
				Assert.Equal(0, controller.Scroller.Vadjustment.Value);

				view.ScrollTo(200, position: ScrollToPosition.Start, animate: false);
				Assert.True(GtkTestHost.PumpUntil(() => controller.Scroller.Vadjustment.Value > 1000, host.PlatformWindow));
				Assert.True(GtkTestHost.PumpUntil(() => Texts(controller).Contains("item 200"), host.PlatformWindow));

				view.ScrollTo("item 5", position: ScrollToPosition.Start, animate: false);
				Assert.True(GtkTestHost.PumpUntil(() => Texts(controller).Contains("item 5"), host.PlatformWindow));
			}
		});

		[GtkFact]
		public void ScrollBarVisibilityMapsToThePolicies() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v =>
			{
				v.ItemsSource = new[] { "a" };
				v.VerticalScrollBarVisibility = ScrollBarVisibility.Never;
				v.HorizontalScrollBarVisibility = ScrollBarVisibility.Always;
			});
			using (host)
			{
				var scroller = Controller(view).Scroller;
				Assert.Equal(global::Gtk.PolicyType.Never, scroller.VscrollbarPolicy);
				Assert.Equal(global::Gtk.PolicyType.Always, scroller.HscrollbarPolicy);
			}
		});

		[GtkFact]
		public void TheCollectionViewFillsItsAllocationAndLogsNoComplaints() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v => v.ItemsSource = Enumerable.Range(0, 50).Select(i => "item " + i).ToList(), 500, 400);
			using (host)
			{
				// The page content area: the window minus its title bar.
				var content = host.PlatformWindow.Child!;
				var bounds = host.Bounds(view);
				Assert.Equal(content.Width, bounds.Width);
				Assert.Equal(content.Height, bounds.Height);
				GtkTestHost.AssertNoComplaints();
			}
		});

		[GtkFact]
		public void DisconnectingReleasesTheRowViews() => GtkTestHost.Run(() =>
		{
			var (view, host) = Create(v => v.ItemsSource = new[] { "a", "b" });
			using (host)
			{
				var controller = Controller(view);
				var label = (Label)controller.BoundHosts.First().View!;
				Assert.NotNull(label.Parent);

				view.Handler!.DisconnectHandler();
				Assert.Null(view.Handler);
			}
		});
	}
}
