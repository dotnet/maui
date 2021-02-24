using ElmSharp;
using EColor = ElmSharp.Color;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native
{
	public class SearchBar : Native.EditfieldEntry
	{
		public SearchBar(EvasObject parent) : base(parent)
		{
			EnableClearButton = true;
		}

		public void SetClearButtonColor(EColor color)
		{
			ClearButtonColor = color;
		}
	}
}