using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.SharpDX;
using System.Windows;
using GraphicsTester.Scenarios;

namespace GraphicsTester.WPF.SharpDX
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        public void Initialize()
        {
            GraphicsPlatform.RegisterGlobalService(DXGraphicsService.Instance);
            GraphicsView.BackgroundColor = Colors.White;

            foreach (var scenario in ScenarioList.Scenarios)
            {
                List.Items.Add(scenario);
            }
            List.SelectionChanged += (source, args) => Drawable = List.SelectedItem as IDrawable;

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
    }
}
