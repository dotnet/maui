using System;
using System.ComponentModel;
#if NETSTANDARD1_0
namespace Xamarin.Forms.Internals
{
	[Flags]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public enum FileMode
	{
		CreateNew = 1,
		Create = 2,
		Open = 3,
		OpenOrCreate = 4,
		Truncate = 5,
		Append = 6
	}
}
#endif