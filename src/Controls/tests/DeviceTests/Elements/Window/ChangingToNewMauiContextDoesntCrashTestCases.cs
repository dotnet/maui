// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowTests
	{
		class ChangingToNewMauiContextDoesntCrashTestCases : IEnumerable<object[]>
		{
			private readonly List<object[]> _data = new()
			{
				new object[] { true, typeof(NavPageWithTabbedPage) },
				new object[] { false, typeof(NavPageWithTabbedPage) },
				new object[] { true, typeof(FlyoutPageWithNavPageAndTabbedPage) },
				new object[] { false, typeof(FlyoutPageWithNavPageAndTabbedPage) },
			};

			public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			class NavPageWithTabbedPage : NavigationPage
			{
				public NavPageWithTabbedPage() : base(new TabbedPage() { Children = { new ContentPage() } })
				{
					Title = "Detail";
				}
			}

			class FlyoutPageWithNavPageAndTabbedPage : FlyoutPage
			{
				public FlyoutPageWithNavPageAndTabbedPage() : base()
				{
					Detail = new NavPageWithTabbedPage();
					Flyout = new ContentPage() { Title = "Flyout" };
				}
			}
		}
	}
}
