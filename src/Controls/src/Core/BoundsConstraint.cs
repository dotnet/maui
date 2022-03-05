using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/BoundsConstraint.xml" path="Type[@FullName='Microsoft.Maui.Controls.BoundsConstraint']/Docs" />
	public class BoundsConstraint
	{
		Func<Rect> _measureFunc;

		BoundsConstraint()
		{
		}

		internal bool CreatedFromExpression { get; set; }
		internal IEnumerable<View> RelativeTo { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/BoundsConstraint.xml" path="//Member[@MemberName='FromExpression']/Docs" />
		public static BoundsConstraint FromExpression(Expression<Func<Rect>> expression, IEnumerable<View> parents = null)
		{
			return FromExpression(expression, false, parents);
		}

		internal static BoundsConstraint FromExpression(Expression<Func<Rect>> expression, bool fromExpression, IEnumerable<View> parents = null)
		{
			Func<Rect> compiled = expression.Compile();
			var result = new BoundsConstraint
			{
				_measureFunc = compiled,
				RelativeTo = parents ?? ExpressionSearch.Default.FindObjects<View>(expression).ToArray(), // make sure we have our own copy
				CreatedFromExpression = fromExpression
			};

			return result;
		}

		internal Rect Compute()
		{
			return _measureFunc();
		}
	}
}