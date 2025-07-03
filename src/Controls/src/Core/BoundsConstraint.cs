#nullable disable
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>A bounds layout constraint used by <see cref="Microsoft.Maui.Controls.Compatibility.RelativeLayout"/>s.</summary>
	public class BoundsConstraint
	{
		Func<Rect> _measureFunc;

		BoundsConstraint()
		{
		}

		internal bool CreatedFromExpression { get; set; }
		internal IEnumerable<View> RelativeTo { get; set; }

		/// <summary>Returns a <see cref="Microsoft.Maui.Controls.BoundsConstraint"/> object that contains the compiled version of <paramref name="expression"/> and is relative to either <paramref name="parents"/> or the views referred to in <paramref name="expression"/>.</summary>
		/// <param name="expression">The expression from which to compile the constraint.</param>
		/// <param name="parents">The parents to consider when compiling the constraint.</param>
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