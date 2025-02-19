using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.InputTransparent)]
	[Category(UITestCategories.Gestures)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
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

		public Issue2858()
		{
#if APP
			InitializeComponent();
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
		[FailsOnMauiAndroid]
		[FailsOnMauiIOS]
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