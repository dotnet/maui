#if WINDOWS
using Maui.Controls.Sample.Services;
using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Wraps PhiSilicaChatClient with PromptBasedSchemaClient so that JSON schema
/// requests are converted to prompt instructions (Phi Silica has no native
/// structured output support).
/// </summary>
public class PhiSilicaSchemaClient : DelegatingChatClient
{
	public PhiSilicaSchemaClient() : base(new PromptBasedSchemaClient(new PhiSilicaChatClient())) { }
}

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

[Category("PhiSilicaChatClient")]
public class PhiSilicaChatClientJsonSchemaTests : ChatClientJsonSchemaTestsBase<PhiSilicaSchemaClient>
{
	[Fact(Skip = "Phi Silica does not support JSON format without a schema — PromptBasedSchemaClient requires a schema to rewrite.")]
	public override Task GetResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow()
		=> base.GetResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow();

	[Fact(Skip = "Phi Silica does not support JSON format without a schema — PromptBasedSchemaClient requires a schema to rewrite.")]
	public override Task GetStreamingResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow()
		=> base.GetStreamingResponseAsync_WithJsonFormatWithoutSchema_DoesNotThrow();
}

#endif
