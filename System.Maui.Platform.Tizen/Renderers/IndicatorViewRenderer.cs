using System.Collections;

namespace System.Maui.Platform.Tizen
{
	public class IndicatorViewRenderer : ViewRenderer<IndicatorView, Native.IndicatorView>
	{
		public IndicatorViewRenderer()
		{
			RegisterPropertyHandler(IndicatorView.CountProperty, UpdateItemsSource);
			RegisterPropertyHandler(IndicatorView.PositionProperty, UpdatePosition);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<IndicatorView> e)
		{
			if (Control == null)
			{
				SetNativeControl(new Native.IndicatorView(System.Maui.Maui.NativeParent));
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

		void UpdateItemsSource()
		{
			Control.ClearIndex();
			int count = 0;
			if (Element.ItemsSource is ICollection collection)
			{
				count = collection.Count;
			}
			else
			{
				var enumerator = Element.ItemsSource.GetEnumerator();
				while (enumerator?.MoveNext() ?? false)
				{
					count++;
				}
			}
			Control.AppendIndex(count);
		}

		void UpdatePosition()
		{
			Control.UpdateSelectedIndex(Element.Position);
		}
	}
}
