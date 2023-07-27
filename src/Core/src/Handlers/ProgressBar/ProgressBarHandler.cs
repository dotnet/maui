#nullable enable
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIProgressView;
#elif MONOANDROID
using PlatformView = Android.Widget.ProgressBar;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.ProgressBar;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.GraphicsView.ProgressBar;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
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

		public static CommandMapper<IProgress, IProgressBarHandler> CommandMapper = new(ViewCommandMapper)
		{
		};

		public ProgressBarHandler() : base(Mapper, CommandMapper)
		{
		}

		public ProgressBarHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public ProgressBarHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IProgress IProgressBarHandler.VirtualView => VirtualView;

		PlatformView IProgressBarHandler.PlatformView => PlatformView;
	}
}