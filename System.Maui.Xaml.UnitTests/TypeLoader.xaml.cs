using System;
using System.Collections.Generic;

using System.Maui;

using NUnit.Framework;
using System.Maui.Core.UnitTests;

namespace System.Maui.Xaml.UnitTests
{
	public partial class TypeLoader : ContentPage
	{
		public TypeLoader ()
		{
			InitializeComponent ();
		}

		public TypeLoader (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void SetUp ()
			{
				Device.PlatformServices = new MockPlatformServices ();
				Application.Current = new MockApplication ();
			}

			[TestCase (false)]
			[TestCase (true)]
			public void LoadTypeFromXmlns (bool useCompiledXaml)
			{
				TypeLoader layout = null;
				Assert.DoesNotThrow (() => layout = new TypeLoader (useCompiledXaml));
				Assert.NotNull (layout.customview0);
				Assert.That (layout.customview0, Is.TypeOf<CustomView> ());
			}

			[TestCase (false)]
			[TestCase (true)]
			public void LoadTypeFromXmlnsWithoutAssembly (bool useCompiledXaml)
			{
				TypeLoader layout = null;
				Assert.DoesNotThrow (() => layout = new TypeLoader (useCompiledXaml));
				Assert.NotNull (layout.customview1);
				Assert.That (layout.customview1, Is.TypeOf<CustomView> ());
			}
		}
	}
}