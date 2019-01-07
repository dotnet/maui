using System;
using System.ComponentModel;
using Android.Content;

namespace Xamarin.Forms.Platform.Android
{
	internal sealed class ToolbarButton : global::Android.Widget.Button, IToolbarButton
	{
		MenuItem MenuItem => Item;

		public ToolbarButton(Context context, ToolbarItem item) : base(context)
		{
			Item = item ?? throw new ArgumentNullException(nameof(item), "you should specify a ToolbarItem");
			Enabled = MenuItem.IsEnabled;
			Text = Item.Text;
			SetBackgroundColor(new Color(0, 0, 0, 0).ToAndroid());
			Click += (sender, e) => ((IMenuItemController)MenuItem).Activate();
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
			if (e.PropertyName == MenuItem.IsEnabledProperty.PropertyName)
				Enabled = MenuItem.IsEnabled;
		}
	}
}