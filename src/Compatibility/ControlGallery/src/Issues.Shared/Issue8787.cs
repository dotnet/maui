using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif
namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8787, "[Bug] Entry text initially invisible on UWP",
		PlatformAffected.UWP)]
	public partial class Issue8787 : TestContentPage
	{
		public Issue8787()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
		}
	}
}

