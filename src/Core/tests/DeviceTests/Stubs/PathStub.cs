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

		PathF IShape.PathForBounds(Rectangle rect, float density) => PathBuilder.Build(Data);
		
	}
}
