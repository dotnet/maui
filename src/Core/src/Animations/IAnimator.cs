namespace Microsoft.Maui.Animations
{
	/// <summary>
	/// Objects implementing the <see cref="IAnimator"/> interface can act as parent objects for <see cref="Animation"/> objects.
	/// </summary>
	public interface IAnimator
	{
		/// <summary>
		/// Add an <see cref="Animation"/> object to this element.
		/// </summary>
		/// <param name="animation">The animation to be added.</param>
		void AddAnimation(Animation animation);

		/// <summary>
		/// Removes an <see cref="Animation"/> object from this element.
		/// </summary>
		/// <param name="animation">The animation to be removed.</param>
		void RemoveAnimation(Animation animation);
	}
}