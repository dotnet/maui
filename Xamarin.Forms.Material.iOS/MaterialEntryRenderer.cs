using UIKit;

namespace Xamarin.Forms.Platform.iOS.Material
{
	public class MaterialEntryRenderer : EntryRendererBase<MaterialTextField>, IMaterialEntryRenderer
	{

		public MaterialEntryRenderer()
		{
			VisualElement.VerifyVisualFlagEnabled();
		}

		protected override MaterialTextField CreateNativeControl()
		{
			var field = new MaterialTextField(this, Element);
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


		protected internal override void UpdateColor()
		{
			Control?.UpdateTextColor(this);
		}


		protected virtual void ApplyTheme()
		{
			Control?.ApplyTheme(this);
		}

		protected internal override void UpdatePlaceholder()
		{
			Control?.UpdatePlaceholder(this);
			
		}


		Color IMaterialEntryRenderer.TextColor => Element?.TextColor ?? Color.Default;
		Color IMaterialEntryRenderer.PlaceholderColor => Element?.PlaceholderColor ?? Color.Default;
		Color IMaterialEntryRenderer.BackgroundColor => Element?.BackgroundColor ?? Color.Default;
		string IMaterialEntryRenderer.Placeholder => Element?.Placeholder ?? string.Empty;
	}
}