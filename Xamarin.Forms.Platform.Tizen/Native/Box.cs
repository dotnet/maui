using System;
using ElmSharp;
using EBox = ElmSharp.Box;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Extends the ElmSharp.Box class with functionality useful to Xamarin.Forms renderer.
	/// </summary>
	/// <remarks>
	/// This class overrides the layout mechanism. Instead of using the native layout,
	/// <c>LayoutUpdated</c> event is sent.
	/// </remarks>
	public class Box : EBox
	{
		/// <summary>
		/// The last processed geometry of the Box which was reported from the native layer.
		/// </summary>
		Rect _previousGeometry;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xamarin.Forms.Platform.Tizen.Native.Box"/> class.
		/// </summary>
		/// <param name="parent">The parent EvasObject.</param>
		public Box(EvasObject parent) : base(parent)
		{
			SetLayoutCallback(() => { NotifyOnLayout(); });
		}

		/// <summary>
		/// Notifies that the layout has been updated.
		/// </summary>
		public event EventHandler<LayoutEventArgs> LayoutUpdated;

		/// <summary>
		/// Triggers the <c>LayoutUpdated</c> event.
		/// </summary>
		/// <remarks>
		/// This method is called whenever there is a possibility that the size and/or position has been changed.
		/// </remarks>
		void NotifyOnLayout()
		{
			if (null != LayoutUpdated)
			{
				var g = Geometry;

				if (0 == g.Width || 0 == g.Height || g == _previousGeometry)
				{
					// ignore irrelevant dimensions
					return;
				}

				LayoutUpdated(this, new LayoutEventArgs()
					{
						Geometry = g,
					}
				);

				_previousGeometry = g;
			}
		}
	}
}
