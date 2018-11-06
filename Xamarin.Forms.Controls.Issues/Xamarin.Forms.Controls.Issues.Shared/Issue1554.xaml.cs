﻿using System;
using System.Collections.Generic;
using Xamarin.Forms;
﻿using Xamarin.Forms.CustomAttributes;
﻿using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
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

