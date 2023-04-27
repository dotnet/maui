using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public class ShellFlyoutTemplatePartsTestCases : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			yield return new object[] { "Flyout Header" };
			yield return new object[] { "Flyout Content" };
			yield return new object[] { "Flyout Footer" };
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		// The Runner parses tests names better if the inputs are primitives
		public static Action<Shell, object> GetTest(string name)
		{
			switch (name)
			{
				case "Flyout Header":
					return (Shell shell, object newContent) =>
					{
						shell.FlyoutHeader = newContent;
					};
				case "Flyout Content":
					return (Shell shell, object newContent) =>
					{
						shell.FlyoutContent = newContent;
					};
				case "Flyout Footer":
					return (Shell shell, object newContent) =>
					{
						shell.FlyoutFooter = newContent;
					};
			}

			throw new ArgumentException(nameof(name));
		}
	}
}
