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

	public class WindowPageSwapTestCase
	{

		Type[] _pageCombinations;
		public ContentPage Page { get; private set; }
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

			Page = CreateNewPage();
			Page returnValue = null;
			if (result == typeof(ContentPage))
				returnValue = Page;
			else if (result == typeof(FlyoutPage))
			{
				Page.Title ??= "Details Page";
				returnValue = new FlyoutPage()
				{
					Detail = Page,
					Flyout = new ContentPage() { Title = "Flyout" }
				};
			}
			else if (result == typeof(TabbedPage))
			{
				returnValue = new TabbedPage()
				{
					Children =
					{
						Page
					}
				};
			}
			else if (result == typeof(NavigationPage))
			{
				returnValue = new NavigationPage(Page);
			}
			else if (result == typeof(Shell))
			{
				returnValue = new Shell() { CurrentItem = (Page as ContentPage) };
			}

			return returnValue;
		}

		ContentPage CreateNewPage()
		{
			var labelToTest = new Label();
			var contentToTest = new ContentPage()
			{
				Title = "title",
				ToolbarItems =
				{
					new ToolbarItem()
					{
						Text = "Item"
					}
				},
				Content = new VerticalStackLayout()
				{
					labelToTest
				}
			};

			return contentToTest;
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
	}

}
