using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Maui;
using System.Maui.Xaml;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
#if APP
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7048, "[Bug][UWP] CheckBox Has Incosistent Paddings",
		PlatformAffected.UWP)]
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Issue7048 : ContentPage
    {
        public Issue7048()
        {
            InitializeComponent();
        }
    }
#endif
}