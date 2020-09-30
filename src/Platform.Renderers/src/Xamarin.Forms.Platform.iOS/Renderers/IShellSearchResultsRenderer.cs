using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public interface IShellSearchResultsRenderer : IDisposable
	{
		UIViewController ViewController { get; }

		SearchHandler SearchHandler { get; set; }

		event EventHandler<object> ItemSelected;
	}
}