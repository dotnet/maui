using System;
using UIKit;

namespace System.Maui.Platform.iOS
{
	public interface IShellSearchResultsRenderer : IDisposable
	{
		UIViewController ViewController { get; }

		SearchHandler SearchHandler { get; set; }

		event EventHandler<object> ItemSelected;
	}
}