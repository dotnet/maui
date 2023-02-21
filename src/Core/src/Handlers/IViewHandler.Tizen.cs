using System;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui
{
	public interface IPlatformViewHandler : IViewHandler, IDisposable
	{
		new NView? PlatformView { get; }

		new NView? ContainerView { get; }
	}
}