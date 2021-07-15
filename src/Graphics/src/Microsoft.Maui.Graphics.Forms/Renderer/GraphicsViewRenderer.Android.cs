using System.ComponentModel;
using Microsoft.Maui.Graphics.Forms.Android;
using Microsoft.Maui.Graphics.Native;
using Android.Content;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Microsoft.Maui.Graphics.Forms.GraphicsView), typeof(GraphicsViewRenderer))]
namespace Microsoft.Maui.Graphics.Forms.Android
{
	[Preserve]
	public class GraphicsViewRenderer : ViewRenderer<Microsoft.Maui.Graphics.Forms.GraphicsView, NativeGraphicsView>
	{
		public GraphicsViewRenderer(Context context) : base(context)
		{

		}

		protected override void OnElementChanged(ElementChangedEventArgs<GraphicsView> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				// Unsubscribe from event handlers and cleanup any resources
				SetNativeControl(null);
			}

			if (e.NewElement != null)
			{
				SetNativeControl(new NativeGraphicsView(Context));
				UpdateDrawable();
			}
		}

		protected override void OnElementPropertyChanged(
			object sender,
			PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == nameof(GraphicsView.Drawable))
				UpdateDrawable();
		}

		private void UpdateDrawable()
		{
			Control.Drawable = Element.Drawable;
		}
	}
}
