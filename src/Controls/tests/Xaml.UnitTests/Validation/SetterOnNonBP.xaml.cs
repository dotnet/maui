// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Controls;
using NUnit.Framework;
namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class FakeView : View
	{
		public string NonBindable { get; set; }
	}

	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class SetterOnNonBP : ContentPage
	{
		public SetterOnNonBP()
		{
			InitializeComponent();
		}

		public SetterOnNonBP(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class SetterOnNonBPTests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void ShouldThrow(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws(new BuildExceptionConstraint(10, 13), () => MockCompiler.Compile(typeof(SetterOnNonBP)));
				else
					Assert.Throws(new XamlParseExceptionConstraint(10, 13), () => new SetterOnNonBP(useCompiledXaml));
			}
		}
	}
}