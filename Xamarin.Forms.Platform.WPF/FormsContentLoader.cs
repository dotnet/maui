using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfLightToolkit.Interfaces;

namespace Xamarin.Forms.Platform.WPF
{
	public class FormsContentLoader : IContentLoader
	{
		public Task<object> LoadContentAsync(FrameworkElement parent, object oldContent, object newContent, CancellationToken cancellationToken)
		{
			VisualElement element = oldContent as VisualElement;
			if (element != null)
			{
				element.Cleanup(); // Cleanup old content
			}

			if (!System.Windows.Application.Current.Dispatcher.CheckAccess())
				throw new InvalidOperationException("UIThreadRequired");
			
			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			return Task.Factory.StartNew(() => LoadContent(parent, newContent), cancellationToken, TaskCreationOptions.None, scheduler);
		}
		
		protected virtual object LoadContent(FrameworkElement parent, object page)
		{
			VisualElement visualElement = page as VisualElement;
			if (visualElement != null)
			{
				var renderer = CreateOrResizeContent(parent, visualElement);
				return renderer;
			}
			return null;
		}

		public void OnSizeContentChanged(FrameworkElement parent, object page)
		{
			VisualElement visualElement = page as VisualElement;
			if (visualElement != null)
			{
				CreateOrResizeContent(parent, visualElement);
			}
		}

		private object CreateOrResizeContent(FrameworkElement parent, VisualElement visualElement)
		{
			var renderer = Platform.GetOrCreateRenderer(visualElement);

			//if (Debugger.IsAttached)
			//	Console.WriteLine("Page type : " + visualElement.GetType() + " (" + (visualElement as Page).Title + ") -- Parent type : " + visualElement.Parent.GetType() + " -- " + parent.ActualHeight + "H*" + parent.ActualWidth + "W");

			visualElement.Layout(new Rectangle(0, 0, parent.ActualWidth, parent.ActualHeight));

			IPageController pageController = visualElement.RealParent as IPageController;
			if (pageController != null)
				pageController.ContainerArea = new Rectangle(0, 0, parent.ActualWidth, parent.ActualHeight);
				
			return renderer.GetNativeElement();
		}
	}
}
