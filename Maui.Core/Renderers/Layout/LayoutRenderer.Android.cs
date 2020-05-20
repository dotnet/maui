namespace System.Maui.Platform
{
	public partial class LayoutRenderer : AbstractViewRenderer<ILayout, LayoutViewGroup>
	{
		protected override LayoutViewGroup CreateView()
		{
			var viewGroup = new LayoutViewGroup(Context)
			{
				CrossPlatformMeasure = VirtualView.Measure,
				CrossPlatformArrange = VirtualView.Arrange
			};

			return viewGroup;
		}

		public override void SetView(IFrameworkElement view)
		{
			base.SetView(view);

			TypedNativeView.CrossPlatformMeasure = VirtualView.Measure;
			TypedNativeView.CrossPlatformArrange = VirtualView.Arrange;

			foreach (var child in VirtualView.Children)
			{
				TypedNativeView.AddView(child.ToNative(Context));
			}
		}

		protected override void DisposeView(LayoutViewGroup nativeView)
		{
			nativeView.CrossPlatformArrange = null;
			nativeView.CrossPlatformMeasure = null;

			nativeView.RemoveAllViews();

			base.DisposeView(nativeView);
		}
	}
} 
