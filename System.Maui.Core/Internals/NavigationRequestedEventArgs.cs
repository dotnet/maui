using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace System.Maui.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NavigationRequestedEventArgs : NavigationEventArgs
	{
		public NavigationRequestedEventArgs(Page page, bool animated) : base(page)
		{
			Animated = animated;
		}

		public NavigationRequestedEventArgs(Page page, Page before, bool animated) : this(page, animated)
		{
			BeforePage = before;
		}

		[Obsolete("This constructor is obsolete as of 3.5.0. Please use NavigationRequestedEventArgs(Page page, " +
			"bool animated) instead.")]
		public NavigationRequestedEventArgs(Page page, bool animated, bool realize = true) : this(page, animated)
		{
		}

		public bool Animated { get; set; }

		public Page BeforePage { get; set; }

		public Task<bool> Task { get; set; }

		public NavigationRequestType RequestType { get; set; } = NavigationRequestType.Unknown;

		[Obsolete("This property is obsolete as of 3.5.0.")]
		public bool Realize { get; set; }
	}
}