using System.Collections.Generic;
using System.Linq.Expressions;

namespace Xamarin.Forms
{
	internal interface IExpressionSearch
	{
		List<T> FindObjects<T>(Expression expression) where T : class;
	}
}