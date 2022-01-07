using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	public interface ISwipeItem
	{
		bool IsVisible { get; }
		bool IsEnabled { get; }

		//bool IsVisible { get; set; }
		//ICommand Command { get; set; }
		//object CommandParameter { get; set; }

		//event EventHandler<EventArgs> Invoked;
		void OnInvoked();
	}


	public interface ISwipeItems : IList<ISwipeItem>
	{
		public SwipeMode Mode { get; }

		public SwipeBehaviorOnInvoked SwipeBehaviorOnInvoked { get; }
	}

	public interface ISwipeItemView : IContentView
	{

	}
}
