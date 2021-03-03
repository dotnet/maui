using CoreGraphics;
using Microsoft.Maui.Controls.Platform.iOS;

namespace Microsoft.Maui.Controls.Compatibility.Material.iOS
{
	public class MaterialDatePickerRenderer : DatePickerRendererBase<MaterialTextField>, IMaterialEntryRenderer
	{
		protected override MaterialTextField CreateNativeControl() => new NoCaretMaterialTextField(this, Element);
		protected override void SetBackgroundColor(Color color) => ApplyTheme();
		protected override void SetBackground(Brush brush) => ApplyTheme();
		protected internal override void UpdateTextColor() => Control?.UpdateTextColor(this);
		protected virtual void ApplyTheme() => Control?.ApplyTheme(this);
		protected virtual void ApplyThemeIfNeeded() => Control?.ApplyThemeIfNeeded(this);

		protected internal override void UpdateFont()
		{
			base.UpdateFont();
			Control?.ApplyTypographyScheme(Element);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
		{
			base.OnElementChanged(e);

			if(e.NewElement != null)
				UpdatePlaceholder();
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			ApplyThemeIfNeeded();
		}

		void UpdatePlaceholder() => Control?.UpdatePlaceholder(this);
		string IMaterialEntryRenderer.Placeholder => string.Empty;
		Color IMaterialEntryRenderer.PlaceholderColor => Color.Default;
		Color IMaterialEntryRenderer.TextColor => Element?.TextColor ?? Color.Default;
		Color IMaterialEntryRenderer.BackgroundColor => Element?.BackgroundColor ?? Color.Default;
		Brush IMaterialEntryRenderer.Background => Element?.Background ?? Brush.Default;
	}
}