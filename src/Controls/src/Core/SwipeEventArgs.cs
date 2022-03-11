using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/BaseSwipeEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.BaseSwipeEventArgs']/Docs" />
	public abstract class BaseSwipeEventArgs : EventArgs
	{
		protected BaseSwipeEventArgs(SwipeDirection swipeDirection)
		{
			SwipeDirection = swipeDirection;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/BaseSwipeEventArgs.xml" path="//Member[@MemberName='SwipeDirection']/Docs" />
		public SwipeDirection SwipeDirection { get; set; }
	}

	public class CloseRequestedEventArgs : EventArgs
	{
		public CloseRequestedEventArgs(bool animated)
		{
			Animated = animated;
		}

		public bool Animated { get; set; }
	}

	public class OpenRequestedEventArgs : EventArgs
	{
		public OpenRequestedEventArgs(OpenSwipeItem openSwipeItem, bool animated)
		{
			OpenSwipeItem = openSwipeItem;
			Animated = animated;
		}

		public OpenSwipeItem OpenSwipeItem { get; set; }
		public bool Animated { get; set; }
	}

	/// <include file="../../docs/Microsoft.Maui.Controls/SwipeStartedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.SwipeStartedEventArgs']/Docs" />
	public class SwipeStartedEventArgs : BaseSwipeEventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeStartedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public SwipeStartedEventArgs(SwipeDirection swipeDirection) : base(swipeDirection)
		{

		}
	}

	/// <include file="../../docs/Microsoft.Maui.Controls/SwipeChangingEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.SwipeChangingEventArgs']/Docs" />
	public class SwipeChangingEventArgs : BaseSwipeEventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeChangingEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public SwipeChangingEventArgs(SwipeDirection swipeDirection, double offset) : base(swipeDirection)
		{
			Offset = offset;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeChangingEventArgs.xml" path="//Member[@MemberName='Offset']/Docs" />
		public double Offset { get; set; }
	}

	/// <include file="../../docs/Microsoft.Maui.Controls/SwipeEndedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.SwipeEndedEventArgs']/Docs" />
	public class SwipeEndedEventArgs : BaseSwipeEventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeEndedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public SwipeEndedEventArgs(SwipeDirection swipeDirection, bool isOpen) : base(swipeDirection)
		{
			IsOpen = isOpen;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipeEndedEventArgs.xml" path="//Member[@MemberName='IsOpen']/Docs" />
		public bool IsOpen { get; set; }
	}
}