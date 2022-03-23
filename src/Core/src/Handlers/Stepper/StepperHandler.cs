#if IOS || MACCATALYST
using PlatformView = UIKit.UIStepper;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif NETSTANDARD || (NET6_0 && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : IStepperHandler
	{
		public static IPropertyMapper<IStepper, IStepperHandler> Mapper = new PropertyMapper<IStepper, StepperHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IStepper.Interval)] = MapIncrement,
			[nameof(IStepper.Maximum)] = MapMaximum,
			[nameof(IStepper.Minimum)] = MapMinimum,
			[nameof(IStepper.Value)] = MapValue,
#if WINDOWS
			[nameof(IStepper.Background)] = MapBackground,
#endif
		};

		public static CommandMapper<IStepper, IStepperHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public StepperHandler() : base(Mapper)
		{
		}

		public StepperHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
		{
		}

		IStepper IStepperHandler.VirtualView => VirtualView;

		PlatformView IStepperHandler.PlatformView => PlatformView;
	}
}