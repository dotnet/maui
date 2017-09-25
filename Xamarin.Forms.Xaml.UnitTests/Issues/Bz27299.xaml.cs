using System;
using System.Collections.Generic;

using Xamarin.Forms;

using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Bz27299ViewModel
	{
		public string Text {
			get { return "Foo"; }
		}
	}
	public class Bz27299ViewModelLocator
	{
		public static int Count { get; set; }
		public object Bz27299 {
			get {
				Count++;
				return new Bz27299ViewModel ();
			}
		}
	}

	public partial class Bz27299 : ContentPage
	{
		public Bz27299 ()
		{
			InitializeComponent ();
		}

		public Bz27299 (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp]
			public void SetUp ()
			{
				Bz27299ViewModelLocator.Count = 0;
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase(true)]
			[TestCase(false)]
			public void ViewModelLocatorOnlyCalledOnce (bool useCompiledXaml)
			{
				Assert.AreEqual (0, Bz27299ViewModelLocator.Count);
				var layout = new Bz27299 (useCompiledXaml);
				Assert.AreEqual (1, Bz27299ViewModelLocator.Count);
				Assert.AreEqual ("Foo", layout.label.Text);
			}
		}
	}
}