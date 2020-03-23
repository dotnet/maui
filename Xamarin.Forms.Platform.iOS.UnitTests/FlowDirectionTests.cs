using NUnit.Framework;
using UIKit;

namespace Xamarin.Forms.Platform.iOS.UnitTests
{
	[TestFixture]
	public class FlowDirectionTests
	{
		[Test]
		public void FlowDirectionConversion() 
		{
			var ltr = UIUserInterfaceLayoutDirection.LeftToRight.ToFlowDirection();
			Assert.That(ltr, Is.EqualTo(FlowDirection.LeftToRight));

			var rtl = UIUserInterfaceLayoutDirection.RightToLeft.ToFlowDirection();
			Assert.That(rtl, Is.EqualTo(FlowDirection.RightToLeft));
		}
	}
}