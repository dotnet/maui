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

#if NETSTANDARD2_0
		/// <summary>
		/// This interface method is provided as a stub for .NET Standard
		/// </summary>
		new Size CrossPlatformMeasure(double widthConstraint, double heightConstraint);

		/// <summary>
		/// This interface method is provided as a stub for .NET Standard
		/// </summary>
		new Size CrossPlatformArrange(Rect bounds);
#else
		/// <summary>
		/// This implementation is provided as a bridge for previous versions. Implementing classes should implement 
		/// the ICrossPlatformLayout interface rather than directly implementing this method.
		/// </summary>
		new Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return (this as ICrossPlatformLayout).CrossPlatformMeasure(widthConstraint, heightConstraint);
		}

		/// <summary>
		/// This implementation is provided as a bridge for previous versions. Implementing classes should implement 
		/// the ICrossPlatformLayout interface rather than directly implementing this method.
		/// </summary>
		new Size CrossPlatformArrange(Rect bounds)
		{
			return (this as ICrossPlatformLayout).CrossPlatformArrange(bounds);
		}
#endif
	}
}
