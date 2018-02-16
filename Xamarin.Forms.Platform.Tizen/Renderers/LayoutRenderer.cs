using ElmSharp;
using System;

namespace Xamarin.Forms.Platform.Tizen
{
	/// <summary>
	/// Renderer of a Layout.
	/// </summary>
	public class LayoutRenderer : ViewRenderer<Layout, Native.Canvas>
	{
		bool _layoutUpdatedRegistered = false;

		public void RegisterOnLayoutUpdated()
		{
			if (!_layoutUpdatedRegistered)
			{
				Control.LayoutUpdated += OnLayoutUpdated;
				_layoutUpdatedRegistered = true;
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
		{
			if (null == Control)
			{
				SetNativeControl(new Native.Canvas(Forms.NativeParent));
			}

			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_layoutUpdatedRegistered)
				{
					Control.LayoutUpdated -= OnLayoutUpdated;
					_layoutUpdatedRegistered = false;
				}
			}

			base.Dispose(disposing);
		}

		void OnLayoutUpdated(object sender, Native.LayoutEventArgs e)
		{
			Element.Layout(e.Geometry.ToDP());
		}
	}
}
