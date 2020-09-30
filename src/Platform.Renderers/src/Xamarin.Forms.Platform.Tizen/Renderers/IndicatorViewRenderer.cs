namespace Xamarin.Forms.Platform.Tizen
{
	public class IndicatorViewRenderer : ViewRenderer<IndicatorView, Native.IndicatorView>
	{
		public IndicatorViewRenderer()
		{
			RegisterPropertyHandler(IndicatorView.CountProperty, UpdateCount);
			RegisterPropertyHandler(IndicatorView.PositionProperty, UpdatePosition);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<IndicatorView> e)
		{
			if (Control == null)
			{
				SetNativeControl(new Native.IndicatorView(Forms.NativeParent));
			}
			if (e.NewElement != null)
			{
				Control.SelectedPosition += OnSelectedPosition;
			}
			if (e.OldElement != null)
			{
				Control.SelectedPosition -= OnSelectedPosition;
			}
			base.OnElementChanged(e);
		}

		void OnSelectedPosition(object sender, SelectedPositionChangedEventArgs e)
		{
			Element.SetValueFromRenderer(IndicatorView.PositionProperty, (int)e.SelectedPosition);
		}

		void UpdateCount()
		{
			Control.ClearIndex();
			Control.AppendIndex(Element.Count);
			Control.Update(0);
			UpdatePosition();
		}

		void UpdatePosition()
		{
			Control.UpdateSelectedIndex(Element.Position);
		}
	}
}
