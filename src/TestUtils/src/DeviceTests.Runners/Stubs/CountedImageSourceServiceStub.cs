using System.Threading;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class CountedImageSourceServiceStub : IImageSourceService<ICountedImageSourceStub>
	{
		/// <summary>
		/// The service is wanting to start.
		/// </summary>
		public AutoResetEvent Starting { get; } = new AutoResetEvent(false);

		/// <summary>
		/// Used to indicate that the service is permitted to actually start.
		/// </summary>
		public AutoResetEvent DoWork { get; } = new AutoResetEvent(false);

		/// <summary>
		/// The service is finished and about to return.
		/// </summary>
		public AutoResetEvent Finishing { get; } = new AutoResetEvent(false);
	}
}