using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, object>
	{
		protected override object CreateNativeElement() => throw new NotImplementedException();

		public static void MapRequestTerminate(ApplicationHandler handler, IApplication application, object? args) { }
	}
}