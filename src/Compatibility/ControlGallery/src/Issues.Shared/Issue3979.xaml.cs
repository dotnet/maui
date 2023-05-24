using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
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