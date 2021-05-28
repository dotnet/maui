#nullable enable
using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.HotReload
{
	public interface IHotReloadableView : IReplaceableView, IView
	{
		IReloadHandler ReloadHandler { get; set; }
		void TransferState(IView newView);
		void Reload();
	}
}
