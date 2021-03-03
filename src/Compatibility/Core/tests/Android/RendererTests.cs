using System;
using System.Threading.Tasks;
using Android.Views;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android.UnitTests
{
	[TestFixture]
	public class RendererTests : PlatformTestFixture
	{
		[Test, Category("Entry")]
		[Description("Validate renderer vertical alignment for Entry with VerticalTextAlignment Center")]
		public async Task EntryVerticalAlignmentCenterInRenderer()
		{
			var entry1 = new Entry { Text = "foo", VerticalTextAlignment = TextAlignment.Center };
			var gravity = await GetControlProperty(entry1, control => control.Gravity);
			var centeredVertical = (gravity & GravityFlags.VerticalGravityMask) == GravityFlags.CenterVertical;
			Assert.That(centeredVertical, Is.True);
		}

		[Test, Category("CollectionView")]
		[Description("EmtpySource should have a count of zero")]
		public void EmptySourceCountIsZero() 
		{
			var emptySource = new EmptySource();
			Assert.That(emptySource.Count, Is.Zero);
		}
	}
}
