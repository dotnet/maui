using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class OnPlatform : ContentPage
	{
		public OnPlatform()
		{
			InitializeComponent();
		}

		public OnPlatform(bool useCompiledXaml)
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

			[TestCase(false)]
			[TestCase(true)]
			public void BoolToVisibility(bool useCompiledXaml)
			{
				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(true, layout.label0.IsVisible);

				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.Android;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(false, layout.label0.IsVisible);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void DoubleToWidth(bool useCompiledXaml)
			{
				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(20, layout.label0.WidthRequest);

				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.Android;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(30, layout.label0.WidthRequest);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void StringToText(bool useCompiledXaml)
			{
				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual("Foo", layout.label0.Text);

				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.Android;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual("Bar", layout.label0.Text);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void OnPlatformAsResource(bool useCompiledXaml)
			{
				var layout = new OnPlatform(useCompiledXaml);
				var onplat = layout.Resources["fontAttributes"] as OnPlatform<FontAttributes>;
				Assert.NotNull(onplat);
#pragma warning disable 612
				Assert.AreEqual(FontAttributes.Bold, onplat.iOS);
				Assert.AreEqual(FontAttributes.Italic, onplat.Android);
#pragma warning restore 612

			}

			[TestCase(false)]
			[TestCase(true)]
			public void OnPlatformAsResourceAreApplied(bool useCompiledXaml)
			{
				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				var onidiom = layout.Resources["fontSize"] as OnIdiom<double>;
				Assert.NotNull(onidiom);
				Assert.That(onidiom.Phone, Is.TypeOf<double>());
				Assert.AreEqual(20, onidiom.Phone);
				Assert.AreEqual(FontAttributes.Bold, layout.label0.FontAttributes);

				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.Android;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(FontAttributes.Italic, layout.label0.FontAttributes);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void OnPlatform2Syntax(bool useCompiledXaml)
			{
				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.Android;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(42, layout.label0.HeightRequest);

				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(21, layout.label0.HeightRequest);


				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = "FooBar";
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(42, layout.label0.HeightRequest);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void OnPlatformDefault(bool useCompiledXaml)
			{
				((MockPlatformServices)Device.PlatformServices).RuntimePlatform = "\ud83d\ude80";
				var layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(63, layout.label0.HeightRequest);
			}
		}
	}
}