// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;
using System;
using System.Collections.Generic;
using System.IO;
using GraphicsTester.Scenarios;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Samples {

	class MainWindow : Window {

		private TreeView _treeView;
		private TreeStore _store;
		private Dictionary<string, AbstractScenario> _items;
		private GtkGraphicsView _gtkGtkGraphicsView;

		public MainWindow() : base(WindowType.Toplevel) {
			// Setup GUI
			WindowPosition = WindowPosition.Center;
			DefaultSize = new Gdk.Size(800, 600);

			var headerBar = new HeaderBar {
				ShowCloseButton = true,
				Title = $"{typeof(Point).Namespace} Gtk Sample Application"
			};

			var btnClickMe = new Button {
				AlwaysShowImage = true,
				Image = Image.NewFromIconName("document-new-symbolic", IconSize.Button)
			};

			headerBar.PackStart(btnClickMe);

			Titlebar = headerBar;

			var hpanned = new HPaned {
				Position = 300
			};

			_treeView = new TreeView {
				HeadersVisible = false
			};

			var scroll0 = new ScrolledWindow {
				Child = _treeView
			};

			hpanned.Pack1(scroll0, true, true);

			_gtkGtkGraphicsView = new GtkGraphicsView {
				BackgroundColor = Colors.White
			};

			var scroll1 = new ScrolledWindow {
				Child = _gtkGtkGraphicsView
			};

			hpanned.Pack2(scroll1, true, true);

			Child = hpanned;

			// Fill up data
			FillUpTreeView();

			// Connect events
			_treeView.Selection.Changed += Selection_Changed;
			Destroyed += (sender, e) => this.Close();

			var scenario = ScenarioList.Scenarios[0];
			_gtkGtkGraphicsView.Drawable = scenario;
		}

		private void Selection_Changed(object sender, EventArgs e) {
			if (!_treeView.Selection.GetSelected(out TreeIter iter)) return;

			var s = _store.GetValue(iter, 0).ToString();

			if (!_items.TryGetValue(s, out var scenario)) return;

			_gtkGtkGraphicsView.Drawable = scenario;
			_gtkGtkGraphicsView.HeightRequest = (int) scenario.Height;
			_gtkGtkGraphicsView.WidthRequest = (int) scenario.Width;
		}

		private void FillUpTreeView() {
			// Init cells
			var cellName = new CellRendererText();

			// Init columns
			var columeSections = new TreeViewColumn {
				Title = "Sections"
			};

			columeSections.PackStart(cellName, true);

			columeSections.AddAttribute(cellName, "text", 0);

			_treeView.AppendColumn(columeSections);

			// Init treeview
			_store = new TreeStore(typeof(string));
			_treeView.Model = _store;
			_items = new Dictionary<string, AbstractScenario>();

			foreach (var scenario in ScenarioList.Scenarios) {
				_store.AppendValues(scenario.ToString());
				_items[scenario.ToString()] = scenario;

			}

			_treeView.ExpandAll();
		}

	}

}
