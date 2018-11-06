using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using AImageButton = Android.Widget.ImageButton;

namespace Xamarin.Forms.Platform.Android
{
	internal sealed class ToolbarImageButton : AImageButton, IToolbarButton
	{
		IMenuItemController Controller => Item;
		public ToolbarImageButton(Context context, ToolbarItem item) : base(context)
		{
			if (item == null)
				throw new ArgumentNullException("item", "you should specify a ToolbarItem");
			Item = item;
			Enabled = Controller.IsEnabled;
			Bitmap bitmap;
			bitmap = Context.Resources.GetBitmap(Item.Icon);
			SetImageBitmap(bitmap);
			SetBackgroundColor(new Color(0, 0, 0, 0).ToAndroid());
			Click += (sender, e) => Controller.Activate();
			bitmap.Dispose();
			Item.PropertyChanged += HandlePropertyChanged;
		}

		public ToolbarItem Item { get; set; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				Item.PropertyChanged -= HandlePropertyChanged;
			base.Dispose(disposing);
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Controller.IsEnabledPropertyName)
				Enabled = Controller.IsEnabled;
		}
	}
}