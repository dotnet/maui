namespace Microsoft.Maui.Dispatching
{
	/// <summary>
	/// The default service provider does not support a single service type for
	/// BOTH a singleton (for the root app) AND a scoped (for the window scope).
	/// This is a small wrapper so we can do the same thing. The preferred way is
	/// actually a keyed service, but this is a new feature that existing factories
	/// may not yet support. Also, this wrapper is not public so it is hard to 
	/// replace/substitute in tests.
	/// 
	/// TODO: Remove in net9 and require a keyed service - or some other way.
	/// </summary>
	internal class ApplicationDispatcher
	{
		public IDispatcher Dispatcher { get; }

		public ApplicationDispatcher(IDispatcher dispatcher)
		{
			Dispatcher = dispatcher;
		}
	}
}