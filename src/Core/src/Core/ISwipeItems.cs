using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public interface ISwipeItems : IList<ISwipeItem>
	{
		public SwipeMode Mode { get; }

		public SwipeBehaviorOnInvoked SwipeBehaviorOnInvoked { get; }
	}
}
