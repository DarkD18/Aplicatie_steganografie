#pragma once
#ifndef STEGANOGRAFIE_H
#define STEGANOGRAFIE_H

#include "bmp_handler.h"

// Exported functions for the DLL
__declspec(dllexport) void hideMessage(const uint8_t* pixel_data, uint32_t pixel_data_size, const uint8_t* message, uint8_t* output_data);
__declspec(dllexport) void revealMessage(const uint8_t* pixel_data, uint32_t pixel_data_size, uint8_t* output, uint32_t max_len);
__declspec(dllexport) void hideFile(const uint8_t* pixel_data, uint32_t pixel_data_size, const uint8_t* file_name, const uint8_t* file_data, uint32_t file_size, uint8_t* output_data);
__declspec(dllexport) void extractFile(const uint8_t* pixel_data, uint32_t pixel_data_size, uint8_t* file_name, uint8_t* file_data, uint32_t* file_size);
__declspec(dllexport) const uint8_t* get_embedded_filename(const uint8_t* stego_path);
__declspec(dllexport) void hide_message_multichannel(const uint8_t* pixel_data, uint32_t pixel_data_size, const uint8_t* message, uint8_t* output_data);
__declspec(dllexport) void reveal_message_multichannel(const uint8_t* pixel_data, uint32_t pixel_data_size, uint8_t* output, uint32_t max_len);
__declspec(dllexport) void hide_file_multichannel(const uint8_t* pixel_data, uint32_t pixel_data_size, const uint8_t* file_name, const uint8_t* file_data, uint32_t file_size, uint8_t* output_data);
__declspec(dllexport) void extract_file_multichannel(const uint8_t* pixel_data, uint32_t pixel_data_size,  uint8_t* file_name, uint8_t* file_data, uint32_t* file_size);

#endif // STEGANOGRAFIE_H
