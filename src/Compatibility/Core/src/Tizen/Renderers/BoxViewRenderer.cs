using Microsoft.Maui.Controls.Platform;
using Tizen.UIExtensions.NUI;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class BoxViewRenderer : ViewRenderer<BoxView, NView>
	{
		public BoxViewRenderer()
		{
			RegisterPropertyHandler(nameof(Element.CornerRadius), OnRadiusUpdate);
			RegisterPropertyHandler(nameof(Element.Color), UpdateColor);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			if (Control == null)
			{
				SetNativeControl(new NView());
			}
			base.OnElementChanged(e);
		}
		void OnRadiusUpdate()
		{
			int topLeft = Forms.ConvertToScaledPixel(Element.CornerRadius.TopLeft);
			Control.CornerRadius = topLeft;
		}

		void UpdateColor(bool init)
		{
			if (init && Element.Color.IsDefault())
				return;

			Control.UpdateBackgroundColor(!Element.Color.IsDefault() ? Element.Color.ToNative() : Element.BackgroundColor.ToNative());
		}
	}
}