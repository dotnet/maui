using System;

namespace Microsoft.Maui
{
	public partial class EmbeddedFontLoader 
	{
		public string? LoadFont(EmbeddedFont font)
		{
			// gtk has no methods to load fonts
			// see: http://mces.blogspot.com/2015/05/how-to-use-custom-application-fonts.html
			
			throw new NotImplementedException();
			
			// freetype is needed. look at:
			// https://github.com/Robmaister/SharpFont
			
			 
			
		}
	}
}
