using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ShellAppearanceTests : ShellTestBase
	{
		[Fact]
		public void ColorSetCorrectly()
		{
			var testShell = new TestShell(CreateShellItem<FlyoutItem>());
			testShell.Items[0].SetValue(Shell.DisabledColorProperty, Colors.Purple);

			ShellAppearance result = new ShellAppearance();
			result.Ingest(testShell.Items[0]);
			Assert.Equal(Colors.Purple, result.DisabledColor);
		}
	}
}
