#nullable enable
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WRect = global::Windows.Foundation.Rect;
using WSize = global::Windows.Foundation.Size;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Platform
{
	public partial class LayoutPanel : MauiPanel
	{
		Canvas? _backgroundLayer;
		public bool ClipsToBounds { get; set; }

		// TODO: Possibly reconcile this code with ViewHandlerExtensions.LayoutVirtualView
		// If you make changes here please review if those changes should also
		// apply to ViewHandlerExtensions.LayoutVirtualView
		protected override WSize ArrangeOverride(WSize finalSize)
		{
			var actual = base.ArrangeOverride(finalSize);

			if (!(Parent is ContentPanel contentPanel && contentPanel.BorderStroke?.Shape is not null))
			{
				Clip = ClipsToBounds ? new RectangleGeometry { Rect = new WRect(0, 0, finalSize.Width, finalSize.Height) } : null;
			}

			return actual;
		}

		public void UpdateInputTransparent(bool inputTransparent, Brush? background)
		{
			if (inputTransparent)
			{
				MakeInputTransparent(background);
			}
			else
			{
				MakeInputVisible(background);
			}
		}

		void MakeInputTransparent(Brush? background)
		{
			Background = null;

			if (background == null)
			{
				// If the background is null, we don't need the background layer
				RemoveBackgroundLayer();
			}
			else
			{
				// Add the background layer to handle the background brush
				AddBackgroundLayer();
				_backgroundLayer!.Background = background;
			}
		}

		void MakeInputVisible(Brush? background)
		{
			// If we aren't input transparent, we don't need the background layer hack 
			RemoveBackgroundLayer();

			if (background == null)
			{
				// We can't have a null background, because that would allow input through
				// So we'll make the background color transparent (visually the same as null, but consumes input)
				background = new WSolidColorBrush(UI.Colors.Transparent);
			}

			Background = background;
		}

		void AddBackgroundLayer()
		{
			// In WinUI, once a control has hit testing disabled, all of its child controls
			// also have hit testing disabled. The exception is a Panel with its 
			// Background Brush set to `null`; the Panel will be invisible to hit testing, but its
			// children will work just fine. 

			// In order to handle the situation where we need the layout to be invisible to hit testing,
			// the child controls to be visible to hit testing, *and* we need to support non-null
			// background brushes, we insert another empty Panel which is invisible to hit testing; that
			// Panel will be our Background brush

			if (_backgroundLayer != null)
			{
				return;
			}

			_backgroundLayer = new Canvas { IsHitTestVisible = false };
			CachedChildren.Insert(0, _backgroundLayer);
		}

		void RemoveBackgroundLayer()
		{
			if (_backgroundLayer == null)
			{
				return;
			}

			CachedChildren.Remove(_backgroundLayer);
			_backgroundLayer = null;
		}
	}
}
