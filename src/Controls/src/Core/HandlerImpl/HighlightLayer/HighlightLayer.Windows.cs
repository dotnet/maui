#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Color = Windows.UI.Color;
namespace Microsoft.Maui.Controls
{
	public partial class HighlightLayer
	{
		HashSet<WindowsView> Views = new HashSet<WindowsView>();

		public bool AddHighlight(Maui.IView view)
		{
			var nativeView = view.GetNative(true);
			if (nativeView == null)
				return false;
			var wv = new WindowsView(view, nativeView, GetDefaultColor(nativeView));
			this.SetColor(wv, true);
			return this.Views.Add(wv);
		}

		public bool RemoveHighlight(Maui.IView view)
		{
			var aview = this.Views.FirstOrDefault(n => n.view == view);
			if (aview != null)
			{
				this.SetColor(aview, false);
				return this.Views.Remove(aview);
			}
			return false;
		}

		public void ClearHighlights()
		{
			foreach (var selectionLayer in Views)
			{
				this.SetColor(selectionLayer, false);
			}
			this.Views.Clear();
		}

		private UI.Xaml.Media.Brush? GetDefaultColor(FrameworkElement element)
		{
			var taskCompletionSource = new TaskCompletionSource<UI.Xaml.Media.Brush?>();

			element.DispatcherQueue.TryEnqueue(new DispatcherQueueHandler(() =>
			{
				try
				{
					if (element is ContentControl cc)
					{
						taskCompletionSource.SetResult(cc.Background);
					}
					else if (element is Panel panel)
					{
						taskCompletionSource.SetResult(panel.Background);
					}
					else
					{
						taskCompletionSource.SetResult(element.GetForeground());
					}
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.ToString());
					taskCompletionSource.SetResult(null);
				}
			}));

			return taskCompletionSource.Task.Result;
		}

		private void SetColor(WindowsView view, bool highlightColor)
		{
			view.highlightedView.DispatcherQueue.TryEnqueue(() =>
			{
				UI.Xaml.Media.Brush? brush = !highlightColor ? view.highlightedViewOriginalBackground : new UI.Xaml.Media.SolidColorBrush(Color.FromArgb(122, 225, 0, 0));
				if (view.highlightedView is ContentControl cc)
				{
					cc.UpdateBackground(paint: null, defaultBrush: brush);
				}
				else if (view.highlightedView is Panel panel)
				{
					panel.UpdateBackground(paint: null, defaultBrush: brush);
				}
				else
				{
					try
					{
						if (brush == null)
							brush = new UI.Xaml.Media.SolidColorBrush(Color.FromArgb(225, 225, 225, 225));
						view.highlightedView.SetForeground(brush);
					}
					catch (Exception)
					{
					}
				}
			});
		}

		internal class WindowsView
		{
			public Maui.IView view;
			public FrameworkElement highlightedView;
			public UI.Xaml.Media.Brush? highlightedViewOriginalBackground;

			public WindowsView(Maui.IView view, FrameworkElement uiview, UI.Xaml.Media.Brush? background)
			{
				this.view = view;
				this.highlightedView = uiview;
				this.highlightedViewOriginalBackground = background;
			}
		}
	}
}
