using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Platform
{
	public static partial class WindowExtensions
	{
		public static Task<RenderedView?> RenderAsImage(this IWindow window, RenderType type)
		{
			return Task.FromResult<RenderedView?>(null);
		}
	}
}
