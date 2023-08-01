using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh2304
	{
		[TestFixture]
		class Tests
		{
			[Test]
			public void XamlCDoesntFail()
			{
				new Gh2304();
			}
		}
	}
}
