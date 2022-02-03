#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class ViewRenderer<TElement, TPlatformView> : VisualElementRenderer<TElement, TPlatformView>, IPlatformViewHandler
		where TElement : View, IView
		where TPlatformView : PlatformView
	{
		public ViewRenderer() : this(VisualElementRendererMapper, VisualElementRendererCommandMapper)
		{

		}

		internal ViewRenderer(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
		{
		}
	}
}