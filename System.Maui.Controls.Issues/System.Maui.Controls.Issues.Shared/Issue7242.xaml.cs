using System.Collections.ObjectModel;
using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System;
using System.Security.Cryptography;
using System.Maui.Xaml;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace System.Maui.Controls.Issues
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