// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.A
{
	public partial class Bz31234 : ContentPage
	{
		public Bz31234()
		{
			InitializeComponent();
		}

		public Bz31234(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(true), TestCase(false)]
			public void ShouldPass(bool useCompiledXaml)
			{
				new Bz31234(useCompiledXaml);
				Assert.Pass();
			}
		}
	}
}