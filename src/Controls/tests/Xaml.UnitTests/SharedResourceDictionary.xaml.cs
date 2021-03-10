using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class SharedResourceDictionary : ResourceDictionary
	{
		public SharedResourceDictionary()
		{
			InitializeComponent();
		}

		public SharedResourceDictionary(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void SetUp()
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
			public void ResourcesDirectoriesCanBeXamlRoots(bool useCompiledXaml)
			{
				var layout = new SharedResourceDictionary(useCompiledXaml);
				Assert.AreEqual(5, layout.Count);
			}
		}
	}
}