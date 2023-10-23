#nullable enable
using PlatformView = Gtk.Widget;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class ViewRenderer : ViewRenderer<View, PlatformView>
	{
		protected ViewRenderer() : base()
		{
		}
	}

	public abstract partial class ViewRenderer<TElement, TPlatformView> : VisualElementRenderer<TElement>, IPlatformViewHandler
		where TElement : View, IView
		where TPlatformView : PlatformView
	{
#pragma warning disable CS0649
		TPlatformView? _nativeView;
#pragma warning restore CS0649

		public TPlatformView? Control
		{
			get
			{
				var value = ((IElementHandler)this).PlatformView as TPlatformView;
				if (value != this && value != null)
					return value;

				return _nativeView;
			}
		}

		object? IElementHandler.PlatformView => (_nativeView as object) ?? this;

		public ViewRenderer() : this(VisualElementRendererMapper, VisualElementRendererCommandMapper)
		{

		}

		internal ViewRenderer(IPropertyMapper mapper, CommandMapper? commandMapper = null)
			: base(mapper, commandMapper)
		{
		}

		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return
				new SizeRequest(this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint),
				MinimumSize());
		}

		protected virtual TPlatformView CreateNativeControl()
		{
			return default(TPlatformView)!;
		}

	}
}