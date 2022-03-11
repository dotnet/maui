using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui.Foldable
{
	public partial class DualScreenInfo : INotifyPropertyChanged
	{
		public Task<int> GetHingeAngleAsync() => DualScreenService.GetHingeAngleAsync();

		void ProcessHingeAngleSubscriberCount(int newCount) { }
	}
}
