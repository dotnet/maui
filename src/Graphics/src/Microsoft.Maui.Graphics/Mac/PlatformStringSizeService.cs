using AppKit;
using Foundation;

namespace Microsoft.Maui.Graphics.Platform
{
	public partial class PlatformStringSizeService : IStringSizeService
	{
		public SizeF GetStringSize(string value, IFont font, float fontSize)
		{
			var nativeString = new NSString(value);
			var attributes = new NSMutableDictionary();
			attributes[NSStringAttributeKey.Font] = font?.ToPlatformFont(fontSize) ?? FontExtensions.GetDefaultPlatformFont(fontSize);
			var size = nativeString.StringSize(attributes);
			return size.AsSizeF();
		}
	}
}
