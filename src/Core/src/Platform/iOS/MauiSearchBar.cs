using System;
using System.Drawing;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public class MauiSearchBar : UISearchBar
	{
		public MauiSearchBar() : this(RectangleF.Empty)
		{
		}

		public MauiSearchBar(NSCoder coder) : base(coder)
		{
		}

		public MauiSearchBar(CGRect frame) : base(frame)
		{
		}

		protected MauiSearchBar(NSObjectFlag t) : base(t)
		{
		}

		protected internal MauiSearchBar(IntPtr handle) : base(handle)
		{
		}

		public override string? Text
		{
			get => base.Text;
			set
			{
				var old = base.Text;

				base.Text = value;

				if (old != value)
					TextPropertySet?.Invoke(this, EventArgs.Empty);
			}
		}

		public event EventHandler? TextPropertySet;
	}
}