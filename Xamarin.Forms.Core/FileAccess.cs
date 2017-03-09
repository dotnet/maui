using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public enum FileAccess
	{
		Read = 0x00000001,
		Write = 0x00000002,
		ReadWrite = Read | Write
	}
}