using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	// TODO Rename to IMenuElement? or IMenuView? right now it conflicts with Android
	public interface IMenuElement
	{
		/// <summary>
		/// Gets the paint which will fill the background of a View.
		/// </summary>
		Paint? Background { get; }

		string? Text { get; }

		bool IsEnabled { get; }
	}

	public interface ISwipeItemMenuItem : IMenuElement, ISwipeItem
	{
	}

	public interface ISwipeItem
	{
		bool IsVisible { get; }

		//bool IsVisible { get; set; }
		//ICommand Command { get; set; }
		//object CommandParameter { get; set; }

		//event EventHandler<EventArgs> Invoked;
		void OnInvoked();

		string AutomationId { get; }
	}


	public interface ISwipeItems : IList<ISwipeItem>
	{
		public SwipeMode Mode { get; }

		public SwipeBehaviorOnInvoked SwipeBehaviorOnInvoked { get; }
	}

	public interface ISwipeItemView : IContentView, ISwipeItem
	{
	}
}
