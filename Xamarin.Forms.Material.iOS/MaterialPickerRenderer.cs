using System.ComponentModel;
using Foundation;
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

		string IMaterialEntryRenderer.Placeholder => Element?.Title;
		Color IMaterialEntryRenderer.PlaceholderColor => Element?.TitleColor ?? Color.Default;
		Color IMaterialEntryRenderer.TextColor => Element?.TextColor ?? Color.Default;
		Color IMaterialEntryRenderer.BackgroundColor => Element?.BackgroundColor ?? Color.Default;
	}
}
