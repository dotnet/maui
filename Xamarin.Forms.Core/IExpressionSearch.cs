using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IExpressionSearch
	{
		List<T> FindObjects<T>(Expression expression) where T : class;
	}
}