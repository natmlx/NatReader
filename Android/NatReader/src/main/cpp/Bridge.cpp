//
//  Bridge.cpp
//  NatReader
//
//  Created by Yusuf Olokoba on 1/12/2020.
//  Copyright © 2020 Yusuf Olokoba. All rights reserved.
//

#include "NatReader.h"
#include <jni.h>
#include <cstring>


static JNIEnv* GetEnv ();

#pragma region --NatReader--

void* NRCreateMP4FrameReader (const char* uri, float startTime, float duration) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env)
        return nullptr;
    // Create reader
    jstring path = env->NewStringUTF(uri);
    jclass clazz = env->FindClass("api/natsuite/natreader/MP4FrameReader");
    jmethodID constructor = env->GetMethodID(clazz, "<init>", "(Ljava/lang/String;FF)V");
    jobject object = env->NewObject(clazz, constructor, path, startTime, duration);
    jobject reader = env->NewGlobalRef(object);
    return static_cast<void*>(reader);
}

void NRMediaURI (void* readerPtr, char* dstString) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env)
        return;
    // Get URI
    jobject reader = static_cast<jobject>(readerPtr);
    jstring uri = (jstring)env->CallObjectMethod(reader, env->GetMethodID(env->GetObjectClass(reader), "uri", "()Ljava/lang/String;"));
    const char *uriStr = env->GetStringUTFChars(uri, 0);
    strcpy(dstString, uriStr);
    env->ReleaseStringUTFChars(uri, uriStr);
}

float NRMediaDuration (void* readerPtr) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env)
        return 0.f;
    // Get duration
    jobject reader = static_cast<jobject>(readerPtr);
    jmethodID method = env->GetMethodID(env->GetObjectClass(reader), "duration", "()F");
    return env->CallFloatMethod(reader, method);
}

void NRCopyNextFrame (void* readerPtr, void* buffer, int32_t* outBufferSize, int64_t* outTimestamp) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env) {
        *outBufferSize = 0;
        *outTimestamp = 0L;
        return;
    }
    // Copy next frame
    jobject byteBuffer = env->NewDirectByteBuffer(buffer, (jlong)*outBufferSize);
    jobject reader = static_cast<jobject>(readerPtr);
    jmethodID method = env->GetMethodID(env->GetObjectClass(reader), "copyNextFrame", "(Ljava/nio/ByteBuffer;)J");
    *outTimestamp = env->CallLongMethod(reader, method, byteBuffer);
    *outBufferSize = env->CallIntMethod(byteBuffer, env->GetMethodID(env->GetObjectClass(byteBuffer), "limit", "()I"));
}

void NRReset (void* readerPtr) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env)
        return;
    // Reset
    jobject reader = static_cast<jobject>(readerPtr);
    jmethodID method = env->GetMethodID(env->GetObjectClass(reader), "reset", "()V");
    env->CallVoidMethod(reader, method);
}

void NRDispose (void* readerPtr) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env)
        return;
    // Release
    jobject reader = static_cast<jobject>(readerPtr);
    jmethodID method = env->GetMethodID(env->GetObjectClass(reader), "release", "()V");
    env->CallVoidMethod(reader, method);
}

void NRFrameSize (void* frameReaderPtr, int32_t* outWidth, int32_t* outHeight) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env)
        return;
    // Get frame size
    jobject frameReader = static_cast<jobject>(frameReaderPtr);
    jclass clazz = env->GetObjectClass(frameReader);
    *outWidth = env->CallIntMethod(frameReader, env->GetMethodID(clazz, "frameWidth", "()I"));
    *outHeight = env->CallIntMethod(frameReader, env->GetMethodID(clazz, "frameHeight", "()I"));
}

float NRFrameRate (void* frameReaderPtr) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env)
        return 0.f;
    // Get frame rate
    jobject frameReader = static_cast<jobject>(frameReaderPtr);
    jmethodID method = env->GetMethodID(env->GetObjectClass(frameReader), "frameRate", "()F");
    return env->CallFloatMethod(frameReader, method);
}
#pragma endregion


#pragma region --JNI--

static JavaVM* jvm;

BRIDGE JNIEXPORT jint JNICALL JNI_OnLoad (JavaVM* vm, void* reserved) {
    jvm = vm;
    return JNI_VERSION_1_6;
}

JNIEnv* GetEnv () {
    JNIEnv* env = nullptr;
    int status = jvm->GetEnv(reinterpret_cast<void**>(&env), JNI_VERSION_1_6);
    if (status == JNI_EDETACHED)
        jvm->AttachCurrentThread(&env, nullptr);
    return env;
}
#pragma endregion