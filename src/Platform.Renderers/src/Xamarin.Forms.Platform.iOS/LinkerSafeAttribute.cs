using System;
using System.ComponentModel;

namespace Xamarin.Forms.Internals
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