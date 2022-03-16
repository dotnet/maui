using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Platform
{
	public static partial class WindowExtensions
	{
		public static async Task<RenderedView?> RenderAsImage(this IWindow window, RenderType type)
		{
			await Task.Delay(5);
			return null;
		}
	}
}
