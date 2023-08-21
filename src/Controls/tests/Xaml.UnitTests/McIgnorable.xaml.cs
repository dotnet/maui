// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class McIgnorable : ContentPage
	{
		public McIgnorable()
		{
			InitializeComponent();
		}

		public McIgnorable(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}


		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void DoesNotThrow(bool useCompiledXaml)
			{
				var layout = new McIgnorable(useCompiledXaml);
			}
		}
	}
}