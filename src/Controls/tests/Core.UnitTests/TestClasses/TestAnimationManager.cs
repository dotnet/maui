using Microsoft.Maui.Animations;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class TestAnimationManager : AnimationManager
	{
		public TestAnimationManager(ITicker ticker = null) 
			: base(ticker ?? new BlockingTicker())
		{
		}

		internal override double AdjustSpeed(double elapsedMilliseconds)
		{
			// Regulate the speed so the tests are predictable
			return 16;
		}
	}
}