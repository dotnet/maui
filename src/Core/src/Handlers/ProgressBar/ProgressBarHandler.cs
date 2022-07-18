#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIProgressView;
#elif MONOANDROID
using PlatformView = Android.Widget.ProgressBar;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.ProgressBar;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID)
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : IProgressBarHandler
	{
		public static IPropertyMapper<IProgress, IProgressBarHandler> Mapper = new PropertyMapper<IProgress, ProgressBarHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IProgress.Progress)] = MapProgress,
			[nameof(IProgress.ProgressColor)] = MapProgressColor
		};

		public static CommandMapper<IPicker, IProgressBarHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public ProgressBarHandler() : base(Mapper)
		{
		}

		public ProgressBarHandler(IPropertyMapper? mapper = null) : base(mapper ?? Mapper)
		{
		}

		IProgress IProgressBarHandler.VirtualView => VirtualView;

		PlatformView IProgressBarHandler.PlatformView => PlatformView;
	}
}