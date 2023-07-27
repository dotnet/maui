#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class VisualTreeHelperTests : BaseTestFixture, IDisposable
	{
		readonly List<(Element? Parent, VisualTreeChangeEventArgs Args)> _treeEvents = new();

		public VisualTreeHelperTests()
		{
			VisualDiagnostics.VisualTreeChanged += OnVisualTreeChanged;
		}

		public void Dispose()
		{
			VisualDiagnostics.VisualTreeChanged -= OnVisualTreeChanged;
		}

		public Action<Element?, VisualTreeChangeEventArgs>? VisualTreeChanged { get; set; }

		void OnVisualTreeChanged(object? sender, VisualTreeChangeEventArgs e)
		{
			Assert.True(e.ChildIndex >= 0, $"Visual Tree inaccurate when OnVisualTreeChanged called. ChildIndex: {e.ChildIndex}");
			_treeEvents.Add((sender as Element, e));
			VisualTreeChanged?.Invoke(sender as Element, e);
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void LayoutChildren(Type TLayout)
		{
			var layout = CreateLayout(TLayout)!;

			var label = new Label();
			var button = new Button();
			layout.Children.Add(label);
			layout.Children.Add(button);

			var visualChildren = (layout as IVisualTreeElement).GetVisualChildren();

			Assert.Equal(layout.Children.Count, visualChildren.Count);
			Assert.Equal(label, visualChildren[0]);
			Assert.Equal(button, visualChildren[1]);
		}

		[Fact]
		public async Task ModalChildren()
		{
			CreateNewApp(out _, out var window, out var page);

			var modalPage = new ContentPage();

			await window.Navigation.PushModalAsync(modalPage);

			var windowChildren = (window as IVisualTreeElement).GetVisualChildren();
			var modalParent = (modalPage as IVisualTreeElement).GetVisualParent();

			Assert.Equal(2, windowChildren.Count);
			Assert.Equal(page, windowChildren[0]);
			Assert.Equal(modalPage, windowChildren[1]);
			Assert.Equal(window, modalParent);
		}

		[Fact]
		public async Task ModalChildrenFiresDiagnosticEvents()
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			CreateNewApp(out _, out var window, out _);

			var modalPage = new ContentPage();

			await window.Navigation.PushModalAsync(modalPage);

			Assert.Single(_treeEvents);
			var (parent, args) = _treeEvents[0];

			Assert.Equal(window, parent);
			Assert.Equal(window, args.Parent);
			Assert.Equal(modalPage, args.Child);
			Assert.Equal(1, args.ChildIndex);
			Assert.Equal(VisualTreeChangeType.Add, args.ChangeType);

			_treeEvents.Clear();

			await window.Navigation.PopModalAsync();

			Assert.Single(_treeEvents);
			(parent, args) = _treeEvents[0];

			Assert.Equal(window, parent);
			Assert.Equal(window, args.Parent);
			Assert.Equal(modalPage, args.Child);
			Assert.Equal(1, args.ChildIndex);
			Assert.Equal(VisualTreeChangeType.Remove, args.ChangeType);
		}

		[Fact]
		public void ApplicationChildren()
		{
			CreateNewApp(out var app, out var window, out var page);

			var appChildren = ((IVisualTreeElement)app).GetVisualChildren();
			var windowChildren = ((IVisualTreeElement)window).GetVisualChildren();
			var pageChildren = ((IVisualTreeElement)page).GetVisualChildren();

			Assert.Single(appChildren);
			Assert.Equal(window, appChildren[0]);
			Assert.Single(windowChildren);
			Assert.Equal(page, windowChildren[0]);
			Assert.Empty(pageChildren);
		}

		[Fact]
		public void VisualElementParent()
		{
			CreateNewApp(out var app, out var window, out var page);

			var appParent = ((IVisualTreeElement)app).GetVisualParent();
			var windowParent = ((IVisualTreeElement)window).GetVisualParent();
			var pageParent = ((IVisualTreeElement)page).GetVisualParent();

			Assert.Null(appParent);
			Assert.Equal(app, windowParent);
			Assert.Equal(window, pageParent);
		}

		[Fact]
		public async Task AddPageContentFiresVisualTreeChanged()
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			CreateNewApp(out _, out _, out var page);
			var button = new Button();

			page.Content = button;

			Assert.Single(_treeEvents);

			var (parent, args) = _treeEvents[0];
			Assert.Equal(page, parent);
			Assert.Equal(page, args.Parent);
			Assert.Equal(button, args.Child);
			Assert.Equal(0, args.ChildIndex);
			Assert.Equal(VisualTreeChangeType.Add, args.ChangeType);
		}

		[Fact]
		public async Task RemovePageContentFiresVisualTreeChanged()
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			CreateNewApp(out _, out _, out var page);
			var button = new Button();
			page.Content = button;
			_treeEvents.Clear();

			page.Content = null;

			Assert.Single(_treeEvents);

			var (parent, args) = _treeEvents[0];
			Assert.Equal(page, parent);
			Assert.Equal(page, args.Parent);
			Assert.Equal(button, args.Child);
			Assert.Equal(0, args.ChildIndex);
			Assert.Equal(VisualTreeChangeType.Remove, args.ChangeType);
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void AddLayoutChildFiresVisualTreeChanged(Type TLayout)
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			CreateNewApp(out _, out _, out var page);

			var layout = CreateLayout(TLayout)!;
			page.Content = layout;
			_treeEvents.Clear();

			var button1 = new Button();
			var button2 = new Button();
			var button3 = new Button();
			var buttons = new[] { button1, button2, button3 };

			for (var i = 0; i < buttons.Length; i++)
			{
				var button = buttons[i];
				layout.Add(button);

				Assert.Equal(i + 1, _treeEvents.Count);

				var (parent, args) = _treeEvents[i];
				Assert.Equal(layout, parent);
				Assert.Equal(layout, args.Parent);
				Assert.Equal(button, args.Child);
				Assert.Equal(i, args.ChildIndex);
				Assert.Equal(VisualTreeChangeType.Add, args.ChangeType);
			}
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void InsertLayoutChildFiresVisualTreeChanged(Type TLayout)
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			CreateNewApp(out _, out _, out var page);

			var layout = CreateLayout(TLayout)!;
			page.Content = layout;
			layout.Add(new Button());
			layout.Add(new Button());
			layout.Add(new Button());
			_treeEvents.Clear();

			var button = new Button();
			layout.Insert(2, button);

			Assert.Single(_treeEvents);

			var (parent, args) = _treeEvents[0];
			Assert.Equal(layout, parent);
			Assert.Equal(layout, args.Parent);
			Assert.Equal(button, args.Child);
			Assert.Equal(2, args.ChildIndex);
			Assert.Equal(VisualTreeChangeType.Add, args.ChangeType);
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void RemoveLayoutChildFiresVisualTreeChanged(Type TLayout)
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			CreateNewApp(out _, out _, out var page);

			var layout = CreateLayout(TLayout)!;
			page.Content = layout;

			var button = new Button();
			layout.Add(button);
			_treeEvents.Clear();

			layout.Remove(button);

			Assert.Single(_treeEvents);

			var (parent, args) = _treeEvents[0];
			Assert.Equal(layout, parent);
			Assert.Equal(layout, args.Parent);
			Assert.Equal(button, args.Child);
			Assert.Equal(0, args.ChildIndex);
			Assert.Equal(VisualTreeChangeType.Remove, args.ChangeType);
		}

		[Theory]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(Grid))]
		[InlineData(typeof(StackLayout))]
		public void ClearLayoutChildrenFiresVisualTreeChanged(Type TLayout)
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			CreateNewApp(out _, out _, out var page);

			var layout = CreateLayout(TLayout);
			page.Content = layout;

			var button1 = new Button();
			var button2 = new Button();
			var button3 = new Button();
			var buttons = new[] { button1, button2, button3 };

			layout.Add(button1);
			layout.Add(button2);
			layout.Add(button3);
			_treeEvents.Clear();

			layout.Clear();

			Assert.Equal(3, _treeEvents.Count);

			for (var i = _treeEvents.Count - 1; i >= 0; i--)
			{
				var (parent, args) = _treeEvents[_treeEvents.Count - 1 - i];

				Assert.Equal(layout, parent);
				Assert.Equal(layout, args.Parent);
				Assert.Equal(buttons[i], args.Child);
				Assert.Equal(i, args.ChildIndex);
				Assert.Equal(VisualTreeChangeType.Remove, args.ChangeType);
			}
		}

		[Fact]
		public void LayoutTableView()
		{
			var tableview = new TableView
			{
				Root = new TableRoot
				{
					new TableSection("Ring")
					{
						// TableSection constructor takes title as an optional parameter
						new SwitchCell { Text = "New Voice Mail" },
						new SwitchCell { Text = "New Mail", On = true }
					}
				},
				Intent = TableIntent.Settings
			};

			var tableRootElement = (tableview as IVisualTreeElement).GetVisualChildren();
			Assert.IsType<TableRoot>(tableRootElement[0]);
			var tableRoot = (TableRoot)tableRootElement[0];
			var tableSectionElement = (tableRoot[0] as IVisualTreeElement).GetVisualChildren();
			Assert.Equal(2, tableSectionElement.Count);
		}

		Layout CreateLayout(Type TLayout)
		{
			var layout = (Layout)Activator.CreateInstance(TLayout)!;
			layout.IsPlatformEnabled = true;

			return layout;
		}

		void CreateNewApp(out Application app, out Window window, out ContentPage page)
		{
			page = new ContentPage();

			app = new Application
			{
				MainPage = page
			};

			var iapp = app as IApplication;

			window = (Window)iapp.CreateWindow(null!);

			_treeEvents.Clear();
		}
	}
}
