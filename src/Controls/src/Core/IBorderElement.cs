using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IBorderElement
	{
		//note to implementor: implement this property publicly
		Color BorderColor { get; }
		int CornerRadius { get; }
		Color BackgroundColor { get; }
		Brush Background { get; }
		double BorderWidth { get; }

		//note to implementor: but implement the methods explicitly
		void OnBorderColorPropertyChanged(Color oldValue, Color newValue);
		bool IsCornerRadiusSet();
		bool IsBackgroundColorSet();
		bool IsBackgroundSet();
		bool IsBorderColorSet();
		bool IsBorderWidthSet();
		int CornerRadiusDefaultValue { get; }
		Color BorderColorDefaultValue { get; }
		double BorderWidthDefaultValue { get; }
	}
}