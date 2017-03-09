using System.ComponentModel;
using System.Threading.Tasks;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NavigationRequestedEventArgs : NavigationEventArgs
	{
		public NavigationRequestedEventArgs(Page page, bool animated, bool realize = true) : base(page)
		{
			Animated = animated;
			Realize = realize;
		}

		public NavigationRequestedEventArgs(Page page, Page before, bool animated) : this(page, animated)
		{
			BeforePage = before;
		}

		public bool Animated { get; set; }

		public Page BeforePage { get; set; }

		public bool Realize { get; set; }

		public Task<bool> Task { get; set; }
	}
}