namespace Microsoft.Maui
{
	/// <summary>
	/// Provides the base properties and methods for all Layout elements.
	/// Use Layout elements to position and size child elements in .NET MAUI applications.
	/// </summary>
	public interface ILayout : IView, IContainer, ISafeAreaView, IPadding, ICrossPlatformLayout
	{
		/// <summary>
		/// Specifies whether the ILayout clips its content to its boundaries.
		/// </summary>
		bool ClipsToBounds { get; }
	}
}
