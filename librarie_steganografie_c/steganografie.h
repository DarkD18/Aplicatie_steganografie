#pragma once
#ifndef STEGANOGRAFIE_H
#define STEGANOGRAFIE_H

#include "bmp_handler.h"

// Exported functions for the DLL
__declspec(dllexport) const uint8_t* get_embedded_filename(const uint8_t* stego_path);
#pragma region standard
__declspec(dllexport) void hideMessage(const uint8_t* pixel_data, uint32_t pixel_data_size, const uint8_t* message, uint8_t* output_data);
__declspec(dllexport) void revealMessage(const uint8_t* pixel_data, uint32_t pixel_data_size, uint8_t* output, uint32_t max_len);
__declspec(dllexport) void hideFile(const uint8_t* pixel_data, uint32_t pixel_data_size, const uint8_t* file_name, const uint8_t* file_data, uint32_t file_size, uint8_t* output_data);
__declspec(dllexport) void extractFile(
    const uint8_t* pixel_data,
    uint32_t pixel_data_size,
    uint8_t* file_name,
    uint32_t file_name_bufsize,
    uint8_t* file_data,
    uint32_t file_data_bufsize,
    uint32_t* file_size);
#pragma endregion
#pragma region multichannel
__declspec(dllexport) void hide_message_multichannel(const uint8_t* pixel_data, uint32_t pixel_data_size, const uint8_t* message, uint8_t* output_data);
__declspec(dllexport) void reveal_message_multichannel(const uint8_t* pixel_data, uint32_t pixel_data_size, uint8_t* output, uint32_t max_len);
__declspec(dllexport) void hide_file_multichannel(const uint8_t* pixel_data, uint32_t pixel_data_size, const uint8_t* file_name, const uint8_t* file_data, uint32_t file_size, uint8_t* output_data);
__declspec(dllexport) void extract_file_multichannel(
    const uint8_t* pixel_data, uint32_t pixel_data_size,
    uint8_t* file_name, uint32_t file_name_bufsize,
    uint8_t* file_data, uint32_t file_data_bufsize,
    uint32_t* file_size);
#pragma endregion
#pragma region shuffle_standard
__declspec(dllexport) void hide_message_shuffle(
    const uint8_t* pixel_data, uint32_t pixel_data_size,
    const uint8_t* message, const uint8_t* password,
    uint8_t* output_data);
__declspec(dllexport) void reveal_message_shuffle(
    const uint8_t* pixel_data, uint32_t pixel_data_size,
    const uint8_t* password,
    uint8_t* output, uint32_t max_len);
__declspec(dllexport) void hide_file_shuffle(
    const uint8_t* pixel_data, uint32_t pixel_data_size,
    const uint8_t* file_name, const uint8_t* file_data, uint32_t file_size,
    const uint8_t* password,
    uint8_t* output_data);
__declspec(dllexport) void extract_file_shuffle(
    const uint8_t* pixel_data, uint32_t pixel_data_size,
    const uint8_t* password,
    uint8_t* file_name, uint32_t file_name_bufsize,
    uint8_t* file_data, uint32_t file_data_bufsize,
    uint32_t* file_size);
#pragma endregion
#pragma region shuffle_multichannel
__declspec(dllexport) void hide_message_multichannel_shuffle(
    const uint8_t* pixel_data,
    uint32_t       pixel_data_size,
    const uint8_t* message,
    const uint8_t* password,
    uint8_t* output_data);
__declspec(dllexport) void reveal_message_multichannel_shuffle(
    const uint8_t* pixel_data,
    uint32_t       pixel_data_size,
    const uint8_t* password,
    uint8_t* output,
    uint32_t       max_len);
__declspec(dllexport) void hide_file_multichannel_shuffle(
    const uint8_t* pixel_data,
    uint32_t       pixel_data_size,
    const uint8_t* file_name,
    const uint8_t* file_data,
    uint32_t       file_size,
    const uint8_t* password,
    uint8_t* output_data);
__declspec(dllexport) void extract_file_multichannel_shuffle(
    const uint8_t* pixel_data,
    uint32_t       pixel_data_size,
    const uint8_t* password,
    uint8_t* file_name,
    uint32_t       file_name_bufsize,
    uint8_t* file_data,
    uint32_t       file_data_bufsize,
    uint32_t* file_size);
#pragma endregion

#endif // STEGANOGRAFIE_H
