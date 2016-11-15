using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Reflection;

using NUnit.Framework;

using Xamarin.Forms.CustomAttributes;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;
using Xamarin.UITest.iOS;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category(UITestCategories.Frame)]
	internal class FrameUITests : _ViewUITests
	{
		public FrameUITests ()
		{
			PlatformViewType = Views.Frame;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.FrameGallery);
		}

		[UiTestExempt (ExemptReason.CannotTest, "Invalid interaction")]
		public override void _Focus () {}

		// TODO
		public override void _GestureRecognizers () {}

		[UiTestExempt (ExemptReason.CannotTest, "Invalid interaction")]
		public override void _IsFocused () {}

		[UiTestExempt (ExemptReason.CannotTest, "Invalid interaction")]
		public override void _UnFocus () {}

		// TODO
		// Implement control specific ui tests

		protected override void FixtureTeardown ()
		{
			App.NavigateBack ();
			base.FixtureTeardown ();
		}

		[UiTestExempt (ExemptReason.CannotTest, "Invalid interaction")]
		public override void _IsEnabled () {}
	}
}