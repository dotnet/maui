using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Maui.Controls.Sample.Services;

public class TaggingService(IChatClient chatClient)
{
    public async Task<List<string>> GenerateTagsAsync(string text, CancellationToken cancellationToken = default)
    {
        var systemPrompt = 
            """
            Your job is to extract the most relevant tags from the input text.
            """;

        var userPrompt = 
            $"""
            Extract relevant tags from this text:
            
            {text}
            """;

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userPrompt)
        };

        var options = new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                TaggingResponse.ToJsonSchema(),
				schemaName: "TaggingResponse",
				schemaDescription: "A response containing relevant tags extracted from input text")
        };

        var response = await chatClient.GetResponseAsync(messages, options, cancellationToken);
        var jsonText = response.ToString();

		var jsonOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true
		};

        return JsonSerializer.Deserialize<TaggingResponse>(jsonText, jsonOptions)?.Tags ?? [];
    }

    public class TaggingResponse
    {
        [Description("Most important topics in the input text.")]
        [Length(5, 5)]
        public List<string> Tags { get; set; } = [];

        public static JsonElement ToJsonSchema()
        {
            var schema = AIJsonUtilities.CreateJsonSchema(
                typeof(TaggingResponse),
                inferenceOptions: new AIJsonSchemaCreateOptions
                {
                    TransformSchemaNode = TransformSchemaNode,
                    TransformOptions = new AIJsonSchemaTransformOptions
                    {
                        DisallowAdditionalProperties = true
                    }
                });

            return schema;

            JsonNode TransformSchemaNode(AIJsonSchemaCreateContext context, JsonNode schema)
            {
                if (schema is not JsonObject obj)
                {
                    return schema;
                }

                if (obj.TryGetPropertyValue("type", out var typeNode) && typeNode?.GetValue<string>() == "object")
                {
                    // Add title for object type definitions only
                    if (context.TypeInfo is not null && !obj.ContainsKey("title"))
                    {
                        obj["title"] = context.TypeInfo.Type.Name;
                    }
                }

                // Add x-order for property ordering
                if (obj.TryGetPropertyValue("properties", out var props) && props is JsonObject propsObj)
                {
                    var order = new JsonArray();
                    foreach (var prop in propsObj)
                    {
                        order.Add(JsonValue.Create(prop.Key));
                    }
                    obj["x-order"] = order;
                }

                return schema;
            }
        }
    }
}
