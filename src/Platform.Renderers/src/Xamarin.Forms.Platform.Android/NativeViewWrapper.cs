using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public class NativeViewWrapper : View
	{
		public NativeViewWrapper(global::Android.Views.View nativeView, GetDesiredSizeDelegate getDesiredSizeDelegate = null, OnLayoutDelegate onLayoutDelegate = null,
								 OnMeasureDelegate onMeasureDelegate = null)
		{
			GetDesiredSizeDelegate = getDesiredSizeDelegate;
			NativeView = nativeView;
			OnLayoutDelegate = onLayoutDelegate;
			OnMeasureDelegate = onMeasureDelegate;

			nativeView.TransferBindablePropertiesToWrapper(this);
		}

		public GetDesiredSizeDelegate GetDesiredSizeDelegate { get; }

		public global::Android.Views.View NativeView { get; }

		public OnLayoutDelegate OnLayoutDelegate { get; }

		public OnMeasureDelegate OnMeasureDelegate { get; }

		protected override void OnBindingContextChanged()
		{
			NativeView.SetBindingContext(BindingContext, (view) => (view as ViewGroup)?.GetChildrenOfType<global::Android.Views.View>());
			base.OnBindingContextChanged();
		}
	}
}