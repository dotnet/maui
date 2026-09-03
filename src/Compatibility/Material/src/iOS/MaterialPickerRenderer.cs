using Foundation;
using Microsoft.Maui.Controls.Platform.iOS;

namespace Microsoft.Maui.Controls.Compatibility.Material.iOS
{
	public class MaterialPickerRenderer : PickerRendererBase<MaterialTextField>, IMaterialEntryRenderer
	{
		protected override MaterialTextField CreateNativeControl() => new ReadOnlyMaterialTextField(this, Element);
		protected override void SetBackgroundColor(Color color) => ApplyTheme();
		protected override void SetBackground(Brush brush) => ApplyTheme();

		protected internal override void UpdateFont()
		{
			base.UpdateFont();
			Control?.ApplyTypographyScheme(Element);
		}

		protected internal override void UpdateTextColor() => Control?.UpdateTextColor(this);
		protected virtual void ApplyTheme() => Control?.ApplyTheme(this);
		protected virtual void ApplyThemeIfNeeded() => Control?.ApplyThemeIfNeeded(this);
		protected internal override void UpdatePlaceholder() => Control?.UpdatePlaceholder(this);

		protected override void UpdateAttributedPlaceholder(NSAttributedString nSAttributedString)
		{
			// AttributedPlaceholder doesn't currently work with Material
			// once/if it does start working it will be handled inside MaterialTextManager	
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				UpdatePlaceholder();
				UpdateCharacterSpacing();
			}
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			ApplyThemeIfNeeded();
		}

		string IMaterialEntryRenderer.Placeholder => Element?.Title;
		Color IMaterialEntryRenderer.PlaceholderColor => Element?.TitleColor ?? Color.Default;
		Color IMaterialEntryRenderer.TextColor => Element?.TextColor ?? Color.Default;
		Color IMaterialEntryRenderer.BackgroundColor => Element?.BackgroundColor ?? Color.Default;
		Brush IMaterialEntryRenderer.Background => Element?.Background ?? Brush.Default;
	}
}