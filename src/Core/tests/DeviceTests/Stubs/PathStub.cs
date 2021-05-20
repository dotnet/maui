#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class PathStub : IPath
	{
		public PathStub()
		{

		}

		public PathStub(string? data)
		{
			Data = data;
		}

		public string? Data { get; set; }
	}
}
