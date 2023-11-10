using System.Net;
using Microsoft.Extensions.Logging;
using VisualTestUtils.AppConnector.Controller;

public class TestAppController : AppConnectorController
{
	public TestAppController(IPAddress ipAddress, int port = 4243, ILogger? logger = null)
		: base(ipAddress, port, logger)
	{
	}
}
