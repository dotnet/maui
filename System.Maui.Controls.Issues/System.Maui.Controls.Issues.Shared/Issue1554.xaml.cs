using System;
using System.Collections.Generic;
using System.Maui;
﻿using System.Maui.CustomAttributes;
﻿using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{	
#if APP
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1554, "NRE: NavigationProxy.set_Inner", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public partial class Issue1554 : ContentPage
	{	
		public Issue1554 ()
		{
			BindingContext = new
			{
				Values = new[] { "ABC", "DEF", "GHI" }
			};

			InitializeComponent ();
		}
	}
#endif
}

