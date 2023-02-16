namespace Microsoft.Maui.Animations
{
	/// <summary>
	/// This class is responsible for controlling the animations in .NET MAUI.
	/// </summary>
	public interface IAnimationManager
	{
		/// <summary>
		/// The ticker that is used by this manager to time the animations.
		/// </summary>
		ITicker Ticker { get; }

		/// <summary>
		/// Specifies a factor with which the animations for this manager should be multiplied.
		/// </summary>
		double SpeedModifier { get; set; }

		/// <summary>
		/// Specifies whether or not to automatically start the animation ticker for this manager.
		/// </summary>
		bool AutoStartTicker { get; set; }

		/// <summary>
		/// Adds the given animation to this manager. 
		/// </summary>
		/// <param name="animation">The animation to be added.</param>
		void Add(Animation animation);

		/// <summary>
		/// Removes the given animation from this manager. 
		/// </summary>
		/// <param name="animation">The animation to be removed.</param>
		void Remove(Animation animation);
	}
}