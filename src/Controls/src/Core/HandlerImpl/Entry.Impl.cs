namespace Microsoft.Maui.Controls
{
	public partial class Entry : IEntry
	{
		Font? _font;

		Font IText.Font 
		{ 
			get 
			{
				if (_font == null)
				{ 
					_font = Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);
				}

				return _font.Value;
			} 
		}
	}
}