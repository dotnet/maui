using System;
using System.Collections.Generic;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal class PageContainer : UIView, IUIAccessibilityContainer
	{
		readonly IAccessibilityElementsController _parent;
		NSArray _accessibilityElements = null;
		bool _disposed;
		bool _loaded;

		public PageContainer(IAccessibilityElementsController parent)
		{
			AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			_parent = parent;
		}

		public PageContainer()
		{
			IsAccessibilityElement = false;
		}

		public override bool IsAccessibilityElement
		{
			get => false;
			set => base.IsAccessibilityElement = value;
		}

		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public virtual NSArray AccessibilityElements
		{
			[Export("accessibilityElements", ArgumentSemantic.Copy)]
			get
			{
				if (_loaded)
					return _accessibilityElements;

				// lazy-loading this list so that the expensive call to GetAccessibilityElements only happens when VoiceOver is on.
				if (_accessibilityElements == null || _accessibilityElements.Count == 0)
				{
					var elements = _parent.GetAccessibilityElements();
					if (elements != null)
					{
						_accessibilityElements = NSArray.FromNSObjects(elements.ToArray());
					}
				}

				_loaded = true;
				return _accessibilityElements;
			}
		}

		public void ClearAccessibilityElements()
		{
			_accessibilityElements = null;
			_loaded = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				ClearAccessibilityElements();
				_disposed = true;
			}
			base.Dispose(disposing);
		}
	}
}