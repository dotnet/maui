using System;
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh1554
	{
		public Gh1554()
		{
			InitializeComponent();
		}

		public Gh1554(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true), InlineData(false)]
			public void NestedRDAreOnlyProcessedOnce(bool useCompiledXaml)
			{
				var layout = new Gh1554(useCompiledXaml);
				Assert.Equal("label0", layout.Resources.MergedDictionaries.First().First().Key);
			}
		}
	}
}
