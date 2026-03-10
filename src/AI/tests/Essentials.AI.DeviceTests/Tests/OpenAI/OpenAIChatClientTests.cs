#if ENABLE_OPENAI_CLIENT

using Microsoft.Extensions.AI;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

public class OpenAIChatClient : DelegatingChatClient
{
	public OpenAIChatClient()
		: base(IPlatformApplication.Current!.Services.GetRequiredService<OpenAI.Chat.ChatClient>().AsIChatClient())
	{
	}
}

[Category(Traits.OpenAIChatClient)]
public class OpenAIChatClientCancellationTests : ChatClientCancellationTestsBase<OpenAIChatClient>
{
}

[Category(Traits.OpenAIChatClient)]
public class OpenAIChatClientFunctionCallingTestsBase : ChatClientFunctionCallingTestsBase<OpenAIChatClient>
{
	protected override IChatClient EnableFunctionCalling(OpenAIChatClient client)
	{
		return client.AsBuilder()
			.UseFunctionInvocation()
			.Build();
	}
}

[Category(Traits.OpenAIChatClient)]
public class OpenAIChatClientGetServiceTests : ChatClientGetServiceTestsBase<OpenAIChatClient>
{
	protected override string ExpectedProviderName => "openai";
	protected override string ExpectedDefaultModelId => "gpt-4o";
}

[Category(Traits.OpenAIChatClient)]
public class OpenAIChatClientInstantiationTests : ChatClientInstantiationTestsBase<OpenAIChatClient>
{
}

[Category(Traits.OpenAIChatClient)]
public class OpenAIChatClientMessagesTests : ChatClientMessagesTestsBase<OpenAIChatClient>
{
}

[Category(Traits.OpenAIChatClient)]
public class OpenAIChatClientOptionsTests : ChatClientOptionsTestsBase<OpenAIChatClient>
{
}

[Category(Traits.OpenAIChatClient)]
public class OpenAIChatClientResponseTests : ChatClientResponseTestsBase<OpenAIChatClient>
{
}

[Category(Traits.OpenAIChatClient)]
public class OpenAIChatClientStreamingTests : ChatClientStreamingTestsBase<OpenAIChatClient>
{
}

[Category(Traits.OpenAIChatClient)]
public class OpenAIChatClientJsonSchemaTests : ChatClientJsonSchemaTestsBase<OpenAIChatClient>
{
}

#endif
