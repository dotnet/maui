using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IExpressionSearch
	{
		List<T> FindObjects<T>(Expression expression) where T : class;
	}
}