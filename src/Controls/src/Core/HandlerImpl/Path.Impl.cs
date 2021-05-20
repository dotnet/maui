#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes2
{
	public class Path : IPath
	{
		public Path()
		{

		}

		public Path(string? data)
		{
			Data = data;
		}

		public string? Data { get; set; }
	}
}
