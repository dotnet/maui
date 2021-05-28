#nullable enable
using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.HotReload
{
	public interface IReloadHandler
	{
		void Reload();
	}
}
