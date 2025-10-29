using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Unreported003 : ContentPage
	{
		public Unreported003()
		{
			InitializeComponent();
		}

		public Unreported003(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true), InlineData(false)]
			public void AllowCtorArgsForValueTypes(bool useCompiledXaml)
			{
				var page = new Unreported003(useCompiledXaml);
			}
		}
	}
}