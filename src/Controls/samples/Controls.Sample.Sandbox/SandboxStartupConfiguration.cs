namespace Maui.Controls.Sample;

static class SandboxStartupConfiguration
{
	public static int UpdateRounds
	{
		get
		{
#if STARTUP_UPDATE_ROUNDS_1
			return 1;
#elif STARTUP_UPDATE_ROUNDS_5
			return 5;
#elif STARTUP_UPDATE_ROUNDS_20
			return 20;
#elif STARTUP_UPDATE_ROUNDS_100
			return 100;
#else
			return 0;
#endif
		}
	}

	public static string Scenario =>
		UpdateRounds == 0 ? "natural" : $"connected-update-{UpdateRounds}";
}
