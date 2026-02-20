# Essentials.AI.Sample - Trip Planner

AI-powered travel itinerary generator using Microsoft.Extensions.AI and Microsoft.Agents.AI in .NET MAUI.

## Overview

This sample demonstrates how to integrate Large Language Models (LLMs) into a .NET MAUI application using a multi-agent workflow architecture. The app generates personalized multi-day travel itineraries for famous landmarks worldwide, featuring:

- **Multi-Agent Workflow**: 4 specialized AI agents that collaborate to generate itineraries
- **Streaming AI Responses**: Real-time itinerary generation with incremental updates
- **Structured JSON Output**: Uses JSON schema to ensure consistent, typed responses
- **AI-Powered Tools**: Agents use function calling to discover points of interest
- **Conditional Translation**: Automatic translation when non-English output is requested
- **Cross-Platform**: Runs on iOS, Android, Windows, and macOS

## Setup

To run this sample, you need to configure Azure OpenAI credentials using user secrets.

### Configure User Secrets

1. Navigate to the sample directory:
   ```bash
   cd src/AI/samples/Essentials.AI.Sample
   ```

2. Initialize user secrets (if not already done):
   ```bash
   dotnet user-secrets init
   ```

3. Create a `secrets.json` file with your Azure OpenAI configuration:
   ```json
   {
       "AI": {
           "DeploymentName": "<your-chat-deployment-name>",
           "EmbeddingDeploymentName": "<your-embedding-deployment-name>",
           "Endpoint": "<your-azure-openai-endpoint>",
           "ApiKey": "<your-azure-openai-api-key>"
       }
   }
   ```

   Replace the placeholders:
   - `<your-chat-deployment-name>` - Your Azure OpenAI chat model deployment (e.g., `gpt-4o`)
   - `<your-embedding-deployment-name>` - Your embedding model deployment (e.g., `text-embedding-3-small`)
   - `<your-azure-openai-endpoint>` - Your Azure OpenAI endpoint URL
   - `<your-azure-openai-api-key>` - Your Azure OpenAI API key

4. The secrets file should be placed at the user secrets location or embedded as `secrets.json` in the project.

## How It Works

### Agent Workflow Architecture

The app uses a 4-agent workflow registered via `AddItineraryWorkflow()`:

1. **Travel Planner Agent** - Parses natural language input to extract destination, day count, and target language (no tools - just structured output)
2. **Researcher Agent** - Uses semantic search (RAG with embeddings) to find candidate destinations, then AI selects the best match
3. **Itinerary Planner Agent** - Builds a detailed itinerary using the `findPointsOfInterest` tool with streaming JSON output
4. **Translator Agent** - Translates the itinerary if a non-English language was requested (conditional, streaming)

The workflow uses a conditional branching pattern:
- **English requests**: Travel Planner → Researcher → Itinerary Planner → Output
- **Non-English requests**: Travel Planner → Researcher → Itinerary Planner → Translator → Output

### Service Registration

Multiple AI clients are registered in dependency injection:
- `IChatClient` - Generic chat client for simple inference (e.g., `TaggingService`)
- `IChatClient` keyed `"local-model"` - Chat client with function calling support
- `IChatClient` keyed `"cloud-model"` - Chat client for high-quality output (translation)
- `AIAgent` keyed `"itinerary-workflow-agent"` - The complete workflow as an invocable agent

### User Flow

1. **Landmark Selection**: Users browse landmarks organized by continent and select one for trip planning
2. **Tag Generation**: The `TaggingService` extracts relevant tags from landmark descriptions using AI
3. **Itinerary Generation**: The `ItineraryService` invokes the workflow agent which:
   - Parses the user's request to extract intent
   - Researches available destinations to find the best match
   - Generates a structured itinerary with real places using tool calls
   - Optionally translates the result if a non-English language was requested
4. **Display**: Generated itineraries show daily activities including sightseeing, dining, and lodging recommendations with real-time streaming updates

### Key AI Patterns

- **Workflow-as-Agent**: The multi-agent workflow is registered as a single `AIAgent` for clean invocation via `workflowAgent.RunStreamingAsync()`
- **RAG (Retrieval-Augmented Generation)**: Researcher agent uses embedding-based semantic search to find candidate destinations before AI selection
- **Structured Output**: Uses `ChatResponseFormat.ForJsonSchema<T>()` to enforce response structure matching C# record types
- **Function Calling**: Itinerary Planner uses `AIFunctionFactory.Create()` to define the `findPointsOfInterest` tool
- **Streaming**: Agents use `RunStreamingAsync()` to emit partial JSON as it's generated for progressive UI updates
- **Partial Deserialization**: `StreamingJsonDeserializer` deserializes incomplete JSON during streaming to show progressive updates
- **Conditional Edges**: Workflow uses typed edge conditions to branch based on output (e.g., skip translation for English)
