using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1758, "LayoutTo needs to be smarted about using layout specific API calls", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public class Issue1758 : ContentPage
	{
		ListView _list;
		Button _button;

		public Issue1758()
		{
			_list = new ListView { ItemsSource = new[] { "hello", "world", "from", "xamarin", "forms" } };

			_button = new Button { Text = "Button" };

			// The same behavior happens for both Absolute and Relative layout.
			//var layout = true ? Relative() : Absolute();
			var layout = Relative();

			Animate();

			Content = layout;
		}

		Compatibility.Layout Relative()
		{
			var layout = new Compatibility.RelativeLayout();

			layout.Children.Add(_list,
				Compatibility.Constraint.RelativeToParent(p => p.X),
				Compatibility.Constraint.RelativeToParent(p => p.Y),
				Compatibility.Constraint.RelativeToParent(p => p.Width),
				Compatibility.Constraint.RelativeToParent(p => p.Height)
			);

			layout.Children.Add(_button,
				Compatibility.Constraint.Constant(0),
				Compatibility.Constraint.Constant(300));

			return layout;
		}

		Layout Absolute()
		{
			var layout = new AbsoluteLayout { Children = { _list, _button } };

			AbsoluteLayout.SetLayoutBounds(_list, new Rect(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
			AbsoluteLayout.SetLayoutBounds(_button, new Rect(0, 300, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

			return layout;
		}

		async void Animate()
		{
			// Comment this delay out to see the bug
			// await Task.Delay(500);

			await _button.LayoutTo(new Rect(100, 100, 100, 100), 1000);
		}
	}
}
