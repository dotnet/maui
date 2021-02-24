using Microsoft.Maui.Essentials;
using Xunit;

namespace Tests
{
	public class DeviceDisplay_Tests
	{
		[Theory]
		[InlineData(0.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, 0.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, true)]
		[InlineData(1.1, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, 1.1, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, true)]
		[InlineData(0.0, 0.0, 0.0, DisplayOrientation.Portrait, DisplayRotation.Rotation0, 0.0, 0.0, 0.0, DisplayOrientation.Portrait, DisplayRotation.Rotation0, true)]
		[InlineData(1.1, 0.0, 2.2, DisplayOrientation.Landscape, DisplayRotation.Rotation180, 1.1, 0.0, 2.2, DisplayOrientation.Landscape, DisplayRotation.Rotation180, true)]
		[InlineData(1.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, 0.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, false)]
		[InlineData(0.0, 1.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, 0.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, false)]
		[InlineData(0.0, 0.0, 1.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, 0.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, false)]
		[InlineData(0.0, 0.0, 0.0, DisplayOrientation.Portrait, DisplayRotation.Rotation0, 0.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, false)]
		[InlineData(1.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation180, 0.0, 0.0, 0.0, DisplayOrientation.Landscape, DisplayRotation.Rotation0, false)]
		public void DeviceDisplay_Comparison(
		   double width1,
		   double height1,
		   double density1,
		   DisplayOrientation orientation1,
		   DisplayRotation rotation1,
		   double width2,
		   double height2,
		   double density2,
		   DisplayOrientation orientation2,
		   DisplayRotation rotation2,
		   bool equals)
		{
			var device1 = new DisplayInfo(
				width: width1,
				height: height1,
				density: density1,
				orientation: orientation1,
				rotation: rotation1);

			var device2 = new DisplayInfo(
				width: width2,
				height: height2,
				density: density2,
				orientation: orientation2,
				rotation: rotation2);

			if (equals)
			{
				Assert.True(device1.Equals(device2));
				Assert.True(device1 == device2);
				Assert.False(device1 != device2);
				Assert.Equal(device1, device2);
				Assert.Equal(device1.GetHashCode(), device2.GetHashCode());
			}
			else
			{
				Assert.False(device1.Equals(device2));
				Assert.True(device1 != device2);
				Assert.False(device1 == device2);
				Assert.NotEqual(device1, device2);
				Assert.NotEqual(device1.GetHashCode(), device2.GetHashCode());
			}
		}
	}
}
