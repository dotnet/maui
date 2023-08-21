// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class AutoMergedResourceDictionaries : ContentPage
	{
		public AutoMergedResourceDictionaries()
		{
			InitializeComponent();
		}

		public AutoMergedResourceDictionaries(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void AutoMergedRd(bool useCompiledXaml)
			{
				var layout = new AutoMergedResourceDictionaries(useCompiledXaml);
				Assert.That(layout.label.TextColor, Is.EqualTo(Colors.Purple));
				Assert.That(layout.label.BackgroundColor, Is.EqualTo(Color.FromArgb("#FF96F3")));
			}
		}
	}
}
