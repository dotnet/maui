using System;
using ERect = ElmSharp.Rect;
using EvasObject = ElmSharp.EvasObject;

namespace Microsoft.Maui
{
	public interface IPlatformViewHandler : IViewHandler, IDisposable
	{
		new EvasObject? PlatformView { get; }

		new EvasObject? ContainerView { get; }

		void SetParent(IPlatformViewHandler parent);

		IPlatformViewHandler? Parent { get; }

		bool ForceContainer { get; set; }

		ERect GetPlatformContentGeometry();
	}
}