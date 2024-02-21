using System;
using System.Collections.Generic;
using System.Text;
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class ViewRenderer<TElement, TNativeView> : VisualElementRenderer<TElement, TNativeView>, IPlatformViewHandler
		where TElement : View, IView
		where TNativeView : PlatformView
	{
		public ViewRenderer() : this(VisualElementRendererMapper, VisualElementRendererCommandMapper)
		{

		}

		protected ViewRenderer(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
		{
		}
	}
}