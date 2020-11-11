using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Xamarin.Forms.Controls.GalleryPages.DateTimePickerGalleries
{
	public partial class KeyboardPage : ContentPage
	{
		public KeyboardPage()
		{
			InitializeComponent();
            BindingContext = this;
            var dep = DependencyService.Get<ILocalize>();
            if (dep != null)
            {
                keyboardphoneculture.Text = $"Device Culture: {dep.GetCurrentCultureInfo()}";
            }
            else
            {
                var s = System.Globalization.CultureInfo.CurrentCulture.Name;
                keyboardphoneculture.Text = "Device Culture: " + s;
            }
        }

        void KeyboardType_SelectedIndexChanged(System.Object sender, System.EventArgs e)
        {
            string selectedValue = KeyboardType.Items[KeyboardType.SelectedIndex];
            var converter = new KeyboardTypeConverter();
            string keyboardStringValue = "Keyboard." + selectedValue;
            keyboardEntry.Keyboard = (Keyboard)converter.ConvertFromInvariantString(keyboardStringValue);
        }
    }
}
