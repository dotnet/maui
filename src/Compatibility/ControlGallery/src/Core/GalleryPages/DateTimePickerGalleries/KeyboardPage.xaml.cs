//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Converters;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.DateTimePickerGalleries
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
