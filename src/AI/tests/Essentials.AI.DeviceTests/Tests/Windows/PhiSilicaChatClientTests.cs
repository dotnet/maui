#if WINDOWS
using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientCancellationTests : ChatClientCancellationTestsBase<PhiSilicaChatClient>
{
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientGetServiceTests : ChatClientGetServiceTestsBase<PhiSilicaChatClient>
{
	protected override string ExpectedProviderName => "windows";
	protected override string ExpectedDefaultModelId => "phi-silica";
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientInstantiationTests : ChatClientInstantiationTestsBase<PhiSilicaChatClient>
{
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientMessagesTests : ChatClientMessagesTestsBase<PhiSilicaChatClient>
{
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientOptionsTests : ChatClientOptionsTestsBase<PhiSilicaChatClient>
{
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientResponseTests : ChatClientResponseTestsBase<PhiSilicaChatClient>
{
}

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientStreamingTests : ChatClientStreamingTestsBase<PhiSilicaChatClient>
{
}

#endif
