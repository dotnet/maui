using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class StaticExtensionException : ContentPage
	{
		public StaticExtensionException()
		{
			InitializeComponent();
		}

		public StaticExtensionException(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Issue2115
		{
			[Theory]
			[InlineData(false)]
			public void xStaticThrowsMeaningfullException(bool useCompiledXaml)
			{
				new XamlParseExceptionConstraint(6, 34).Validate(() => new StaticExtensionException(useCompiledXaml));
			}
		}
	}
}