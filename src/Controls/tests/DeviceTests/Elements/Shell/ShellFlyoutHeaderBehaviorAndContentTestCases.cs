using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests
{
	public class ShellFlyoutHeaderBehaviorAndContentTestCases : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			foreach (var behavior in Enum.GetValues(typeof(FlyoutHeaderBehavior)))
			{
				// FlyoutHeaderBehavior, content type, header margin top (null is safe area), header margin bottom, content margin top, content margin bottom
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", null, null, 0, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", 10, 0, 0, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", 10, 20, 0, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", 0, 20, 0, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", null, null, 30, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", 10, 0, 30, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", 10, 20, 30, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", 0, 20, 30, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", null, null, 0, 40 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", 10, 0, 0, 40 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", 10, 20, 0, 40 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", 0, 20, 0, 40 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", null, null, 30, 40 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", 10, 0, 30, 40 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", 10, 20, 30, 40 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "ScrollView", 0, 20, 30, 40 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", null, null, 0, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", 10, 0, 0, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", 10, 20, 0, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", 0, 20, 0, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", null, null, 30, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", 10, 0, 30, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", 10, 20, 30, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", 0, 20, 30, 0 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", null, null, 0, 40 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", 10, 0, 0, 40 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", 10, 20, 0, 40 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", 0, 20, 0, 40 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", null, null, 30, 40 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", 10, 0, 30, 40 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", 10, 20, 30, 40 };
				yield return new object[] { (FlyoutHeaderBehavior)behavior, "VerticalStackLayout", 0, 20, 30, 40 };
			}
		}

		public static object GetFlyoutContentAction(string name, Thickness margin)
		{
			switch (name)
			{
				case "ScrollView":
					return new ScrollView() { SafeAreaEdges = SafeAreaEdges.None, Content = new Label() { Text = "ScrollView" }, Margin = margin, BackgroundColor = Colors.Orange };
				case "VerticalStackLayout":
					return new VerticalStackLayout() { SafeAreaEdges = SafeAreaEdges.None, Margin = margin, Children = { new Label() { Text = "VerticalStackLayout" } }, BackgroundColor = Colors.Orange };
			}

			throw new ArgumentException(nameof(name));
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
