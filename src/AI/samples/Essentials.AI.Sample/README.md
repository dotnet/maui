# Essentials.AI.Sample - Trip Planner

AI-powered travel itinerary generator using Microsoft.Extensions.AI in .NET MAUI.

## Overview

This sample demonstrates how to integrate Large Language Models (LLMs) into a .NET MAUI application using Microsoft.Extensions.AI. The app generates personalized multi-day travel itineraries for famous landmarks worldwide, featuring:

- **Streaming AI Responses**: Real-time itinerary generation with incremental updates
- **Structured JSON Output**: Uses JSON schema to ensure consistent, typed responses
- **AI-Powered Tagging**: Automatic keyword extraction from landmark descriptions
- **Cross-Platform**: Runs on iOS, Android, Windows, and macOS

## How It Works

1. **Chat Client Setup**: An AI chat client is registered as an `IChatClient` singleton in dependency injection
2. **Landmark Selection**: Users browse landmarks organized by continent and select one for trip planning
3. **Tag Generation**: The `TaggingService` extracts relevant tags from landmark descriptions using AI with JSON response format
4. **Itinerary Generation**: The `ItineraryService` streams a structured 3-day itinerary using:
   - System instructions that define the AI's role and constraints
   - JSON schema validation to ensure properly formatted responses with required fields (title, days, activities, etc.)
   - Real-time streaming updates displayed progressively in the UI
5. **Display**: Generated itineraries show daily activities including sightseeing, dining, and lodging recommendations

### Key AI Patterns

- **Structured Output**: Uses `ChatResponseFormat.ForJsonSchema()` to enforce response structure matching C# record types
- **Streaming**: Implements `IAsyncEnumerable` to process AI responses incrementally as they arrive
- **Schema Generation**: Uses `AIJsonUtilities.CreateJsonSchema()` with custom transforms to add enum constraints and property ordering
- **Partial Deserialization**: Deserializes incomplete JSON during streaming to show progressive updates

## Project Structure

### Core Files

- **`MauiProgram`**: Configures DI container and registers `IChatClient` and services
- **`App`**: Application entry point, creates main window with `AppShell`
- **`AppShell`**: Shell navigation structure, defines `LandmarksPage` as the initial route

### Pages

- **`Pages/LandmarksPage`**: Displays landmarks organized by continent with featured item
- **`Pages/TripPlanningPage`**: Shows landmark details, AI-generated tags, and itinerary generation UI

### ViewModels

- **`ViewModels/LandmarksViewModel`**: Manages landmark data loading and grouping by continent
- **`ViewModels/TripPlanningViewModel`**: Orchestrates itinerary generation, tag display, and loading states using `[QueryProperty]` for navigation parameters

### Models

- **`Models/Landmark`**: Represents a travel destination with coordinates, description, and metadata
- **`Models/Itinerary`**: Structured itinerary with title, days, and activities; includes `ToJsonSchema()` method for AI response validation

### Services

- **`Services/ItineraryService`**: Streams AI-generated itineraries using `IChatClient.GetStreamingResponseAsync()` with JSON schema constraints
- **`Services/TaggingService`**: Generates descriptive tags from text using AI with JSON response format
- **`Services/LandmarkDataService`**: Singleton service that loads and provides access to landmark data from `landmarkData.json`
- **`Services/FindPointsOfInterestTool`**: Mock tool for AI function calling (demonstrates tool integration pattern, currently returns placeholder data)
- **`Services/StreamingJsonDeserializer`**: Utility for deserializing incomplete JSON during streaming

### Views

**Landmarks:**
- **`Views/Landmarks/LandmarkFeaturedItemView`**: Large featured landmark card display
- **`Views/Landmarks/LandmarkHorizontalListView`**: Horizontal scrolling list of landmarks
- **`Views/Landmarks/LandmarkListItemView`**: Individual landmark item template

**Itinerary:**
- **`Views/Itinerary/LandmarkTripView`**: Container view for trip planning interface
- **`Views/Itinerary/LandmarkDescriptionView`**: Displays landmark details and AI-generated tags
- **`Views/Itinerary/ItineraryPlanningView`**: Itinerary generation trigger and loading state
- **`Views/Itinerary/ItineraryView`**: Complete itinerary display with title and days
- **`Views/Itinerary/DayView`**: Single day plan view with activities
- **`Views/Itinerary/ActivityListView`**: List of activities for a day
- **`Views/Itinerary/MessageView`**: Error message display

### Converters

- **`Converters/InvertedBoolConverter`**: XAML converter to invert boolean values
- **`Converters/IsNotNullConverter`**: XAML converter to check if value is not null
- **`Converters/IsNotNullOrEmptyConverter`**: XAML converter to check if collection is not empty

### Configuration

- **`Essentials.AI.Sampleproj`**: Project file with Microsoft.Extensions.AI packages
- **`Directory.Build.targets`**: Build configuration

## Dependencies

- **Microsoft.Extensions.AI**: Core AI abstractions and chat client interfaces
- **CommunityToolkit.Mvvm**: MVVM helpers for data binding and commands
