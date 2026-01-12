#if IOS || MACCATALYST

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientCancellationTests : ChatClientCancellationTestsBase<AppleIntelligenceChatClient>
{
}

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientFunctionCallingTestsBase : ChatClientFunctionCallingTestsBase<AppleIntelligenceChatClient>
{
}

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientGetServiceTests : ChatClientGetServiceTestsBase<AppleIntelligenceChatClient>
{
	protected override string ExpectedProviderName => "apple";
	protected override string ExpectedDefaultModelId => "apple-intelligence";
}

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientInstantiationTests : ChatClientInstantiationTestsBase<AppleIntelligenceChatClient>
{
}

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientMessagesTests : ChatClientMessagesTestsBase<AppleIntelligenceChatClient>
{
}

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientOptionsTests : ChatClientOptionsTestsBase<AppleIntelligenceChatClient>
{
}

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientResponseTests : ChatClientResponseTestsBase<AppleIntelligenceChatClient>
{
}

[Category("AppleIntelligenceChatClient")]
public class AppleIntelligenceChatClientStreamingTests : ChatClientStreamingTestsBase<AppleIntelligenceChatClient>
{
}

#endif
