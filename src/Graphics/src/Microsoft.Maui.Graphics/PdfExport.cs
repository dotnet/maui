namespace Microsoft.Maui.Graphics
{
	public static class PdfExport
	{
		private static IPdfExportService _registeredExportService;

		public static IPdfExportService CurrentExportService => _registeredExportService;

		public static void RegisterService(IPdfExportService exportService)
		{
			_registeredExportService = exportService;
		}

		public static PdfExportContext CreateContext(float width = -1, float height = -1)
		{
			return CurrentExportService.CreateContext(width, height);
		}
	}
}
