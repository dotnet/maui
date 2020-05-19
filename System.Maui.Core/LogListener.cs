using System.ComponentModel;

namespace System.Maui.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class LogListener
	{
		public abstract void Warning(string category, string message);
	}
}