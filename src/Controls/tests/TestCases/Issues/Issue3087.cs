using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3087, "[Android] Non appcompat SwitchRenderer regression between 3.0 and 3.1",
		PlatformAffected.iOS)]
	public class Issue3087 : TestContentPage
	{
		LegacyComponents.NonAppCompatSwitch legacySwitch = null;
		Label status = new Label();
		protected override void Init()
		{
			Content =
				new StackLayout()
				{
					Children =
					{
						new Label(){ Text = "If nothing crashes this passes" },
						status
					}
				};
		}

		protected async override void OnAppearing()
		{
			base.OnAppearing();
			legacySwitch = new LegacyComponents.NonAppCompatSwitch() { IsToggled = true };
			(Content as StackLayout).Children.Add(legacySwitch);
			await Task.Delay(10);
			legacySwitch.IsToggled = !legacySwitch.IsToggled;
			await Task.Delay(10);
			legacySwitch.IsToggled = !legacySwitch.IsToggled;
			await Task.Delay(10);
			legacySwitch.IsToggled = !legacySwitch.IsToggled;
			status.Text = "Success";
		}
	}
}
