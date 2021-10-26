using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.GDI;
using Microsoft.Maui.Graphics.Skia;
using System.Windows;
using GraphicsTester.Scenarios;

namespace GraphicsTester.WPF.Skia
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Initialize();
		}

		public void Initialize()
		{
			Fonts.Register(new GDIFontService());
			GraphicsPlatform.RegisterGlobalService(SkiaGraphicsService.Instance);
			GraphicsView.BackgroundColor = Colors.White;

			foreach (var scenario in ScenarioList.Scenarios)
			{
				List.Items.Add(scenario);
			}
			List.SelectionChanged += OnSelectionChanged;
			List.SelectedIndex = 0;

			this.SizeChanged += (source, args) => Draw();
		}

		public IDrawable Drawable
		{
			set
			{
				GraphicsView.Drawable = value;
				Draw();
			}
		}

		private void Draw()
		{
			GraphicsView.Invalidate();
		}

		private void OnSelectionChanged(object source, System.Windows.Controls.SelectionChangedEventArgs args)
		{
			AbstractScenario scenario = (AbstractScenario)List.SelectedItem;
			Drawable = scenario;
			GraphicsView.Width = scenario.Width;
			GraphicsView.Height = scenario.Height;
			this.Title = $"WPF Maui Graphics Sample: {scenario}";
			Draw();
		}
	}
}
