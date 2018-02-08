using System;
using System.ComponentModel;
#if NETSTANDARD1_0
namespace Xamarin.Forms.Internals
{
	[Flags]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public enum FileAccess
	{
		Read = 0x00000001,
		Write = 0x00000002,
		ReadWrite = Read | Write
	}
}
#endif