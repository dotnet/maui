using System;
using System.ComponentModel;
#if NETSTANDARD1_0
namespace Xamarin.Forms.Internals
{
	[Flags]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public enum FileShare
	{
		None = 0,
		Read = 1,
		Write = 2,
		ReadWrite = 3,
		Delete = 4,
		Inheritable = 16
	}
}
#endif