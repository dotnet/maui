using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public partial class ImageButton
	{
		internal override void RemapForControls(HashSet<Type> remapped)
		{
			if (remapped.Add(typeof(ImageButton)))
			{
				base.RemapForControls(remapped);

#if ANDROID
				ImageButtonHandler.Mapper.ReplaceMapping<ImageButton, IImageButtonHandler>(PlatformConfiguration.AndroidSpecific.ImageButton.RippleColorProperty.PropertyName, MapRippleColor);
				ImageButtonHandler.Mapper.AppendToMapping<ImageButton, IImageButtonHandler>(nameof(Background), MapRippleColor);
#endif
			}
		}
	}
}
