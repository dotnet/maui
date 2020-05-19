using System.Threading.Tasks;

namespace System.Maui.Controls
{
	public sealed class FailImageSource : ImageSource
	{
		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}
	}
}
