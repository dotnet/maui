using System.ComponentModel;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace Xamarin.Forms.Material.iOS
{
	public class MaterialPickerRenderer : PickerRendererBase<MaterialTextField>, IMaterialEntryRenderer
	{
		protected override MaterialTextField CreateNativeControl() => new ReadOnlyMaterialTextField(this, Element);
		protected override void SetBackgroundColor(Color color) => ApplyTheme();

		protected internal override void UpdateFont()
		{
			base.UpdateFont();
			Control?.ApplyTypographyScheme(Element);
		}

		protected internal override void UpdateTextColor() => Control?.UpdateTextColor(this);
		protected virtual void ApplyTheme() => Control?.ApplyTheme(this);
		protected internal override void UpdatePlaceholder() => Control?.UpdatePlaceholder(this);

		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				UpdatePlaceholder();
				UpdateCharacterSpacing();
			}
		}

		string IMaterialEntryRenderer.Placeholder => string.Empty;
		Color IMaterialEntryRenderer.PlaceholderColor => Color.Default;
		Color IMaterialEntryRenderer.TextColor => Element?.TextColor ?? Color.Default;
		Color IMaterialEntryRenderer.BackgroundColor => Element?.BackgroundColor ?? Color.Default;
	}
}
