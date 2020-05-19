using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;
using System.Threading.Tasks;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif
namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8177, "[Bug] Picker does not update when it's underlying list changes content",
		PlatformAffected.UWP)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Picker)]
#endif
	public class Issue8177 : TestContentPage
	{
		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label { Text = "Open the Picker below. It should contain 3 items ('one', 'two', 'three'). Tap the button marked 'Change Picker Contents'. The Picker should now contain four items ('uno', 'dos', 'tres', 'quatro'). If it does not, the test has failed."};

			var button = new Button { Text = "Change Picker Contents " };
			
			var originalList = new List<string> { "one", "two", "three" };
			var picker = new Picker { ItemsSource = originalList };

			var newList = new List<string> { "uno", "dos", "tres", "quatro" };
			button.Clicked += (sender, args) => { picker.ItemsSource = newList; };

			layout.Children.Add(instructions);
			layout.Children.Add(button);
			layout.Children.Add(picker);

			Content = layout;
		}
	}
}
