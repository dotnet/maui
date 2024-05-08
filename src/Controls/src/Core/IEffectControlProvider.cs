#nullable disable
namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides the functionality to register an <see cref="Effect"/> to an element.
	/// </summary>
	public interface IEffectControlProvider
	{
		/// <summary>
		/// Registers the specified <paramref name="effect"/> to an element.
		/// </summary>
		/// <param name="effect">The effect to be registered.</param>
		void RegisterEffect(Effect effect);
	}
}