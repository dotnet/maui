using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	public class BoundsConstraint
	{
		Func<Rectangle> _measureFunc;

		BoundsConstraint()
		{
		}

		internal bool CreatedFromExpression { get; set; }
		internal IEnumerable<View> RelativeTo { get; set; }

		public static BoundsConstraint FromExpression(Expression<Func<Rectangle>> expression, IEnumerable<View> parents = null)
		{
			return FromExpression(expression, false, parents);
		}

		internal static BoundsConstraint FromExpression(Expression<Func<Rectangle>> expression, bool fromExpression, IEnumerable<View> parents = null)
		{
			Func<Rectangle> compiled = expression.Compile();
			var result = new BoundsConstraint
			{
				_measureFunc = compiled,
				RelativeTo = parents ?? ExpressionSearch.Default.FindObjects<View>(expression).ToArray(), // make sure we have our own copy
				CreatedFromExpression = fromExpression
			};

			return result;
		}

		internal Rectangle Compute()
		{
			return _measureFunc();
		}
	}
}