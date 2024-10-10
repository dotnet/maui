using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues;

[Preserve(AllMembers = true)]
[Issue(IssueTracker.Github, 1763, "First item of grouped ListView not firing .ItemTapped", PlatformAffected.WinPhone, NavigationBehavior.PushAsync)]
public class Issue1763 : TestTabbedPage
{
	public Issue1763()
	{

	}

	protected override void Init()
	{
		Title = "Contacts";
		Children.Add(new ContactsPage());
	}
}
