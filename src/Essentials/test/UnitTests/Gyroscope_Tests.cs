using Microsoft.Maui.Essentials;
using Xunit;

namespace Tests
{
	public class Gyroscope_Tests
	{
		[Fact]
		public void Gyroscope_Start() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Gyroscope.Stop());

		[Fact]
		public void Gyroscope_Stop() =>
			Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Gyroscope.Start(SensorSpeed.Default));

		[Fact]
		public void Gyroscope_IsMonitoring() =>
			Assert.False(Gyroscope.IsMonitoring);

		[Fact]
		public void Gyroscope_IsSupported() =>
				  Assert.Throws<NotImplementedInReferenceAssemblyException>(() => Gyroscope.IsSupported);

		[Theory]
		[InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, true)]
		[InlineData(0.0, 0.0, 0.0, 1.0, 0.0, 0.0, false)]
		[InlineData(0.0, 0.0, 0.0, 0.0, 1.0, 0.0, false)]
		[InlineData(0.0, 0.0, 0.0, 0.0, 0.0, 1.0, false)]
		public void GyroscopeData_Comparison(
			float x1,
			float y1,
			float z1,
			float x2,
			float y2,
			float z2,
			bool equals)
		{
			var data1 = new GyroscopeData(x1, y1, z1);
			var data2 = new GyroscopeData(x2, y2, z2);
			if (equals)
			{
				Assert.True(data1.Equals(data2));
				Assert.True(data1 == data2);
				Assert.False(data1 != data2);
				Assert.Equal(data1, data2);
				Assert.Equal(data1.GetHashCode(), data2.GetHashCode());
			}
			else
			{
				Assert.False(data1.Equals(data2));
				Assert.False(data1 == data2);
				Assert.True(data1 != data2);
				Assert.NotEqual(data1, data2);
				Assert.NotEqual(data1.GetHashCode(), data2.GetHashCode());
			}
		}
	}
}
