using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

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
		}		public class Tests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void ResourcesDirectoriesCanBeXamlRoots(bool useCompiledXaml)
			{
				var layout = new SharedResourceDictionary(useCompiledXaml);
				Assert.Equal(5, layout.Count);
			}
		}
	}
}