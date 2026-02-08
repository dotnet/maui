# Keep Kotlin coroutines classes
-keepclassmembers class kotlinx.coroutines.** { *; }
-keep class kotlinx.coroutines.** { *; }

# Keep our extension functions and listeners
-keep class com.microsoft.maui.essentials.ai.** { *; }
-keepclassmembers class com.microsoft.maui.essentials.ai.** { *; }

# Keep listener interfaces
-keep interface com.microsoft.maui.essentials.ai.ContentGenerationListener { *; }
-keep interface com.microsoft.maui.essentials.ai.StreamContentGenerationListener { *; }
-keep interface com.microsoft.maui.essentials.ai.ModelStatusListener { *; }
-keep interface com.microsoft.maui.essentials.ai.ModelWarmupListener { *; }
