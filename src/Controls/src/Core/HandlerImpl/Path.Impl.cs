#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes2
{
	public class Path : IShape
    {
        public Path()
        {

        }

        public Path(string? data)
        {
            Data = data;
        }

        public string? Data { get; set; }

        public PathF PathForBounds(Graphics.Rectangle rect, float density = 1) =>
            PathBuilder.Build(Data);
    }
}