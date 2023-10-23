#nullable enable
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{

	public abstract partial class VisualElementRenderer<TElement> : Gtk.Widget, IPlatformViewHandler, IElementHandler
		where TElement : Element, IView
	{

		object? IElementHandler.PlatformView => this;

	}

}