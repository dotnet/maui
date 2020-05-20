using Foundation;

#if __MOBILE__
using UIKit;
#else
using AppKit;
#endif

namespace System.Maui.Platform
{
	public static class NSAttributedTextExtensions
	{
		static NSAttributedStringDocumentAttributes htmlAttributes = new NSAttributedStringDocumentAttributes
		{
			DocumentType = NSDocumentType.HTML,
			StringEncoding = NSStringEncoding.UTF8
		};
		public static NSAttributedString ToNSAttributedString(this string html)
		{
			
#if __MOBILE__

			NSError nsError = null;

			return new NSAttributedString(html, htmlAttributes, ref nsError);
#else
			var htmlData = new NSMutableData();
			htmlData.SetData(html);

			return new NSAttributedString(html, htmlAttributes, out _);
#endif
		}

	}
}
