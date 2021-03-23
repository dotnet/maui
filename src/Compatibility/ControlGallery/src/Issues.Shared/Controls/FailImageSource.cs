using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public sealed class FailImageSource : ImageSource
	{
		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}
	}
}
