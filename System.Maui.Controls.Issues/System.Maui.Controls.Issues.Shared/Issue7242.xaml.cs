using System.Collections.ObjectModel;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System;
using System.Security.Cryptography;
using Xamarin.Forms.Xaml;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;


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