using System.ComponentModel;

namespace System.Maui
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IBorderElement
	{
		//note to implementor: implement this property publicly
		Color BorderColor { get; }
		int CornerRadius { get; }
		Color BackgroundColor { get; }
		double BorderWidth { get; }

		//note to implementor: but implement the methods explicitly
		void OnBorderColorPropertyChanged(Color oldValue, Color newValue);
		bool IsCornerRadiusSet();
		bool IsBackgroundColorSet();
		bool IsBorderColorSet();
		bool IsBorderWidthSet();
		int CornerRadiusDefaultValue { get; }
		Color BorderColorDefaultValue { get; }
		double BorderWidthDefaultValue { get; }
	}
}