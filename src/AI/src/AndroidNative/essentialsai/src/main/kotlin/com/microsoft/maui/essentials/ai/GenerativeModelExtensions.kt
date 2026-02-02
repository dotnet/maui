package com.microsoft.maui.essentials.ai

import android.os.CancellationSignal
import com.google.mlkit.genai.prompt.GenerateContentRequest
import com.google.mlkit.genai.prompt.GenerativeModel
import kotlinx.coroutines.DelicateCoroutinesApi
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.flow.onCompletion
import kotlinx.coroutines.future.future

/**
 * Extension functions for GenerativeModel
 * Top-level functions that bind to C# as GenerativeModelExtensionsKt static class
 */

/**
 * Checks the model status asynchronously
 */
@OptIn(DelicateCoroutinesApi::class)
fun GenerativeModel.checkStatus(
    listener: ModelStatusListener
): CancellationSignal {
    val model = this
    val job = GlobalScope.future {
        try {
            val status = model.checkStatus()
            listener.onSuccess(status)
        } catch (ex: Exception) {
            listener.onFailure(ex)
        }
    }
    val cancellationSignal = CancellationSignal()
    cancellationSignal.setOnCancelListener {
        job.cancel(true)
    }
    return cancellationSignal
}

/**
 * Warms up the model for better first-response latency
 */
@OptIn(DelicateCoroutinesApi::class)
fun GenerativeModel.warmup(
    listener: ModelWarmupListener
): CancellationSignal {
    val model = this
    val job = GlobalScope.future {
        try {
            model.warmup()
            listener.onSuccess()
        } catch (ex: Exception) {
            listener.onFailure(ex)
        }
    }
    val cancellationSignal = CancellationSignal()
    cancellationSignal.setOnCancelListener {
        job.cancel(true)
    }
    return cancellationSignal
}

/**
 * Generates content (non-streaming)
 */
@OptIn(DelicateCoroutinesApi::class)
fun GenerativeModel.generateContent(
    request: GenerateContentRequest,
    listener: ContentGenerationListener
): CancellationSignal {
    val model = this
    val job = GlobalScope.future {
        try {
            val response = model.generateContent(request)
            listener.onSuccess(response)
        } catch (ex: Exception) {
            listener.onFailure(ex)
        }
    }
    val cancellationSignal = CancellationSignal()
    cancellationSignal.setOnCancelListener {
        job.cancel(true)
    }
    return cancellationSignal
}

/**
 * Generates content with streaming
 */
@OptIn(DelicateCoroutinesApi::class)
fun GenerativeModel.generateContentStream(
    request: GenerateContentRequest,
    listener: StreamContentGenerationListener
): CancellationSignal {
    val model = this
    val job = GlobalScope.future {
        model
            .generateContentStream(request)
            .onCompletion { maybeError ->
                listener.onComplete(maybeError)
            }
            .collect { response ->
                listener.onResponse(response)
            }
    }
    val cancellationSignal = CancellationSignal()
    cancellationSignal.setOnCancelListener {
        job.cancel(true)
    }
    return cancellationSignal
}