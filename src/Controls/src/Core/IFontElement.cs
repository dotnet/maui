using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IFontElement
	{
		//note to implementor: implement these properties publicly
		FontAttributes FontAttributes { get; }
		string FontFamily { get; }

		[TypeConverter(typeof(FontSizeConverter))]
		double FontSize { get; }

		bool FontScalingEnabled { get; }

		//note to implementor: but implement these methods explicitly
		void OnFontFamilyChanged(string oldValue, string newValue);
		void OnFontSizeChanged(double oldValue, double newValue);
		void OnFontScalingEnableChanged(bool oldValue, bool newValue);
		double FontSizeDefaultValueCreator();
		void OnFontAttributesChanged(FontAttributes oldValue, FontAttributes newValue);
		void OnFontChanged(Font oldValue, Font newValue);
	}
}