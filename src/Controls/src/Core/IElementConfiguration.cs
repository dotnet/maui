
namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Helper that handles storing and lookup of platform specifics implementations
	/// </summary>
	/// <typeparam name="TElement">The element type.</typeparam>
	public interface IElementConfiguration<out TElement> where TElement : Element
	{
		/// <summary>
		/// Returns the platform-specific instance of this <typeparamref name="TElement"/>, on which a platform-specific method may be called.
		/// </summary>
		/// <typeparam name="T">A type of <see cref="IConfigPlatform"/> which specifies the platform to retrieve for.</typeparam>
		/// <returns>A platform-specific instance of <typeparamref name="TElement"/>.</returns>
		IPlatformElementConfiguration<T, TElement> On<T>() where T : IConfigPlatform;
	}
}
