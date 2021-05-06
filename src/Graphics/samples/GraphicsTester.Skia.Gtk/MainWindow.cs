// This is free and unencumbered software released into the public domain.
// Happy coding!!! - GtkSharp Team

using Gtk;
using System;
using System.Collections.Generic;
using System.IO;
using GraphicsTester.Scenarios;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Skia;

namespace Samples
{
	class MainWindow : Window
	{
		private TreeView _treeView;
		private TreeStore _store;
		private Dictionary<string, AbstractScenario> _items;
		private GtkSkiaGraphicsView _skiaGraphicsView;

		public MainWindow() : base(WindowType.Toplevel)
		{
			// Setup GUI
			WindowPosition = WindowPosition.Center;
			DefaultSize = new Gdk.Size(800, 600);

			var headerBar = new HeaderBar
			{
				ShowCloseButton = true,
				Title = $"{nameof(GtkSkiaGraphicsView)} Sample Application"
			};

			var btnClickMe = new Button
			{
				AlwaysShowImage = true,
				Image = Image.NewFromIconName("document-new-symbolic", IconSize.Button)
			};
			headerBar.PackStart(btnClickMe);

			Titlebar = headerBar;

			var hpanned = new HPaned
			{
				Position = 300
			};

			_treeView = new TreeView
			{
				HeadersVisible = false
			};
			var scroll0 = new ScrolledWindow
			{
				Child = _treeView
			};
			hpanned.Pack1(scroll0, true, true);

			Fonts.Register(new SkiaFontService("", ""));
			GraphicsPlatform.RegisterGlobalService(SkiaGraphicsService.Instance);

			_skiaGraphicsView = new GtkSkiaGraphicsView();

			var skiaGraphicsRenderer = new GtkSkiaDirectRenderer
			{
				BackgroundColor = Colors.White
			};

			_skiaGraphicsView.Renderer = skiaGraphicsRenderer;

			var scroll1 = new ScrolledWindow
			{
				Child = _skiaGraphicsView
			};

			hpanned.Pack2(scroll1, true, true);

			Child = hpanned;

			// Fill up data
			FillUpTreeView();

			// Connect events
			_treeView.Selection.Changed += Selection_Changed;
			Destroyed += (sender, e) => this.Close();

			var scenario = ScenarioList.Scenarios[0];
			_skiaGraphicsView.Drawable = scenario;
		}

		private void Selection_Changed(object sender, EventArgs e)
		{
			if (_treeView.Selection.GetSelected(out TreeIter iter))
			{
				var s = _store.GetValue(iter, 0).ToString();

				if (_items.TryGetValue(s, out var scenario))
				{
					_skiaGraphicsView.Drawable = scenario;
					_skiaGraphicsView.HeightRequest = (int) scenario.Height;
					_skiaGraphicsView.WidthRequest = (int) scenario.Width;
				}

			}
		}

		private void FillUpTreeView()
		{
			// Init cells
			var cellName = new CellRendererText();

			// Init columns
			var columeSections = new TreeViewColumn
			{
				Title = "Sections"
			};
			columeSections.PackStart(cellName, true);

			columeSections.AddAttribute(cellName, "text", 0);

			_treeView.AppendColumn(columeSections);

			// Init treeview
			_store = new TreeStore(typeof(string));
			_treeView.Model = _store;
			_items = new Dictionary<string, AbstractScenario>();

			foreach (var scenario in ScenarioList.Scenarios)
			{
				_store.AppendValues(scenario.ToString());
				_items[scenario.ToString()] = scenario;

			}

			_treeView.ExpandAll();
		}

	}

}
