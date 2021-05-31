using System;
using EvasObject = ElmSharp.EvasObject;
using ERect = ElmSharp.Rect;

namespace Microsoft.Maui
{
	public interface INativeViewHandler : IViewHandler, IDisposable
	{
		new EvasObject? NativeView { get; }

		new EvasObject? ContainerView { get; }

		void SetParent(INativeViewHandler parent);

		INativeViewHandler? Parent { get; }

		ERect GetNativeContentGeometry();
	}
}