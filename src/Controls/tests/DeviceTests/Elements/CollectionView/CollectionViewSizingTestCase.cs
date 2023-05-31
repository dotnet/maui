using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit.Abstractions;

namespace Microsoft.Maui.DeviceTests
{
	public class CollectionViewSizingTestCase : IXunitSerializable
	{
		public LayoutOptions LayoutOptions { get; private set; }
		public LinearItemsLayout ItemsLayout { get; private set; }

		public CollectionViewSizingTestCase() { }

		public CollectionViewSizingTestCase(LayoutOptions layoutOptions, LinearItemsLayout linearItemsLayout)
		{
			LayoutOptions = layoutOptions;
			ItemsLayout = linearItemsLayout;
		}

		public void Deserialize(IXunitSerializationInfo info)
		{
			var orientationString = info.GetValue<string>(nameof(ItemsLayout));
			var orientation = (ItemsLayoutOrientation)Enum.Parse(typeof(ItemsLayoutOrientation), orientationString);
			ItemsLayout = new LinearItemsLayout(orientation);

			var alignmentString = info.GetValue<string>(nameof(LayoutOptions));
			var alignment = (LayoutAlignment)Enum.Parse(typeof(LayoutAlignment), alignmentString);
			LayoutOptions = new LayoutOptions(alignment, false);
		}

		public void Serialize(IXunitSerializationInfo info)
		{
			info.AddValue(nameof(LayoutOptions), LayoutOptions.Alignment.ToString(), typeof(string));
			info.AddValue(nameof(ItemsLayout), ItemsLayout.Orientation.ToString(), typeof(string));
		}

		public override string ToString()
		{
			var optionsString = LayoutOptions.Alignment.ToString();
			return $"{ItemsLayout.Orientation}, {optionsString}";
		}
	}
}