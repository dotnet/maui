using System.Collections.Generic;
using System.Linq.Expressions;

namespace Xamarin.Forms.Internals
{
	public interface IExpressionSearch
	{
		List<T> FindObjects<T>(Expression expression) where T : class;
	}
}