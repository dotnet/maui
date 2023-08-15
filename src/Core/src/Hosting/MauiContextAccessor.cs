using System;
using System.ComponentModel;

namespace Microsoft.Maui.Hosting
{
	public static class MauiContextAccessor
	{
		public static IServiceProvider Services { get; [EditorBrowsable(EditorBrowsableState.Never)]set; } = default!;

		public static IApplication Application { get; [EditorBrowsable(EditorBrowsableState.Never)]set; } = default!;
	}
}
