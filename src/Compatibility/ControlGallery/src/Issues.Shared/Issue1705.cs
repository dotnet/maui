using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1705, "Editor.IsEnabled = false", PlatformAffected.iOS)]
	public class Issue1705 : ContentPage
	{
		public Issue1705()
		{
			Title = "test page";
			Padding = 10;

			var layout = new StackLayout();
			var t = new Entry
			{
				IsEnabled = false
			};

			var e = new Editor
			{
				IsEnabled = false,
				BackgroundColor = Colors.Aqua
			};

			layout.Children.Add(e);
			layout.Children.Add(t);

			Content = layout;
		}
	}

}
