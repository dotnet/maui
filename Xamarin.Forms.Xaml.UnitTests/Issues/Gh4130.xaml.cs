using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Gh4130Control:ContentView
	{
		public delegate void TextChangedEventHandler(object sender, TextChangedEventArgs args);
#pragma warning disable 067
		public event TextChangedEventHandler TextChanged;
#pragma warning restore 067
		public void FireEvent()
		{
			TextChanged?.Invoke(this, new TextChangedEventArgs(null, null));
		}
	}

	public partial class Gh4130 : ContentPage
	{
		public Gh4130()
		{
			InitializeComponent();
			var c = new Gh4130Control();
		}

		public Gh4130(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		void OnTextChanged(object sender, EventArgs e)
		{
			Assert.Pass();
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

			[TestCase(false),TestCase(true)]
			public void NonGenericEventHanlders(bool useCompiledXaml)
			{
				var layout = new Gh4130(useCompiledXaml);
				var control = layout.Content as Gh4130Control;
				control.FireEvent();
				Assert.Fail();
			}
		}
	}
}
