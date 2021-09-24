using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ShellAppearanceTests : ShellTestBase
	{
		[Test]
		public void ColorSetCorrectly()
		{
			var testShell = new TestShell(CreateShellItem<FlyoutItem>());
			testShell.Items[0].SetValue(Shell.DisabledColorProperty, Colors.Purple);

			ShellAppearance result = new ShellAppearance();
			result.Ingest(testShell.Items[0]);
			Assert.AreEqual(Colors.Purple, result.DisabledColor);
		}
	}
}
