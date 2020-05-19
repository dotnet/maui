using System;
using System.Collections.Generic;

using Xamarin.Forms;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class BPNotResolvedOnSubClass : ContentPage
	{
		public static readonly BindableProperty ShadowColorProperty =
			BindableProperty.CreateAttached ("ShadowColor", typeof (Color), typeof (BPNotResolvedOnSubClass), Color.Default);

		public static Color GetShadowColor (Element bindable) // Change to Element instead of BindableObject o make fail
		{
			return (Color)bindable.GetValue (ShadowColorProperty);
		}

		public BPNotResolvedOnSubClass ()
		{
			InitializeComponent ();
		}

		public BPNotResolvedOnSubClass (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
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

			[TestCase(true)]
			[TestCase(false)]
			public void CorrectlyResolveBPOnSubClasses (bool useCompiledXaml)
			{
				var layout = new BPNotResolvedOnSubClass (useCompiledXaml);
				var style = (Style)layout.Resources["Xamarin.Forms.Button"];
				Assert.NotNull (style);

				var button = new Button ();
				button.Style = style;

				Assert.AreEqual (Color.FromHex ("#dddddd"), button.GetValue (ShadowColorProperty));
			}
		}
	}
}

