using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	public static class VisualDiagnosticsExtensions
	{
		internal static Task<byte[]?> RenderAsPng(this IView view)
		{
			return Task.FromResult<byte[]?>(null);
		}
	}
}