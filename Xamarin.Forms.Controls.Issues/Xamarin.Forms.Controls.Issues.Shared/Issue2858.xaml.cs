using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.InputTransparent)]
	[Category(UITestCategories.Gestures)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2858, "Transparency Cascading", PlatformAffected.Android)]
	public partial class Issue2858 : TestContentPage
	{
		const string Success = "Success";
		const string Failure = "Fail";
		const string InnerGrid = "InnerGrid";
		const string OuterGrid = "OuterGrid";

#pragma warning disable CS0414
		int _tapCount = 0;
#pragma warning restore CS0414

		public Issue2858 ()
		{
#if APP
			InitializeComponent ();
#endif
		}

#if APP
		void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
		{
			_tapCount += 1;

			if (_tapCount == 1)
			{
				Result.Text = Success;
			}
			else
			{
				Result.Text = Failure;
			}
		}
#endif

		protected override void Init()
		{
			
		}


#if UITEST
		[Test]
		public void CascadeInputTransparentGrids()
		{
			RunningApp.WaitForElement(InnerGrid);
			RunningApp.Tap(InnerGrid);

			var green = RunningApp.WaitForElement(OuterGrid);
			RunningApp.TapCoordinates(green[0].Rect.CenterX, green[0].Rect.Y + 20);

			RunningApp.WaitForElement(Success);
		}
#endif
	}
}