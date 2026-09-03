using System;
using Microsoft.Maui.Controls.Platform.iOS;

namespace Microsoft.Maui.Controls.Compatibility.Material.iOS
{
	public class MaterialCheckBoxRenderer : CheckBoxRendererBase<MaterialFormsCheckBox>
	{
		protected override float MinimumSize => 48f;
		bool _disposed = false;

		protected override MaterialFormsCheckBox CreateNativeControl()
		{
			return new MaterialFormsCheckBox();
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CheckBox> e)
		{
			base.OnElementChanged(e);

			if (Control == null)
				return;

			if (e.OldElement != null)
				Control.CheckedChanged -= OnCheckedChanged;

			if (e.NewElement != null)
				Control.CheckedChanged += OnCheckedChanged;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;
			if (Control != null && disposing)
			{
				Control.CheckedChanged -= OnCheckedChanged;
			}

			base.Dispose(disposing);
		}

		void OnCheckedChanged(object sender, EventArgs e)
		{
			UpdateTintColor();
		}

		protected override void UpdateTintColor()
		{
			if (Element.Color != Color.Default)
			{
				base.UpdateTintColor();
				return;
			}

			Control.CheckBoxTintColor = MaterialColors.GetCheckBoxColor(Control.IsChecked, Element.IsEnabled).ToColor();
		}
	}
}
