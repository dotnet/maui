using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
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