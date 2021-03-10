using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Issue2114 : Application
	{
		public Issue2114()
		{
			InitializeComponent();
		}

		public Issue2114(bool useCompiledXaml)
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

				Current = null;
			}

			[TestCase(false)]
			[TestCase(true)]
			public void StaticResourceOnApplication(bool useCompiledXaml)
			{
				Issue2114 app;
				Assert.DoesNotThrow(() => app = new Issue2114(useCompiledXaml));

				Assert.True(Current.Resources.ContainsKey("ButtonStyle"));
				Assert.True(Current.Resources.ContainsKey("NavButtonBlueStyle"));
				Assert.True(Current.Resources.ContainsKey("NavButtonGrayStyle"));
			}
		}
	}
}