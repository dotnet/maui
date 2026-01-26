#if ANDROID
using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

[Category("GeminiNanoChatClient")]
public class GeminiNanoChatClientCancellationTests : ChatClientCancellationTestsBase<GeminiNanoChatClient>
{
}

[Category("GeminiNanoChatClient")]
public class GeminiNanoChatClientGetServiceTests : ChatClientGetServiceTestsBase<GeminiNanoChatClient>
{
	protected override string ExpectedProviderName => "google";
	protected override string ExpectedDefaultModelId => "gemini-nano";
}

[Category("GeminiNanoChatClient")]
public class GeminiNanoChatClientInstantiationTests : ChatClientInstantiationTestsBase<GeminiNanoChatClient>
{
}

[Category("GeminiNanoChatClient")]
public class GeminiNanoChatClientMessagesTests : ChatClientMessagesTestsBase<GeminiNanoChatClient>
{
}

[Category("GeminiNanoChatClient")]
public class GeminiNanoChatClientOptionsTests : ChatClientOptionsTestsBase<GeminiNanoChatClient>
{
}

[Category("GeminiNanoChatClient")]
public class GeminiNanoChatClientResponseTests : ChatClientResponseTestsBase<GeminiNanoChatClient>
{
}

[Category("GeminiNanoChatClient")]
public class GeminiNanoChatClientStreamingTests : ChatClientStreamingTestsBase<GeminiNanoChatClient>
{
}

#endif
