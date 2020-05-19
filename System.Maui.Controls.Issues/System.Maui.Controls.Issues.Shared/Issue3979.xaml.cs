using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Maui;
using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Maui.Xaml;

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3979, "Removing LineBreakMode support from WindowsResourcesProvider", PlatformAffected.UWP)]
	public partial class Issue3979 : TestContentPage
	{
		public Issue3979()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
#if APP
		private void Button_OnClicked(object sender, EventArgs e)
		{
			TargetSpan.Style = Device.Styles.BodyStyle;
		}
#endif
	}
}