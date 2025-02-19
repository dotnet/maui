using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 51642, "Delayed BindablePicker UWP", PlatformAffected.All)]
	public partial class Bugzilla51642 : ContentPage
	{
#if APP
		public Bugzilla51642()
		{
			InitializeComponent();
			LoadDelayedVM();
			BoundPicker.SelectedIndexChanged += (s, e) =>
			{
				SelectedItemLabel.Text = BoundPicker.SelectedItem.ToString();
			};
		}

		public async void LoadDelayedVM()
		{
			await Task.Delay(1000);
			Device.BeginInvokeOnMainThread(() => BindingContext = new Bz51642VM());
		}
#endif
	}

	[Preserve(AllMembers = true)]
	class Bz51642VM
	{
		public IList<string> Items
		{
			get
			{
				return new List<String> { "Foo", "Bar", "Baz" };
			}
		}
	}
}
