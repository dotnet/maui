#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
	/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class ExpressionSearch
	{
		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public static IExpressionSearch Default { get; set; }
	}
}