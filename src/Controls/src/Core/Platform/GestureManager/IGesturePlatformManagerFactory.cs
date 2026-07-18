namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Application-wide factory for creating <see cref="IGesturePlatformManager"/> instances.
	/// Register an implementation in the application's <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>
	/// to replace the built-in platform gesture handling for every handler connection.
	/// </summary>
	/// <remarks>
	/// <para>
	/// When an <see cref="IGesturePlatformManagerFactory"/> is present in
	/// <see cref="Microsoft.Maui.IMauiContext.Services"/>, <see cref="GestureManager"/> calls
	/// <see cref="CreateGesturePlatformManager"/> instead of constructing the default
	/// <c>GesturePlatformManager</c>. This lets alternative platform backends (for example,
	/// community-maintained backends that do not use <c>IPlatformViewHandler</c>)
	/// supply their own gesture infrastructure without subclassing or forking the built-in types.
	/// </para>
	/// <para>
	/// The factory takes precedence over a handler-scoped <see cref="IGesturePlatformManagerProvider"/>.
	/// When both are present, the factory wins.
	/// </para>
	/// <example>
	/// Registering a custom factory:
	/// <code lang="csharp">
	/// builder.Services.AddSingleton&lt;IGesturePlatformManagerFactory, MyGestureFactory&gt;();
	/// </code>
	/// </example>
	/// </remarks>
	public interface IGesturePlatformManagerFactory
	{
		/// <summary>
		/// Creates a new <see cref="IGesturePlatformManager"/> for the supplied handler connection.
		/// </summary>
		/// <param name="handler">The handler whose platform view the gestures are attached to.</param>
		/// <returns>
		/// A new <see cref="IGesturePlatformManager"/> instance. This instance is owned and disposed by
		/// <see cref="GestureManager"/>. A new instance must be returned for each call because
		/// <see cref="GestureManager"/> disposes and recreates the manager on every connect or handler change.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// Thrown when <paramref name="handler"/> is <see langword="null"/>.
		/// </exception>
		IGesturePlatformManager CreateGesturePlatformManager(IViewHandler handler);
	}
}
