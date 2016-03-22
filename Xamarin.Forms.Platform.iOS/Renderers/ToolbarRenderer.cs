using System.Linq;
using System.Drawing;
#if __UNIFIED__
using UIKit;

#else
using MonoTouch.UIKit;
#endif

namespace Xamarin.Forms.Platform.iOS
{
	public class ToolbarRenderer : ViewRenderer
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((Toolbar)Element).ItemAdded -= OnItemAdded;
				((Toolbar)Element).ItemRemoved -= OnItemRemoved;

				if (((UIToolbar)Control).Items != null)
				{
					foreach (var item in ((UIToolbar)Control).Items)
						item.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<View> e)
		{
			base.OnElementChanged(e);

			SetNativeControl(new UIToolbar(RectangleF.Empty));
			UpdateItems(false);

			((Toolbar)Element).ItemAdded += OnItemAdded;
			((Toolbar)Element).ItemRemoved += OnItemRemoved;
		}

		void OnItemAdded(object sender, ToolbarItemEventArgs eventArg)
		{
			UpdateItems(true);
		}

		void OnItemRemoved(object sender, ToolbarItemEventArgs eventArg)
		{
			UpdateItems(true);
		}

		void UpdateItems(bool animated)
		{
			if (((UIToolbar)Control).Items != null)
			{
				for (var i = 0; i < ((UIToolbar)Control).Items.Length; i++)
					((UIToolbar)Control).Items[i].Dispose();
			}
			var items = new UIBarButtonItem[((Toolbar)Element).Items.Count];
			for (var i = 0; i < ((Toolbar)Element).Items.Count; i++)
				items[i] = ((Toolbar)Element).Items[i].ToUIBarButtonItem();

			((UIToolbar)Control).SetItems(items, animated);
		}
	}
}