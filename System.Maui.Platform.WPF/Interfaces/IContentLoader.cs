namespace System.Maui.Platform.WPF.Interfaces
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Windows;

	public interface IContentLoader
	{
		Task<object> LoadContentAsync(FrameworkElement parent, object oldContent, object newContent, CancellationToken cancellationToken);

		void OnSizeContentChanged(FrameworkElement parent, object content);
	}

	public class DefaultContentLoader : IContentLoader
	{
		public Task<object> LoadContentAsync(FrameworkElement parent, object oldContent, object newContent, CancellationToken cancellationToken)
		{
			if (!Application.Current.Dispatcher.CheckAccess())
				throw new InvalidOperationException("UIThreadRequired");

			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			return Task.Factory.StartNew(() => LoadContent(newContent), cancellationToken, TaskCreationOptions.None, scheduler);
		}

		protected virtual object LoadContent(object content)
		{
			if (content is FrameworkElement)
				return content;

			if (content is Uri)
				return Application.LoadComponent(content as Uri);

			if (content is string)
			{
				if (Uri.TryCreate(content as string, UriKind.RelativeOrAbsolute, out Uri uri))
				{
					return Application.LoadComponent(uri);
				}
			}

			return null;
		}

		public void OnSizeContentChanged(FrameworkElement parent, object page)
		{

		}
	}
}