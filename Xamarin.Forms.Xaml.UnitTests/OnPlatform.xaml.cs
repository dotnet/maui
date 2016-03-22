using System;
using System.Collections.Generic;

using Xamarin.Forms;

using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class OnPlatform : ContentPage
	{
		public OnPlatform ()
		{
			InitializeComponent ();
		}

		public OnPlatform (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase (false)]
			[TestCase (true)]
			public void BoolToVisibility (bool useCompiledXaml)
			{
				Device.OS = TargetPlatform.iOS;
				var layout = new OnPlatform (useCompiledXaml);
				Assert.AreEqual (true, layout.label0.IsVisible);

				Device.OS = TargetPlatform.Android;
				layout = new OnPlatform (useCompiledXaml);
				Assert.AreEqual (false, layout.label0.IsVisible);
			}
		}

		public void T ()
		{
			var onplat = new OnPlatform<bool> ();
			var label = new Label ();
			label.IsVisible = onplat;
		}
	}
}