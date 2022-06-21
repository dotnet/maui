using System;
using System.Drawing;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiSearchBar : UISearchBar
	{
		readonly WeakEventManager _weakEventManager = new WeakEventManager();
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

		protected internal MauiSearchBar(NativeHandle handle) : base(handle)
		{
		}

		// Native Changed doesn't fire when the Text Property is set in code
		// We use this event as a way to fire changes whenever the Text changes
		// via code or user interaction.
		public event EventHandler<UISearchBarTextChangedEventArgs>? TextSetOrChanged
		{
			add
			{
				if (value != null)
					_weakEventManager.AddEventHandler(value);
			}

			remove
			{
				if (value != null)
					_weakEventManager.RemoveEventHandler(value);
			}
		}

		public override string? Text
		{
			get => base.Text;
			set
			{
				var old = base.Text;

				base.Text = value;

				if (old != value)
				{
					_weakEventManager.HandleEvent(this, new UISearchBarTextChangedEventArgs(value ?? String.Empty), nameof(TextSetOrChanged));
				}
			}
		}
	}
}