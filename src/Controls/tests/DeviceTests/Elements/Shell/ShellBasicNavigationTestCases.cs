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
			var uniqueId = Guid.NewGuid().ToString("N")[..8]; 

			yield return new object[] { new ShellItem[]
				{
					new ShellItem() { Items = { new ContentPage() }, Route = $"page1_{uniqueId}" },
					new ShellItem() { Items = { new ContentPage() }, Route = $"page2_{uniqueId}" }
				} };

			yield return new object[] {new ShellItem[]
				{
					new ShellSection() { Items = { new ContentPage() }, Route = $"page1_{uniqueId}_s" },
					new ShellSection() { Items = { new ContentPage() }, Route = $"page2_{uniqueId}_s" }
				} };

			yield return new object[] { new ShellItem[]
				{
					new ShellContent() { Content = new ContentPage(), Route = $"page1_{uniqueId}_c" },
					new ShellContent() { Content = new ContentPage(), Route = $"page2_{uniqueId}_c" }
				} };

			yield return new object[] { new ShellItem[]
				{
					new FlyoutItem()
					{
						Items =
						{
							new ShellContent() { Content = new ContentPage(), Route = $"page1_{uniqueId}_f" },
							new ShellContent() { Content = new ContentPage(), Route = $"page2_{uniqueId}_f" },
							new ShellContent() { Content = new ContentPage(), Route = $"page3_{uniqueId}_f" },
						}
					}
				} };
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
