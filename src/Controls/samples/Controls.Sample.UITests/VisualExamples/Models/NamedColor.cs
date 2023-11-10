using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.VisualExamples.Models
{
    // Used in TabbedPageDemoPage, CarouselPageDemoPage & FlyoutPageDemoPage.
    public class NamedColor
    {
        public NamedColor()
        {
        }

        public NamedColor(string name, Color color)
        {
            Name = name;
            Color = color;
        }

        public string Name { set; get; }

        public Color Color { set; get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
