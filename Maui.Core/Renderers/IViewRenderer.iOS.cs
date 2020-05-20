using System;
using System.Maui.Core.Controls;
using UIKit;

namespace System.Maui {
	public interface INativeViewRenderer : IViewRenderer {

		UIView View { get; }

	}
}
