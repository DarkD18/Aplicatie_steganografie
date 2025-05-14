#pragma once
#ifndef STEGANOGRAFIE_H
#define STEGANOGRAFIE_H

#include "bmp_handler.h"

// Exported functions for the DLL
__declspec(dllexport) void hideMessage(const char* image_path, const char* message, const char* output_path);
__declspec(dllexport) void revealMessage(const char* image_path, char* output, size_t max_len);
__declspec(dllexport) void hide_file(const char* cover_path, const char* file_path, const char* output_path);
__declspec(dllexport) void extract_file(const char* stego_path, const char* output_file_path);
__declspec(dllexport) const char* get_embedded_filename(const char* stego_path);
__declspec(dllexport) void hide_message_multichannel(const char* image_path, const char* message, const char* output_path);
__declspec(dllexport) void reveal_message_multichannel(const char* image_path, char* output, size_t max_len);
#endif // STEGANOGRAFIE_H
