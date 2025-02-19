using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.iOS;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2289, "TextCell IsEnabled property not disabling element in TableView", PlatformAffected.iOS)]
	public partial class Issue2289 : TestContentPage
	{

#if APP
		public Issue2289()
		{
			InitializeComponent();
		}
#endif

		protected override void Init()
		{
			MoreCommand = new Command<MenuItem>((menuItem) =>
			{
				Debug.WriteLine("More! Command Called!");
			});

			DeleteCommand = new Command<MenuItem>((menuItem) =>
			{
				Debug.WriteLine("Delete Command Called!");
			});
			BindingContext = this;
		}

		public ICommand MoreCommand { get; protected set; }

		public ICommand DeleteCommand { get; protected set; }

#if UITEST && __IOS__
		[Test]
		[Ignore("Fails sometimes on XTC")]
		public void TestIsEnabledFalse ()
		{
			var disable1 = RunningApp.Query (c => c.Marked ("txtCellDisable1")) [0];
			Assert.IsFalse (disable1.Enabled);
			var disable2 = RunningApp.Query (c => c.Marked ("txtCellDisable2")) [0];
			Assert.IsFalse (disable2.Enabled);
		}

		[Test]
		[Ignore("Fails sometimes on XTC")]
		public void TestIsEnabledFalseContextActions ()
		{
			var disable1 = RunningApp.Query (c => c.Marked ("txtCellDisableContextActions1")) [0];
			Assert.IsFalse (disable1.Enabled);

			var screenBounds = RunningApp.RootViewRect();

			RunningApp.DragCoordinates (screenBounds.Width - 10, disable1.Rect.CenterY, 10, disable1.Rect.CenterY);

			RunningApp.Screenshot ("Not showing context menu");
			RunningApp.WaitForNoElement (c => c.Marked ("More"));
			RunningApp.TapCoordinates (screenBounds.CenterX, screenBounds.CenterY);
		}

		[Test]
		[Ignore("Fails sometimes on XTC")]
		public void TestIsEnabledTrue ()
		{
			var disable1 = RunningApp.Query (c => c.Marked ("txtCellEnable1")) [0];
			Assert.IsTrue (disable1.Enabled);
			var disable2 = RunningApp.Query (c => c.Marked ("txtCellEnable2")) [0];
			Assert.IsTrue (disable2.Enabled);
		}

		[Test]
		[Ignore("Fails sometimes on XTC")]
		public void TestIsEnabledTrueContextActions ()
		{
			var disable1 = RunningApp.Query (c => c.Marked ("txtCellEnabledContextActions1")) [0];
			Assert.IsTrue (disable1.Enabled);

			var screenBounds = RunningApp.RootViewRect();

			RunningApp.DragCoordinates (screenBounds.Width - 10, disable1.Rect.CenterY, 10, disable1.Rect.CenterY);

			RunningApp.Screenshot ("Showing context menu");
			RunningApp.WaitForElement (c => c.Marked ("More"));
		}
#endif

	}
}

