using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public class ShellBasicNavigationTestCases : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			yield return new object[] { new ShellItem[]
				{
					new ShellItem() { Items = { new ContentPage() }, Route = "page1" },
					new ShellItem() { Items = { new ContentPage() }, Route = "page2" }
				} };

			yield return new object[] {new ShellItem[]
				{
					new ShellSection() { Items = { new ContentPage() }, Route = "page1" },
					new ShellSection() { Items = { new ContentPage() }, Route = "page2" }
				} };

			yield return new object[] { new ShellItem[]
				{
					new ShellContent() { Content = new ContentPage(), Route = "page1" },
					new ShellContent() { Content = new ContentPage(), Route = "page2" }
				} };

			yield return new object[] { new ShellItem[]
				{
					new FlyoutItem()
					{
						Items =
						{
							new ShellContent() { Content = new ContentPage(), Route = "page1" },
							new ShellContent() { Content = new ContentPage(), Route = "page2" },
							new ShellContent() { Content = new ContentPage(), Route = "page3" },
						}
					}
				} };
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
