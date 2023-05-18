#nullable disable
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/RadioButton.xml" path="Type[@FullName='Microsoft.Maui.Controls.RadioButton']/Docs/*" />
	public partial class RadioButton : IRadioButton
	{
		Font ITextStyle.Font => this.ToFont();

#if ANDROID
		object IContentView.Content => ContentAsString();
#endif

		IView IContentView.PresentedContent => ((this as IControlTemplated).TemplateRoot as IView) ?? (Content as IView);

		double IButtonStroke.StrokeThickness => (double)GetValue(BorderWidthProperty);

		Color IButtonStroke.StrokeColor => (Color)GetValue(BorderColorProperty);

		int IButtonStroke.CornerRadius => (int)GetValue(CornerRadiusProperty);

		private protected override Semantics UpdateSemantics()
		{
			var semantics = base.UpdateSemantics();

			if (ControlTemplate != null)
			{
				string contentAsString = ContentAsString();

				if (!string.IsNullOrWhiteSpace(contentAsString) && string.IsNullOrWhiteSpace(semantics?.Description))
				{
					semantics ??= new Semantics();
					semantics.Description = contentAsString;
				}
			}

			return semantics;
		}
	}
}