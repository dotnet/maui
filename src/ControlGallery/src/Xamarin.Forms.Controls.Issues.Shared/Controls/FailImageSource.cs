using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public sealed class FailImageSource : ImageSource
	{
		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}
	}
}
