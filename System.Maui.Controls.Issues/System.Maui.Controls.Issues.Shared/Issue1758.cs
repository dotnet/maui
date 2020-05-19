using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1758, "LayoutTo needs to be smarted about using layout specific API calls", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
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

		Layout Relative()
		{
			var layout = new RelativeLayout();
                   
			layout.Children.Add(_list,
				Forms.Constraint.RelativeToParent(p => p.X),
				Forms.Constraint.RelativeToParent(p => p.Y),
				Forms.Constraint.RelativeToParent(p => p.Width),
				Forms.Constraint.RelativeToParent(p => p.Height)
			);
        
			layout.Children.Add(_button, 
				Forms.Constraint.Constant(0),
				Forms.Constraint.Constant(300));
 
			return layout;
		}

		Layout Absolute()
		{
			var layout = new AbsoluteLayout { Children = { _list, _button } };
 
			AbsoluteLayout.SetLayoutBounds(_list, new Rectangle(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
			AbsoluteLayout.SetLayoutBounds(_button, new Rectangle(0, 300, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
 
			return layout;
		}

		async void Animate()
		{
			// Comment this delay out to see the bug
			// await Task.Delay(500);
 
			await _button.LayoutTo(new Rectangle(100, 100, 100, 100), 1000);
		}
	}
}
