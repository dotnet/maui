using System;
using System.Maui.Platform;

namespace System.Maui.Platform
{
	// THIS CLASS WILL BE ADDED AS A LINKED FILE ACROSS PLATFORMS
	// THEN ONCE MULTI TARGETING IS BETTER IT WON'T BE :-)
	public partial class LabelRenderer 
	{
		//public static PropertyMapper<IText> ITextMapper = new PropertyMapper<IText>(ViewRenderer.ViewMapper)
		//{
		//	[nameof(ILabel.Text)] = MapPropertyText,
		//	//[nameof(ILabel.Color)] = MapPropertyColor,
		//};

		public static PropertyMapper<ILabel> Mapper = new PropertyMapper<ILabel>(ViewRenderer.ViewMapper)
		{
			//[nameof(ILabel.Text)] = MapPropertyText,
			//[nameof(ILabel.Color)] = MapPropertyColor,
			//[nameof(ILabel.LineHeight)] = MapPropertyLineHeight,
		};

	}
}
