using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

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
		public public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
			// [SetUp]
			[Xunit.Fact]
			public void SetUp()
			{
				Application.Current = null;
			}

			[InlineData(false)]
			[Theory]
			[InlineData(true)]
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