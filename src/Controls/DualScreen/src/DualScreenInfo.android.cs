using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Maui.Foldable
{
	public partial class DualScreenInfo : INotifyPropertyChanged
	{
		static object hingeAngleLock = new object();
		public Task<int> GetHingeAngleAsync() => DualScreenService.GetHingeAngleAsync();

		void ProcessHingeAngleSubscriberCount(int newCount)
		{
			lock (hingeAngleLock)
			{
				if (newCount == 1)
				{
					Foldable.FoldableService.FoldableServiceImpl.HingeAngleChanged += OnHingeAngleChanged;
				}
				else if (newCount == 0)
				{
					Foldable.FoldableService.FoldableServiceImpl.HingeAngleChanged -= OnHingeAngleChanged;
				}
			}
		}

		void OnHingeAngleChanged(object sender, HingeSensor.HingeSensorChangedEventArgs e)
		{
			_hingeAngleChanged?.Invoke(this, new HingeAngleChangedEventArgs(e.HingeAngle));
		}
	}
}
