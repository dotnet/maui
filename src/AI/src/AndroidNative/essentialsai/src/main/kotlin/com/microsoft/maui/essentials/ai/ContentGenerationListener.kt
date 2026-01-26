package com.microsoft.maui.essentials.ai

import com.google.mlkit.genai.prompt.GenerateContentResponse

interface ContentGenerationListener {
    fun onSuccess(response: GenerateContentResponse)
    fun onFailure(error: Throwable)
}
