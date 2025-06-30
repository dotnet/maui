#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>For internal use by the Microsoft.Maui.Controls platform.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class ExpressionSearch
	{
		/// <summary>For internal use by the Microsoft.Maui.Controls platform.</summary>
		public static IExpressionSearch Default { get; set; }
	}
}