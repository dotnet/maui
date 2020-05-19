using UIKit;
using MaterialComponents;
using System;
using Foundation;
using System.Maui.Platform.iOS;

namespace System.Maui.Material.iOS
{
	public class MaterialEditorRenderer : EditorRendererBase<MaterialMultilineTextField>, IMaterialEntryRenderer
	{
		bool _hackHasRan = false;

		protected override MaterialMultilineTextField CreateNativeControl() => new MaterialMultilineTextField(this, Element);
		protected IntrinsicHeightTextView IntrinsicHeightTextView => (IntrinsicHeightTextView)TextView;
		protected override UITextView TextView => Control?.TextView;
		protected override void SetBackgroundColor(Color color) => ApplyTheme();

		protected internal override void UpdateTextColor() => Control?.UpdateTextColor(this);
		protected virtual void ApplyTheme() => Control?.ApplyTheme(this);

		protected internal override void UpdateFont()
		{
			base.UpdateFont();
			Control?.ApplyTypographyScheme(Element);
		}	

		protected internal override void UpdatePlaceholderText()
		{
			if (Control == null || !_hackHasRan)
				return;

			Control.UpdatePlaceholder(this);
		}

		protected internal override void UpdatePlaceholderColor()
		{
			if (Control == null || !_hackHasRan)
				return;

			Control.UpdatePlaceholder(this);
		}

		protected internal override void UpdateCharacterSpacing()
		{
			Control.AttributedText = Control.AttributedText.AddCharacterSpacing(Element.Text, Element.CharacterSpacing);
		}

		protected internal override void UpdateText()
		{
			if (!_hackHasRan)
				return;

			base.UpdateText();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			base.OnElementChanged(e);

			if(e.NewElement != null)
				InitialPlaceholderSetupHack();
		}

		protected internal override void UpdateAutoSizeOption()
		{
			base.UpdateAutoSizeOption();
			Control.AutoSizeWithChanges = Element.AutoSize == EditorAutoSizeOption.TextChanges;

			if(!Control.ExpandsOnOverflow)
				Control.ExpandsOnOverflow = Element.AutoSize == EditorAutoSizeOption.TextChanges;
		}

		// this is required to force the placeholder to size correctly if it starts out prefilled
		void InitialPlaceholderSetupHack()
		{
			if (Element == null)
				return;

			if(String.IsNullOrWhiteSpace(Element.Text) || String.IsNullOrWhiteSpace(Element.Placeholder))
			{
				_hackHasRan = true;
				UpdateText();
				UpdatePlaceholderText();
				return;
			}

			TextView.BecomeFirstResponder();
			Control.UpdatePlaceholder(this);
			Device.BeginInvokeOnMainThread(() =>
			{
				_hackHasRan = true;
				UpdateText();
				TextView.ResignFirstResponder();
			});
		}

		string IMaterialEntryRenderer.Placeholder => Element?.Placeholder;
		Color IMaterialEntryRenderer.TextColor => Element?.TextColor ?? Color.Default;
		Color IMaterialEntryRenderer.PlaceholderColor => Element?.PlaceholderColor ?? Color.Default;
		Color IMaterialEntryRenderer.BackgroundColor => Element?.BackgroundColor ?? Color.Default;
	}
}
