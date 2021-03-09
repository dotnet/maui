using Foundation;
using Microsoft.Maui.Controls.Platform.iOS;

namespace Microsoft.Maui.Controls.Compatibility.Material.iOS
{
	public class MaterialEntryRenderer : EntryRendererBase<MaterialTextField>, IMaterialEntryRenderer
	{
		protected override MaterialTextField CreateNativeControl() => new MaterialTextField(this, Element);
		protected override void SetBackgroundColor(Color color) => ApplyTheme();
		protected override void SetBackground(Brush brush) => ApplyTheme();
		protected override void UpdateColor() => Control?.UpdateTextColor(this);
		protected virtual void ApplyTheme() => Control?.ApplyTheme(this);
		protected virtual void ApplyThemeIfNeeded() => Control?.ApplyThemeIfNeeded(this);
		protected override void UpdatePlaceholder() => Control?.UpdatePlaceholder(this);
		protected override void UpdateAttributedPlaceholder(NSAttributedString nsAttributedString)
		{
			// AttributedPlaceholder doesn't currently work with Material
			// once/if it does start working it will be handled inside MaterialTextManager	
		}

		protected override void UpdateFont()
		{
			base.UpdateFont();
			Control?.ApplyTypographyScheme(Element);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			ApplyThemeIfNeeded();
		}

		Color IMaterialEntryRenderer.TextColor => Element?.TextColor ?? Color.Default;
		Color IMaterialEntryRenderer.PlaceholderColor => Element?.PlaceholderColor ?? Color.Default;
		Color IMaterialEntryRenderer.BackgroundColor => Element?.BackgroundColor ?? Color.Default;
		Brush IMaterialEntryRenderer.Background => Element?.Background ?? Brush.Default;
		string IMaterialEntryRenderer.Placeholder => Element?.Placeholder ?? string.Empty;
	}
}