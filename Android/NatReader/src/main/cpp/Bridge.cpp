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

#pragma region --Bridge--

void NRDisposeReader (void* readerPtr) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env)
        return;
    // Release
    jobject reader = static_cast<jobject>(readerPtr);
    jclass clazz = env->GetObjectClass(reader);
    jmethodID method = env->GetMethodID(clazz, "release", "()V");
    env->CallVoidMethod(reader, method);
    env->DeleteLocalRef(clazz);
    env->DeleteGlobalRef(reader);
}

void NRMediaURI (void* readerPtr, char* dstString) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env)
        return;
    // Get URI
    jobject reader = static_cast<jobject>(readerPtr);
    jclass clazz = env->GetObjectClass(reader);
    jmethodID method = env->GetMethodID(clazz, "uri", "()Ljava/lang/String;");
    jstring uri = (jstring)env->CallObjectMethod(reader, method);
    const char *uriStr = env->GetStringUTFChars(uri, 0);
    strcpy(dstString, uriStr);
    // Release
    env->ReleaseStringUTFChars(uri, uriStr);
    env->DeleteLocalRef(clazz);
    env->DeleteLocalRef(uri);
}

float NRMediaDuration (void* readerPtr) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env)
        return 0.f;
    // Get duration
    jobject reader = static_cast<jobject>(readerPtr);
    jclass clazz = env->GetObjectClass(reader);
    jmethodID method = env->GetMethodID(clazz, "duration", "()F");
    float value =  env->CallFloatMethod(reader, method);
    env->DeleteLocalRef(clazz);
    return value;
}

void* NRCreateEnumerator (void* readerPtr, float startTime, float duration, int frameSkip) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env)
        return nullptr;
    // Create
    jobject reader = static_cast<jobject>(readerPtr);
    jclass clazz = env->GetObjectClass(reader);
    jmethodID method = env->GetMethodID(clazz, "createEnumerator", "(FFI)Lapi/natsuite/natreader/MediaEnumerator;");
    jobject object = env->CallObjectMethod(reader, method, startTime, duration, frameSkip);
    jobject enumerator = env->NewGlobalRef(object);
    // Release locals
    env->DeleteLocalRef(clazz);
    env->DeleteLocalRef(object);
    return enumerator;
}

void NRDisposeEnumerator (void* enumeratorPtr) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env || !enumeratorPtr)
        return;
    // Release
    jobject enumerator = static_cast<jobject>(enumeratorPtr);
    jclass clazz = env->GetObjectClass(enumerator);
    jmethodID method = env->GetMethodID(clazz, "release", "()V");
    env->CallVoidMethod(enumerator, method);
    env->DeleteLocalRef(clazz);
    env->DeleteGlobalRef(enumerator);
}

void NRCopyNextFrame (void* enumeratorPtr, void* buffer, int32_t* outBufferSize, int64_t* outTimestamp) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env) {
        *outBufferSize = 0;
        *outTimestamp = 0L;
        return;
    }
    // Copy next frame
    jobject enumerator = static_cast<jobject>(enumeratorPtr);
    jobject byteBuffer = env->NewDirectByteBuffer(buffer, 1 << 30); // Should be big enough for most things
    jclass clazz = env->GetObjectClass(enumerator);
    jclass bbClazz = env->GetObjectClass(byteBuffer);
    jmethodID method = env->GetMethodID(clazz, "copyNextFrame", "(Ljava/nio/ByteBuffer;)J");
    jmethodID limitMethod = env->GetMethodID(bbClazz, "limit", "()I");
    *outTimestamp = env->CallLongMethod(enumerator, method, byteBuffer);
    *outBufferSize = env->CallIntMethod(byteBuffer, limitMethod);
    // Release
    env->DeleteLocalRef(byteBuffer);
    env->DeleteLocalRef(clazz);
    env->DeleteLocalRef(bbClazz);
}

void* NRCreateMP4Reader (const char* uri) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env)
        return nullptr;
    // Create reader
    jstring path = env->NewStringUTF(uri);
    jclass clazz = env->FindClass("api/natsuite/natreader/MP4Reader");
    jmethodID constructor = env->GetMethodID(clazz, "<init>", "(Ljava/lang/String;)V");
    jobject object = env->NewObject(clazz, constructor, path);
    jobject reader = env->NewGlobalRef(object);
    // Release locals
    env->DeleteLocalRef(path);
    env->DeleteLocalRef(clazz);
    env->DeleteLocalRef(object);
    return static_cast<void*>(reader);
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
    // Release
    env->DeleteLocalRef(clazz);
}

float NRFrameRate (void* frameReaderPtr) {
    // Get Java environment
    JNIEnv* env = GetEnv();
    if (!env)
        return 0.f;
    // Get frame rate
    jobject reader = static_cast<jobject>(frameReaderPtr);
    jclass clazz = env->GetObjectClass(reader);
    jmethodID method = env->GetMethodID(clazz, "frameRate", "()F");
    float value = env->CallFloatMethod(reader, method);
    env->DeleteLocalRef(clazz);
    return value;
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