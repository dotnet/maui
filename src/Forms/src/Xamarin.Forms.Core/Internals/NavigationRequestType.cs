using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public enum NavigationRequestType
	{
		Unknown = 0,
		Push,
		Pop,
		PopToRoot,
		Insert,
		Remove,
	}
}