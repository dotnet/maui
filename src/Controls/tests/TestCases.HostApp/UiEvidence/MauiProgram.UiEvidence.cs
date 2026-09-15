#if MAUI_UI_EVIDENCE
using Microsoft.Maui.Hosting;
#if MAUI_UI_EVIDENCE_DEVFLOW
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DevFlow.Agent.Core;
using Microsoft.Maui.LifecycleEvents;
#endif

namespace Maui.Controls.Sample;

public static partial class MauiProgram
{
	static partial void ConfigureUiEvidence(MauiAppBuilder builder)
	{
#if MAUI_UI_EVIDENCE_DEVFLOW
		if (Environment.GetEnvironmentVariable("MAUI_UI_EVIDENCE_DISABLE_DEVFLOW") == "1")
			return;

		var service = new DevFlowAgentService(new AgentOptions
		{
			Port = 9223,
			EnableLayoutDiagnostics = true
		});
		builder.Services.AddSingleton(service);

		void EnsureStarted()
		{
			var app = Application.Current;
			if (app is null || service.IsRunning)
				return;

			app.Dispatcher.Dispatch(() => service.Start(app, app.Dispatcher));
		}

		builder.ConfigureLifecycleEvents(lifecycle =>
		{
#if ANDROID
			lifecycle.AddAndroid(android => android.OnResume(_ => EnsureStarted()));
#elif WINDOWS
			lifecycle.AddWindows(windows => windows.OnActivated((_, _) => EnsureStarted()));
#endif
		});
#endif
	}

	static partial void OverrideMainPage(ref Page mainPage)
	{
#if MAUI_UI_EVIDENCE_LAYOUT_CONTROLS
		mainPage = new UiEvidenceLayoutControlsPage();
#elif MAUI_UI_EVIDENCE_COLLECTIONVIEW
		mainPage = new UiEvidenceCollectionViewPage();
#else
#error A UI evidence scenario constant must be selected.
#endif
	}
}
#endif
