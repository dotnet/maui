using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class ExpressionSearch
	{
		public static IExpressionSearch Default { get; set; }
	}
}