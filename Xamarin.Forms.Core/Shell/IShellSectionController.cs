using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public interface IShellSectionController : IElementController
	{
		event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

		Page PresentedPage { get; }

		void AddContentInsetObserver(IShellContentInsetObserver observer);

		void AddDisplayedPageObserver(object observer, Action<Page> callback);

		Task GoToPart(List<string> parts, Dictionary<string, string> queryData);

		bool RemoveContentInsetObserver(IShellContentInsetObserver observer);

		bool RemoveDisplayedPageObserver(object observer);

		void SendInsetChanged(Thickness inset, double tabThickness);

		void SendPopped();
	}
}