#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class PathStub : IShape
    {
        public PathStub()
        {

        }

        public PathStub(string? data)
        {
            Data = data;
        }

        public string? Data { get; set; }

        public PathF PathForBounds(Graphics.Rectangle rect, float density = 1) =>
            PathBuilder.Build(Data);
    }
}