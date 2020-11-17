using System.Graphics;
using System.Graphics.Xaml;
using System.Windows;
using GraphicsTester.Scenarios;

namespace GraphicsTester.Xaml
{
    public partial class MainWindow : Window
    {
        private readonly XamlCanvas canvas = new XamlCanvas();
        private IDrawable drawable;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        public void Initialize()
        {
            canvas.Canvas = Canvas;

            foreach (var scenario in ScenarioList.Scenarios)
            {
                List.Items.Add(scenario);
            }
            List.SelectionChanged += (source, args) => Drawable = List.SelectedItem as IDrawable;

            List.SelectedIndex = 0;

            this.SizeChanged += (source,args) => Draw();
        }

        public IDrawable Drawable
        {
            get => drawable;
            set
            {
                drawable = value;
                Draw();
            }
        }

        private void Draw()
        {
            if (drawable != null)
            {
                using (canvas.CreateSession())
                {
                    drawable.Draw(canvas, new RectangleF(0, 0, (float) Canvas.Width, (float) Canvas.Height));
                }
            }            
        }
    }
}
