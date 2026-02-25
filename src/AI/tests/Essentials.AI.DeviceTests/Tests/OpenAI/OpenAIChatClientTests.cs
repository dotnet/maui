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

[Category("OpenAIChatClient")]
public class OpenAIChatClientCancellationTests : ChatClientCancellationTestsBase<OpenAIChatClient>
{
}

[Category("OpenAIChatClient")]
public class OpenAIChatClientFunctionCallingTestsBase : ChatClientFunctionCallingTestsBase<OpenAIChatClient>
{
	protected override IChatClient EnableFunctionCalling(OpenAIChatClient client)
	{
		return client.AsBuilder()
			.UseFunctionInvocation()
			.Build();
	}
}

[Category("OpenAIChatClient")]
public class OpenAIChatClientGetServiceTests : ChatClientGetServiceTestsBase<OpenAIChatClient>
{
	protected override string ExpectedProviderName => "openai";
	protected override string ExpectedDefaultModelId => "gpt-4o";
}

[Category("OpenAIChatClient")]
public class OpenAIChatClientInstantiationTests : ChatClientInstantiationTestsBase<OpenAIChatClient>
{
}

[Category("OpenAIChatClient")]
public class OpenAIChatClientMessagesTests : ChatClientMessagesTestsBase<OpenAIChatClient>
{
}

[Category("OpenAIChatClient")]
public class OpenAIChatClientOptionsTests : ChatClientOptionsTestsBase<OpenAIChatClient>
{
}

[Category("OpenAIChatClient")]
public class OpenAIChatClientResponseTests : ChatClientResponseTestsBase<OpenAIChatClient>
{
}

[Category("OpenAIChatClient")]
public class OpenAIChatClientStreamingTests : ChatClientStreamingTestsBase<OpenAIChatClient>
{
}

[Category("OpenAIChatClient")]
public class OpenAIChatClientJsonSchemaTests : ChatClientJsonSchemaTestsBase<OpenAIChatClient>
{
}

#endif
