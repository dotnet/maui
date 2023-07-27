using Microsoft.Maui.Graphics;

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

		/// <summary>
		/// This interface method is provided for backward compatibility with previous versions. 
		/// Implementing classes should implement the ICrossPlatformLayout interface rather than directly implementing this method.
		/// </summary>
		new Size CrossPlatformArrange(Rect bounds);

		/// <summary>
		/// This interface method is provided for backward compatibility with previous versions. 
		/// Implementing classes should implement the ICrossPlatformLayout interface rather than directly implementing this method.
		/// </summary>
		new Size CrossPlatformMeasure(double widthConstraint, double heightConstraint);

#if !NETSTANDARD2_0
		Size ICrossPlatformLayout.CrossPlatformArrange(Microsoft.Maui.Graphics.Rect bounds) => CrossPlatformArrange(bounds);
		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint) => CrossPlatformMeasure(widthConstraint, heightConstraint);
#endif
	}
}
