using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Compatibility.Internals
{
	[AttributeUsage(AttributeTargets.All)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	class LinkerSafeAttribute : Attribute
	{
		public LinkerSafeAttribute()
		{
		}
	}
}