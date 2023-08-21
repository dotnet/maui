// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh2574 : ContentPage
	{
		public Gh2574()
		{
			InitializeComponent();
		}

		public Gh2574(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{

			[TestCase(false), TestCase(true)]
			public void xNameOnRoot(bool useCompiledXaml)
			{
				var layout = new Gh2574(useCompiledXaml);
				Assert.That(layout.page, Is.EqualTo(layout));
			}
		}
	}
}
