using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class LogListener
	{
		public abstract void Warning(string category, string message);
	}
}