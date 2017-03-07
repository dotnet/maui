using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public class BoundsConstraint
	{
		Func<Rectangle> _measureFunc;

		BoundsConstraint()
		{
		}

		internal IEnumerable<View> RelativeTo { get; set; }

		public static BoundsConstraint FromExpression(Expression<Func<Rectangle>> expression, IEnumerable<View> parents = null)
		{
			Func<Rectangle> compiled = expression.Compile();
			var result = new BoundsConstraint
			{
				_measureFunc = compiled,
				RelativeTo = parents ?? ExpressionSearch.Default.FindObjects<View>(expression).ToArray() // make sure we have our own copy
			};

			return result;
		}

		internal Rectangle Compute()
		{
			return _measureFunc();
		}
	}
}