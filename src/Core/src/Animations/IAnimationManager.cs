namespace Microsoft.Maui.Animations
{
	public interface IAnimationManager
	{
		ITicker Ticker { get; }

		double SpeedModifier { get; set; }

		bool AutoStartTicker { get; set; }

		void Add(Animation animation);

		void Remove(Animation animation);
	}
}