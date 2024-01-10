using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests
{
	public class ShellFlyoutHeaderScrollTestCases : IEnumerable<object[]>
	{
		string[] ContentType = ["ShellItems", "CollectionView", "ScrollView"];

		public IEnumerator<object[]> GetEnumerator()
		{
			foreach (var behavior in Enum.GetValues(typeof(FlyoutHeaderBehavior)))
			{
				foreach (var contentType in ContentType)
				{
					yield return new object[] { (FlyoutHeaderBehavior)behavior, contentType };
				}
			}
		}

		public static void SetFlyoutContent(string contentType, Shell shell)
		{
			switch (contentType)
			{
				case "ShellItems":
					AddShellItems(shell);
					break;
				case "CollectionView":
					AddCollectionView(shell);
					break;
				case "ScrollView":
					AddScrollView(shell);
					break;
			}
		}

		static void AddShellItems(Shell shell)
		{
			Enumerable.Range(0, 100)
					.ForEach(i =>
					{
						shell.Items.Add(new FlyoutItem() { Title = $"FlyoutItem {i}", Items = { new ContentPage() } });
					});
		}

		static void AddCollectionView(Shell shell)
		{
			shell.FlyoutContent = new CollectionView()
			{
				ItemsSource = Enumerable.Range(0, 100).ToList(),
				BackgroundColor = Colors.Orange
			};
		}

		static void AddScrollView(Shell shell)
		{
			var grid = new Grid
			{
				BackgroundColor = Colors.Orange
			};

			for (int i = 0; i < 100; i++)
			{
				var item = new Label() { Text = $"Item {i}" };
				grid.RowDefinitions.Add(new RowDefinition() { Height = 40 });
				grid.SetRow(item, i);
				grid.Add(item);
			}

			shell.FlyoutContent = new ScrollView()
			{
				Content = grid
			};
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
