#nullable enable
using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.HotReload
{
	// TODO ezhart This interface name is probably wrong because they want it to work for page, and page is not a view
	// (or FrameworkElement should be called View, and then it's fine)

	public interface IHotReloadableView : IReplaceableView, IFrameworkElement
	{
		IReloadHandler ReloadHandler { get; set; }
		void TransferState(IFrameworkElement newView);
		void Reload();
	}
}
