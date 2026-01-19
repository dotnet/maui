package com.microsoft.maui.essentials.ai

interface ModelStatusListener {
    fun onSuccess(status: Int)
    fun onFailure(error: Throwable)
}
