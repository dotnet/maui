using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;


namespace Microsoft.Maui.Controls.ControlGallery.Issues
{


#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7242, "[Bug] iOS FontSize is calculated incorrectly", PlatformAffected.iOS)]
	public partial class Issue7242 : TestContentPage
	{
#if APP
		public Issue7242()
		{
			InitializeComponent();
		}
#endif
		protected override void Init()
		{

		}
	}

}