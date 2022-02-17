using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellSearchResultsRenderer : IDisposable
	{
		UIViewController ViewController { get; }

		SearchHandler SearchHandler { get; set; }

		event EventHandler<object> ItemSelected;
	}
}