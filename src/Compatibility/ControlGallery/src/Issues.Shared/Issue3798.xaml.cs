using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3798, "[Android] SeparatorColor of ListView is NOT updated dynamically")]
	public partial class Issue3798 : TestContentPage
	{
#if UITEST
		[Test]
		[NUnit.Framework.Category(UITestCategories.ManualReview)]
		public void Issue3798Test()
		{
			RunningApp.WaitForElement("listViewSeparatorColor");
			RunningApp.Screenshot("Green ListView Separator");
			RunningApp.Tap(q => q.Marked("item1"));
			RunningApp.Screenshot("Red ListView Separator");
		}
#endif
		public Issue3798()
		{

#if APP
			InitializeComponent();
			this.BindingContext = new[] { "item1", "item2", "item3" };
#endif
		}

		bool _showRedSeparator;

		void OnItemTapped(object sender, ItemTappedEventArgs e)
		{

			if (e == null)
				return; // has been set to null, do not 'process' tapped event

			_showRedSeparator = !_showRedSeparator;
			//Uncomment the below code to test ListView SeparatorVisibility (Updating dynamically)
			//listView.SeparatorVisibility = _showRedSeparator ? SeparatorVisibility.None : SeparatorVisibility.Default;
#if APP
			listView.SeparatorColor = _showRedSeparator ? Colors.Red : Colors.Green;
#endif
			((ListView)sender).SelectedItem = null; // de-select the row

		}

		protected override void Init()
		{

		}
	}
}