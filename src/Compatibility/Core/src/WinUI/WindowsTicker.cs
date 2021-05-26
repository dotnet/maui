using Windows.ApplicationModel.Core;
using System;
using Microsoft.UI.Xaml.Media;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal class WindowsTicker : Ticker
	{
		[ThreadStatic]
		static Ticker s_ticker;

		protected override void DisableTimer()
		{
			CompositionTarget.Rendering -= RenderingFrameEventHandler;
		}

		protected override void EnableTimer()
		{
			CompositionTarget.Rendering += RenderingFrameEventHandler;
		}

		void RenderingFrameEventHandler(object sender, object args)
		{
			SendSignals();
		}

		protected override Ticker GetTickerInstance()
		{
			if (CoreApplication.Views.Count > 1)
			{
				// We've got multiple windows open, we'll need to use the local ThreadStatic Ticker instead of the 
				// singleton in the base class 
				if (s_ticker == null)
				{
					s_ticker = new WindowsTicker();
				}

				return s_ticker;
			}

			return base.GetTickerInstance();
		}
	}
}