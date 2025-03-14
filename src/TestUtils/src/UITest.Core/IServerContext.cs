namespace UITest.Core
{
	public interface IServerContext : IDisposable
	{
		IUIClientContext CreateUIClientContext(IConfig config);
		bool IsServerRunning { get; }
	}
}
