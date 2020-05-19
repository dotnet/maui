using System;
using System.Maui;
using System.Maui.Platform.UWP;

[assembly: Dependency(typeof(DispatcherProvider))]
namespace System.Maui.Platform.UWP
{
	internal class DispatcherProvider : IDispatcherProvider
	{
		[ThreadStatic]
		static Dispatcher s_current;

		public IDispatcher GetDispatcher(object context)
		{
			if (s_current != null)
			{
				return s_current;			
			}

			if (context is BindableObject)
			{
				if (context is VisualElement element)
				{
					var renderer = Platform.GetRenderer(element);
					if (renderer?.ContainerElement != null)
					{
						s_current = new Dispatcher(renderer.ContainerElement.Dispatcher);
						return s_current;
					}
				}

				return null;
			}
			
			return s_current = s_current ?? new Dispatcher();
		}
	}
}
