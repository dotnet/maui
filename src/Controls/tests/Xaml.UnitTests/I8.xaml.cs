using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class I8 : ContentPage
{
	public long l0 { get; set; }
	public long l1 { get; set; }
	public long l2 { get; set; }
	public long l3 { get; set; }
	public long l4 { get; set; }
	public long l5 { get; set; }
	public long l6 { get; set; }
	public long l7 { get; set; }
	public long l8 { get; set; }
	public long l9 { get; set; }
	public ulong ul0 { get; set; }
	public ulong ul1 { get; set; }
	public ulong ul2 { get; set; }
	public ulong ul3 { get; set; }
	public ulong ul4 { get; set; }
	public ulong ul5 { get; set; }

	public I8() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void I8AreConverted([Values] XamlInflator inflator)
		{
			var p = new I8(inflator);
			Assert.AreEqual(0L, p.l0);
			Assert.AreEqual((long)int.MaxValue, p.l1);
			Assert.AreEqual((long)uint.MaxValue, p.l2);
			Assert.AreEqual(long.MaxValue, p.l3);
			Assert.AreEqual((long)-int.MaxValue, p.l4);
			Assert.AreEqual((long)-uint.MaxValue, p.l5);
			Assert.AreEqual(-long.MaxValue, p.l6);
			Assert.AreEqual((long)256, p.l7);
			Assert.AreEqual((long)-256, p.l8);
			Assert.AreEqual((long)127, p.l9);
			Assert.AreEqual(0L, p.ul0);
			Assert.AreEqual((long)int.MaxValue, p.ul1);
			Assert.AreEqual((long)uint.MaxValue, p.ul2);
			Assert.AreEqual(long.MaxValue, p.ul3);
			Assert.AreEqual(ulong.MaxValue, p.ul4);
			Assert.AreEqual((ulong)256, p.ul5);
		}
	}
}