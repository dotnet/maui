using System;
using System.Collections.Generic;

using Xamarin.Forms;

using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

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
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

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

			[TestCase(false)]
			[TestCase(true)]
			public void DoubleToWidth(bool useCompiledXaml)
			{
				Device.OS = TargetPlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(20, layout.label0.WidthRequest);

				Device.OS = TargetPlatform.Android;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(30, layout.label0.WidthRequest);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void StringToText(bool useCompiledXaml)
			{
				Device.OS = TargetPlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual("Foo", layout.label0.Text);

				Device.OS = TargetPlatform.Android;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual("Bar", layout.label0.Text);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void OnPlatformAsResource(bool useCompiledXaml)
			{
				var layout = new OnPlatform(useCompiledXaml);
				var onplat = layout.Resources ["fontAttributes"] as OnPlatform<FontAttributes>;
				Assert.NotNull(onplat);
				Assert.AreEqual(FontAttributes.Bold, onplat.iOS);

				Assert.AreEqual(FontAttributes.Italic, onplat.Android);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void OnPlatformAsResourceAreApplied(bool useCompiledXaml)
			{
				Device.OS = TargetPlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				var onidiom = layout.Resources ["fontSize"] as OnIdiom<double>;
				Assert.NotNull(onidiom);
				Assert.That(onidiom.Phone, Is.TypeOf<double>());
				Assert.AreEqual(20, onidiom.Phone);
				Assert.AreEqual(FontAttributes.Bold, layout.label0.FontAttributes);

				Device.OS = TargetPlatform.Android;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(FontAttributes.Italic, layout.label0.FontAttributes);
			}
		}
	}
}