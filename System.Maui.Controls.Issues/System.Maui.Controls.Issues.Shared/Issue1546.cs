using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1546, "Crash when Label.Text is null", PlatformAffected.Android)]
	public class Issue1546
		: ContentPage
	{
		public Issue1546()
		{
			Content = new Label();
		}
	}
}
