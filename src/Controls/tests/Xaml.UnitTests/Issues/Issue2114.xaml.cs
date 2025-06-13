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

		// [TestFixture] - removed for xUnit
		public class Tests
		{
			public void SetUp()
			{
				Application.Current = null;
			}

			[InlineData(false)]]
			[InlineData(true)]]
			public void StaticResourceOnApplication(bool useCompiledXaml)
			{
				Issue2114 app;
				() => app = new Issue2114(useCompiledXaml)

				Assert.True(Current.Resources.ContainsKey("ButtonStyle"));
				Assert.True(Current.Resources.ContainsKey("NavButtonBlueStyle"));
				Assert.True(Current.Resources.ContainsKey("NavButtonGrayStyle"));
			}
		}
	}
}