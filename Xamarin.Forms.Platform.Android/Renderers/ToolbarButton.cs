using System;
using System.ComponentModel;
using Android.Content;

namespace Xamarin.Forms.Platform.Android
{
	internal sealed class ToolbarButton : global::Android.Widget.Button, IToolbarButton
	{
		public ToolbarButton(Context context, ToolbarItem item) : base(context)
		{
			if (item == null)
				throw new ArgumentNullException("item", "you should specify a ToolbarItem");
			Item = item;
			Enabled = Item.IsEnabled;
			Text = Item.Text;
			SetBackgroundColor(new Color(0, 0, 0, 0).ToAndroid());
			Click += (sender, e) => Item.Activate();
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
				Enabled = Item.IsEnabled;
		}
	}
}