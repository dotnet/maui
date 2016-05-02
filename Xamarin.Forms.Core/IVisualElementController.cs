using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public interface IVisualElementController : IElementController
	{
		void NativeSizeChanged();
		void InvalidateMeasure(InvalidationTrigger trigger);
	}
}