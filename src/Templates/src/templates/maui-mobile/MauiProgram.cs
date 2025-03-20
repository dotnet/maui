#if (IncludeSampleContent)
using CommunityToolkit.Maui;
#endif
using Microsoft.Extensions.Logging;
#if (IncludeSampleContent)
using Syncfusion.Maui.Toolkit.Hosting;
#endif

namespace MauiApp._1;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
#if (IncludeSampleContent)
			.UseMauiCommunityToolkit()
			.ConfigureSyncfusionToolkit()
			.ConfigureMauiHandlers(handlers =>
			{
//-:cnd:noEmit
#if IOS || MACCATALYST
				handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
#endif
//+:cnd:noEmit
			})
#endif
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
#if (IncludeSampleContent)
				fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
				fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
#endif
			});

//-:cnd:noEmit
#if DEBUG
		builder.Logging.AddDebug();
//+:cnd:noEmit
#if (IncludeSampleContent)
		builder.Services.AddLogging(configure => configure.AddDebug());
#endif
//-:cnd:noEmit
#endif
//+:cnd:noEmit

#if (IncludeSampleContent)
		builder.Services.AddSingleton<ProjectRepository>();
		builder.Services.AddSingleton<TaskRepository>();
		builder.Services.AddSingleton<CategoryRepository>();
		builder.Services.AddSingleton<TagRepository>();
		builder.Services.AddSingleton<SeedDataService>();
		builder.Services.AddSingleton<ModalErrorHandler>();
		builder.Services.AddSingleton<MainPageModel>();
		builder.Services.AddSingleton<ProjectListPageModel>();
		builder.Services.AddSingleton<ManageMetaPageModel>();

		builder.Services.AddTransientWithShellRoute<ProjectDetailPage, ProjectDetailPageModel>("project");
		builder.Services.AddTransientWithShellRoute<TaskDetailPage, TaskDetailPageModel>("task");
		
#endif
		return builder.Build();
	}
}
