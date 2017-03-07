using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	public abstract class ExpressionSearch
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IExpressionSearch Default { get; set; }
	}
}