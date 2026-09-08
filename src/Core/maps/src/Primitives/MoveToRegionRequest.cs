namespace Microsoft.Maui.Maps
{
	/// <summary>
	/// Represents a request to move the map to a specific region, with optional animation control.
	/// </summary>
	public sealed class MoveToRegionRequest
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MoveToRegionRequest"/> class.
		/// </summary>
		/// <param name="region">The target region.</param>
		/// <param name="animated">Whether the transition should be animated.</param>
		public MoveToRegionRequest(MapSpan? region, bool animated)
		{
			Region = region;
			Animated = animated;
		}

		/// <summary>
		/// Gets the target region.
		/// </summary>
		public MapSpan? Region { get; }

		/// <summary>
		/// Gets whether the transition should be animated.
		/// </summary>
		public bool Animated { get; }
	}
}
