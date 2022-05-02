using System;
using EvasObject = ElmSharp.EvasObject;
using ERect = ElmSharp.Rect;

namespace Microsoft.Maui
{
	public interface IPlatformViewHandler : IViewHandler, IDisposable
	{
		new EvasObject? PlatformView { get; }

		new EvasObject? ContainerView { get; }

		void SetParent(IPlatformViewHandler parent);

		IPlatformViewHandler? Parent { get; }

		ERect GetPlatformContentGeometry();
	}
}