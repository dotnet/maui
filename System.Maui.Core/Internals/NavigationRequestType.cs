using System.ComponentModel;

namespace System.Maui.Internals
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