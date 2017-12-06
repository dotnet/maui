using ElmSharp;
using System;
using ELayout = ElmSharp.Layout;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class EditfieldEntry : Native.Entry, IMeasurable
	{
		ELayout _editfieldLayout;
		int _heightPadding = 0;

		public EditfieldEntry(EvasObject parent) : base(parent)
		{
		}

		protected override IntPtr CreateHandle(EvasObject parent)
		{
			var bg = new ELayout(parent);
			bg.SetTheme("layout", "background", "default");
			_editfieldLayout = new ELayout(parent);
			_editfieldLayout.SetTheme("layout", "editfield", "singleline");

			var handle = base.CreateHandle(parent);

			// If true, It means, there is no extra layout on the widget handle
			// We need to set RealHandle, becuase we replace Handle to Layout
			if (RealHandle == IntPtr.Zero)
			{
				RealHandle = handle;
			}
			Handle = handle;

			_editfieldLayout.SetPartContent("elm.swallow.content", this);
			bg.SetPartContent("elm.swallow.content", _editfieldLayout);

			// The minimun size for the Content area of an Editfield. This is used to calculate the size when layouting.
			_heightPadding = _editfieldLayout.EdjeObject["elm.swallow.content"].Geometry.Height;
			return bg;
		}

		public new ElmSharp.Size Measure(int availableWidth, int availableHeight)
		{
			var textBlockSize = base.Measure(availableWidth, availableHeight);

			// Calculate the minimum size by adding the width of a TextBlock and an Editfield.
			textBlockSize.Width += _editfieldLayout.MinimumWidth;

			// If the height of a TextBlock is shorter than Editfield, use the minimun height of the Editfield.
			// Or add the height of the EditField to the TextBlock
			if (textBlockSize.Height < _editfieldLayout.MinimumHeight)
				textBlockSize.Height = _editfieldLayout.MinimumHeight;
			else
				textBlockSize.Height += _heightPadding;

			return textBlockSize;
		}
	}
}