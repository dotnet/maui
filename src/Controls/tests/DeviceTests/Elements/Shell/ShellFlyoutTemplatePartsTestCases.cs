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
			yield return new object[] { (Shell shell, object newContent) => {
				shell.FlyoutHeader = newContent;
				return "Flyout Header";
			} };
			yield return new object[] { (Shell shell, object newContent) => {
				shell.FlyoutContent = newContent;
				return "Flyout Content";
			} };
			yield return new object[] { (Shell shell, object newContent) => {
				shell.FlyoutFooter = newContent;
				return "Flyout Footer";
			} };
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
