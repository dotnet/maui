using System.ComponentModel;
using UIKit;

namespace Xamarin.Forms.Platform.iOS.Material
{
	public class MaterialDatePickerRenderer : DatePickerRendererBase<MaterialTextField>, IMaterialEntryRenderer
	{
		public MaterialDatePickerRenderer()
		{
			VisualElement.VerifyVisualFlagEnabled();
		}

		protected override MaterialTextField CreateNativeControl()
		{
			var field = new NoCaretMaterialTextField(this, Element);
			return field;
		}

		protected override void SetBackgroundColor(Color color)
		{
			ApplyTheme();
		}

		protected internal override void UpdateFont()
		{
			base.UpdateFont();
			Control?.ApplyTypographyScheme(Element);
		}
		

		protected internal override void UpdateTextColor()
		{
			Control?.UpdateTextColor(this);
		}


		protected virtual void ApplyTheme()
		{
			Control?.ApplyTheme(this);
		}

		internal void UpdatePlaceholder()
		{
			Control?.UpdatePlaceholder(this);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
		{
			base.OnElementChanged(e);
			UpdatePlaceholder();
		}

		string IMaterialEntryRenderer.Placeholder => string.Empty;
		Color IMaterialEntryRenderer.PlaceholderColor => Color.Default;

		Color IMaterialEntryRenderer.TextColor => Element?.TextColor ?? Color.Default;
		Color IMaterialEntryRenderer.BackgroundColor => Element?.BackgroundColor ?? Color.Default;
	}
}