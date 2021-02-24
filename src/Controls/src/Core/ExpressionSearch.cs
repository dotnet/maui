using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class ExpressionSearch
	{
		public static IExpressionSearch Default { get; set; }
	}
}