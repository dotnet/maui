#nullable disable
using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static class TypeLoader
	{
		public static Func<string, string, Type> TypeProvider { get; set; }
	}
}
