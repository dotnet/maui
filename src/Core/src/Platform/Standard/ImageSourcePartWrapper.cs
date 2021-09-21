using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	public partial class ImageSourcePartLoader
	{
		public ImageSourcePartLoader(
			IElementHandler handler,
			Func<IImageSourcePart?> imageSourcePart,
			Action<object?> setImage)
			: this(handler, imageSourcePart)
		{
		}
	}
}
