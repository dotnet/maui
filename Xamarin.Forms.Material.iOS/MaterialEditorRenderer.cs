using UIKit;
using MaterialComponents;
using System;

namespace Xamarin.Forms.Platform.iOS.Material
{
	public class MaterialEditorRenderer : EditorRendererBase<MaterialMultilineTextField>, IMaterialEntryRenderer
	{
		bool _hackHasRan = false;

		protected override MaterialMultilineTextField CreateNativeControl()
		{			
			return new MaterialMultilineTextField(this, Element);
		}

		protected override void SetBackgroundColor(Color color)
		{
			ApplyTheme();			
		}


		protected internal override void UpdateTextColor()
		{
			Control?.UpdateTextColor(this);
		}


		protected virtual void ApplyTheme()
		{
			Control?.ApplyTheme(this);
		}

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

		protected internal override void UpdateText()
		{
			if (!_hackHasRan)
				return;

			base.UpdateText();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
		{
			base.OnElementChanged(e);
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


		// Placeholder is currently broken upstream and doesn't animate to the correct scale
		string IMaterialEntryRenderer.Placeholder => Element?.Placeholder;
		// string IMaterialEntryRenderer.Placeholder => String.Empty;

		Color IMaterialEntryRenderer.TextColor => Element?.TextColor ?? Color.Default;
		Color IMaterialEntryRenderer.PlaceholderColor => Element?.PlaceholderColor ?? Color.Default;
		Color IMaterialEntryRenderer.BackgroundColor => Element?.BackgroundColor ?? Color.Default;

		protected IntrinsicHeightTextView IntrinsicHeightTextView => (IntrinsicHeightTextView)TextView;
		protected override UITextView TextView => Control?.TextView;

	}
}