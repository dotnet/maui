using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public class WindowPageSwapTestCases : IEnumerable<object[]>
	{
		private readonly List<object[]> _data = new()
		{
			new object[] { new WindowPageSwapTestCase(typeof(TabbedPage), typeof(Shell), typeof(FlyoutPage)) },
			new object[] { new WindowPageSwapTestCase(typeof(Shell), typeof(NavigationPage), typeof(FlyoutPage)) },
			new object[] { new WindowPageSwapTestCase(typeof(NavigationPage), typeof(NavigationPage), typeof(Shell)) },
		};

		public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class WindowPageSwapTestCase : IEqualityComparer<WindowPageSwapTestCase>
	{

		Type[] _pageCombinations;
		Page _page;
		int index = 0;


		public WindowPageSwapTestCase(params Type[] pageCombinations)
		{
			_pageCombinations = pageCombinations;
		}

		public bool IsFinished() => index >= _pageCombinations.Length;

		public Page GetNextPageType()
		{
			if (IsFinished())
				return null;

			var result = _pageCombinations[index];
			index++;

			Page returnValue = null;
			if (result == typeof(ContentPage))
				returnValue = _page;
			else if (result == typeof(FlyoutPage))
			{
				_page.Title ??= "Details Page";
				returnValue = new FlyoutPage()
				{
					Detail = _page,
					Flyout = new ContentPage() { Title = "Flyout" }
				};
			}
			else if (result == typeof(TabbedPage))
			{
				returnValue = new TabbedPage()
				{
					Children =
					{
						_page
					}
				};
			}
			else if (result == typeof(NavigationPage))
			{
				returnValue = new NavigationPage(_page);
			}
			else if (result == typeof(Shell))
			{
				returnValue = new Shell() { CurrentItem = (_page as ContentPage) };
			}

			_page = null;
			return returnValue;
		}

		public void SetPageContent(Page page)
		{
			_page = page;
		}

		public override string ToString()
		{
			String debugName = String.Empty;

			foreach (var type in _pageCombinations)
			{
				if (!String.IsNullOrWhiteSpace(debugName))
					debugName += ", ";

				debugName += $"{type}";
			}

			return debugName;
		}

		public bool Equals(WindowPageSwapTestCase x, WindowPageSwapTestCase y)
		{
			return Object.ReferenceEquals(x, y);
		}

		public int GetHashCode([DisallowNull] WindowPageSwapTestCase obj)
		{
			return obj.GetHashCode();
		}
	}

}
