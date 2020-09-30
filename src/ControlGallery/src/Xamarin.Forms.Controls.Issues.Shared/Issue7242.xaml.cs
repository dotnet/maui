using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;


namespace Xamarin.Forms.Controls.Issues
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