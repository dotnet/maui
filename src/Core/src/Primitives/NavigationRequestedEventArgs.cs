using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	internal class MauiNavigationEventArgs : EventArgs
	{
		public MauiNavigationEventArgs(IView page)
		{
			if (page == null)
				throw new ArgumentNullException("page");

			Page = page;
		}

		public IView Page { get; }
	}

	internal class MauiNavigationRequestedEventArgs : MauiNavigationEventArgs
	{
		public MauiNavigationRequestedEventArgs(IView page, bool animated) : base(page)
		{
			Animated = animated;
		}

		public MauiNavigationRequestedEventArgs(IView page, IView before, bool animated) : this(page, animated)
		{
			BeforePage = before;
		}

		public bool Animated { get; set; }

		public IView? BeforePage { get; set; }

		public Task<bool>? Task { get; set; }
	}
}