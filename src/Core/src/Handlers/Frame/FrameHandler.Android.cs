using Android.Graphics.Drawables;
using AndroidX.CardView.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class FrameHandler : ViewHandler<IFrame, CardView>
	{
		static float? DefaultElevation;
		static float? DefaultCornerRadius;

		static GradientDrawable? BackgroundDrawable;

		protected override CardView CreateNativeView()
		{
			var cardView = new CardView(Context);

			BackgroundDrawable = new GradientDrawable();
			BackgroundDrawable.SetShape(ShapeType.Rectangle);

			cardView.Background = BackgroundDrawable;

			return cardView;
		}

		protected override void SetupDefaults(CardView nativeView)
		{
			DefaultElevation = -1f;
			DefaultCornerRadius = -1f;
		}

		public static void MapContent(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateContent(frame, handler.MauiContext);
		}

		public static void MapBackgroundColor(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateBackgroundColor(frame, BackgroundDrawable);
		}

		public static void MapBorderColor(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateBorderColor(frame, BackgroundDrawable);
		}

		public static void MapHasShadow(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateHasShadow(frame, DefaultElevation);
		}

		public static void MapCornerRadius(FrameHandler handler, IFrame frame)
		{ 
			handler.NativeView?.UpdateCornerRadius(frame, BackgroundDrawable, DefaultCornerRadius);
		}
	}
}