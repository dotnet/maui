package com.microsoft.maui.essentials.ai

interface ModelWarmupListener {
    fun onSuccess()
    fun onFailure(error: Throwable)
}
