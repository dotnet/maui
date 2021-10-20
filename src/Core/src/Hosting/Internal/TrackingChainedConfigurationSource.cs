using Microsoft.Extensions.Configuration;

namespace Microsoft.Maui.Hosting
{
	internal sealed class TrackingChainedConfigurationSource : IConfigurationSource
	{
		private readonly ChainedConfigurationSource _chainedConfigurationSource = new();

		public TrackingChainedConfigurationSource(ConfigurationManager configManager)
		{
			_chainedConfigurationSource.Configuration = configManager;
		}

		public IConfigurationProvider? BuiltProvider { get; set; }

		public IConfigurationProvider Build(IConfigurationBuilder builder)
		{
			BuiltProvider = _chainedConfigurationSource.Build(builder);
			return BuiltProvider;
		}
	}
}
