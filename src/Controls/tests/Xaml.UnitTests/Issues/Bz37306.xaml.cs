// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz37306 : ContentPage
	{
		public Bz37306()
		{
			InitializeComponent();
		}

		public Bz37306(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void xStringInResourcesDictionaries(bool useCompiledXaml)
			{
				var layout = new Bz37306(useCompiledXaml);
				Assert.AreEqual("Mobile App", layout.Resources["AppName"]);
				Assert.AreEqual("Mobile App", layout.Resources["ApplicationName"]);
			}
		}
	}
}