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

		Semantics _semantics;

		Semantics IView.Semantics
		{
			get
			{
				if (ControlTemplate != null)
				{
					_semantics ??= new Semantics();
					_semantics.Description = SemanticProperties.GetDescription(this) ?? this.ContentAsString();
				}

				return _semantics;
			}
		}
	}
}