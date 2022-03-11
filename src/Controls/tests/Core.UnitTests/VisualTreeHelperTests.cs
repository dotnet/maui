#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
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
			_treeEvents.Add((sender as Element, e));
			VisualTreeChanged?.Invoke(sender as Element, e);
		}

		[TestCase(typeof(VerticalStackLayout))]
		[TestCase(typeof(HorizontalStackLayout))]
		[TestCase(typeof(Grid))]
		[TestCase(typeof(StackLayout))]
		public void LayoutChildren(Type TLayout)
		{
			var layout = CreateLayout(TLayout)!;

			var label = new Label();
			var button = new Button();
			layout.Children.Add(label);
			layout.Children.Add(button);

			var visualChildren = (layout as IVisualTreeElement).GetVisualChildren();

			Assert.AreEqual(layout.Children.Count, visualChildren.Count);
			Assert.AreEqual(label, visualChildren[0]);
			Assert.AreEqual(button, visualChildren[1]);
		}

		[Test]
		public async Task ModalChildren()
		{
			CreateNewApp(out _, out var window, out var page);

			var modalPage = new ContentPage();

			await window.Navigation.PushModalAsync(modalPage);

			var windowChildren = (window as IVisualTreeElement).GetVisualChildren();
			var modalParent = (modalPage as IVisualTreeElement).GetVisualParent();

			Assert.AreEqual(windowChildren.Count, 2);
			Assert.AreEqual(page, windowChildren[0]);
			Assert.AreEqual(modalPage, windowChildren[1]);
			Assert.AreEqual(window, modalParent);
		}

		[Test]
		public async Task ModalChildrenFiresDiagnosticEvents()
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			CreateNewApp(out _, out var window, out _);

			var modalPage = new ContentPage();

			await window.Navigation.PushModalAsync(modalPage);

			Assert.AreEqual(1, _treeEvents.Count);
			var (parent, args) = _treeEvents[0];

			Assert.AreEqual(window, parent);
			Assert.AreEqual(window, args.Parent);
			Assert.AreEqual(modalPage, args.Child);
			Assert.AreEqual(1, args.ChildIndex);
			Assert.AreEqual(VisualTreeChangeType.Add, args.ChangeType);

			_treeEvents.Clear();

			await window.Navigation.PopModalAsync();

			Assert.AreEqual(1, _treeEvents.Count);
			(parent, args) = _treeEvents[0];

			Assert.AreEqual(window, parent);
			Assert.AreEqual(window, args.Parent);
			Assert.AreEqual(modalPage, args.Child);
			Assert.AreEqual(1, args.ChildIndex);
			Assert.AreEqual(VisualTreeChangeType.Remove, args.ChangeType);
		}

		[Test]
		public void ApplicationChildren()
		{
			CreateNewApp(out var app, out var window, out var page);

			var appChildren = ((IVisualTreeElement)app).GetVisualChildren();
			var windowChildren = ((IVisualTreeElement)window).GetVisualChildren();
			var pageChildren = ((IVisualTreeElement)page).GetVisualChildren();

			Assert.AreEqual(1, appChildren.Count);
			Assert.AreEqual(window, appChildren[0]);
			Assert.AreEqual(1, windowChildren.Count);
			Assert.AreEqual(page, windowChildren[0]);
			Assert.AreEqual(0, pageChildren.Count);
		}

		[Test]
		public void VisualElementParent()
		{
			CreateNewApp(out var app, out var window, out var page);

			var appParent = ((IVisualTreeElement)app).GetVisualParent();
			var windowParent = ((IVisualTreeElement)window).GetVisualParent();
			var pageParent = ((IVisualTreeElement)page).GetVisualParent();

			Assert.IsNull(appParent);
			Assert.AreEqual(app, windowParent);
			Assert.AreEqual(window, pageParent);
		}

		[Test]
		public async Task AddPageContentFiresVisualTreeChanged()
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			CreateNewApp(out _, out _, out var page);
			var button = new Button();

			page.Content = button;

			Assert.AreEqual(1, _treeEvents.Count);

			var (parent, args) = _treeEvents[0];
			Assert.AreEqual(page, parent);
			Assert.AreEqual(page, args.Parent);
			Assert.AreEqual(button, args.Child);
			Assert.AreEqual(0, args.ChildIndex);
			Assert.AreEqual(VisualTreeChangeType.Add, args.ChangeType);
		}

		[Test]
		public async Task RemovePageContentFiresVisualTreeChanged()
		{
			if (!DebuggerHelper.DebuggerIsAttached)
				return;

			CreateNewApp(out _, out _, out var page);
			var button = new Button();
			page.Content = button;
			_treeEvents.Clear();

			page.Content = null;

			Assert.AreEqual(1, _treeEvents.Count);

			var (parent, args) = _treeEvents[0];
			Assert.AreEqual(page, parent);
			Assert.AreEqual(page, args.Parent);
			Assert.AreEqual(button, args.Child);
			Assert.AreEqual(0, args.ChildIndex);
			Assert.AreEqual(VisualTreeChangeType.Remove, args.ChangeType);
		}

		[TestCase(typeof(VerticalStackLayout))]
		[TestCase(typeof(HorizontalStackLayout))]
		[TestCase(typeof(Grid))]
		[TestCase(typeof(StackLayout))]
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

				Assert.AreEqual(i + 1, _treeEvents.Count);

				var (parent, args) = _treeEvents[i];
				Assert.AreEqual(layout, parent);
				Assert.AreEqual(layout, args.Parent);
				Assert.AreEqual(button, args.Child);
				Assert.AreEqual(i, args.ChildIndex);
				Assert.AreEqual(VisualTreeChangeType.Add, args.ChangeType);
			}
		}

		[TestCase(typeof(VerticalStackLayout))]
		[TestCase(typeof(HorizontalStackLayout))]
		[TestCase(typeof(Grid))]
		[TestCase(typeof(StackLayout))]
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

			Assert.AreEqual(1, _treeEvents.Count);

			var (parent, args) = _treeEvents[0];
			Assert.AreEqual(layout, parent);
			Assert.AreEqual(layout, args.Parent);
			Assert.AreEqual(button, args.Child);
			Assert.AreEqual(2, args.ChildIndex);
			Assert.AreEqual(VisualTreeChangeType.Add, args.ChangeType);
		}

		[TestCase(typeof(VerticalStackLayout))]
		[TestCase(typeof(HorizontalStackLayout))]
		[TestCase(typeof(Grid))]
		[TestCase(typeof(StackLayout))]
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

			Assert.AreEqual(1, _treeEvents.Count);

			var (parent, args) = _treeEvents[0];
			Assert.AreEqual(layout, parent);
			Assert.AreEqual(layout, args.Parent);
			Assert.AreEqual(button, args.Child);
			Assert.AreEqual(0, args.ChildIndex);
			Assert.AreEqual(VisualTreeChangeType.Remove, args.ChangeType);
		}

		[TestCase(typeof(VerticalStackLayout))]
		[TestCase(typeof(HorizontalStackLayout))]
		[TestCase(typeof(Grid))]
		[TestCase(typeof(StackLayout))]
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

			Assert.AreEqual(3, _treeEvents.Count);

			for (var i = _treeEvents.Count - 1; i >= 0; i--)
			{
				var (parent, args) = _treeEvents[_treeEvents.Count - 1 - i];

				Assert.AreEqual(layout, parent);
				Assert.AreEqual(layout, args.Parent);
				Assert.AreEqual(buttons[i], args.Child);
				Assert.AreEqual(i, args.ChildIndex);
				Assert.AreEqual(VisualTreeChangeType.Remove, args.ChangeType);
			}
		}

		[Test]
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
			Assert.IsInstanceOf(typeof(TableRoot), tableRootElement[0]);
			var tableRoot = (TableRoot)tableRootElement[0];
			var tableSectionElement = (tableRoot[0] as IVisualTreeElement).GetVisualChildren();
			Assert.AreEqual(tableSectionElement.Count, 2);
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
