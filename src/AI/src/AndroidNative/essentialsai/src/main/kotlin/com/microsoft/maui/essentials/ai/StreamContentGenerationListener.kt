package com.microsoft.maui.essentials.ai

import com.google.mlkit.genai.prompt.GenerateContentResponse

interface StreamContentGenerationListener {
    fun onComplete(error: Throwable?)
    fun onResponse(response: GenerateContentResponse)
}
