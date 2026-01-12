#if IOS || MACCATALYST

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class AppleIntelligenceChatClientCancellationTests : ChatClientCancellationTestsBase<AppleIntelligenceChatClient>
{
	// All cancellation tests are inherited from base class
	// Add implementation-specific cancellation tests here if needed
}

public class AppleIntelligenceChatClientFunctionCallingTestsBase : ChatClientFunctionCallingTestsBase<AppleIntelligenceChatClient>
{
	// All function calling tests are inherited from base class
	// Add implementation-specific function calling tests here if needed
}

public class AppleIntelligenceChatClientGetServiceTests : ChatClientGetServiceTestsBase<AppleIntelligenceChatClient>
{
	protected override string ExpectedProviderName => "apple";
	protected override string ExpectedDefaultModelId => "apple-intelligence";

	// All GetService tests are inherited from base class
	// Add implementation-specific GetService tests here if needed
}

public class AppleIntelligenceChatClientInstantiationTests : ChatClientInstantiationTestsBase<AppleIntelligenceChatClient>
{
	// All instantiation tests are inherited from base class
	// Add implementation-specific instantiation tests here if needed
}

public class AppleIntelligenceChatClientMessagesTests : ChatClientMessagesTestsBase<AppleIntelligenceChatClient>
{
	// All message handling tests are inherited from base class
	// Add implementation-specific message tests here if needed
}

public class AppleIntelligenceChatClientOptionsTests : ChatClientOptionsTestsBase<AppleIntelligenceChatClient>
{
	// All options tests are inherited from base class
	// Add implementation-specific options tests here if needed
}

public class AppleIntelligenceChatClientResponseTests : ChatClientResponseTestsBase<AppleIntelligenceChatClient>
{
	// All response tests are inherited from base class
	// Add implementation-specific response tests here if needed
}

public class AppleIntelligenceChatClientStreamingTests : ChatClientStreamingTestsBase<AppleIntelligenceChatClient>
{
	// All streaming tests are inherited from base class
	// Add implementation-specific streaming tests here if needed
}

#endif
