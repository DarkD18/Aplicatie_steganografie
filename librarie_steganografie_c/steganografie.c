#include "bmp_handler.h"
#include "steganografie.h"
#include <stdio.h>
#include <string.h>
#include <windows.h>

static uint8_t extracted_filename[256];

// Helper to advance to the next pixel/channel, handling row/col/channel
static int32_t next_pixel_channel(uint32_t width, uint32_t height, uint32_t* x, uint32_t* y, uint32_t* channel) {
    (*channel)++;
    if (*channel == 3) {
        *channel = 0;
        (*x)++;
        if (*x == width) {
            *x = 0;
            (*y)++;
            if (*y == height) {
                return 0; // End of image
            }
        }
    }
    return 1;
}
__declspec(dllexport) const uint8_t* get_embedded_filename(const uint8_t* stego_path) {
    BMPImage* bmp = load_bmp(stego_path);
    if (!bmp) return NULL;

    uint32_t bit_index = 0;
    uint32_t name_len = 0;
    for (int b = 0; b < 32; ++b)
        name_len |= (bmp->pixel_data[bit_index++] & 1) << b;

    if (name_len == 0 || name_len > 255) {
        free_bmp(bmp);
        return NULL;
    }

    for (uint32_t i = 0; i < name_len; ++i) {
        uint8_t byte = 0;
        for (int b = 0; b < 8; ++b)
            byte |= (bmp->pixel_data[bit_index++] & 1) << b;
        extracted_filename[i] = byte;
    }
    extracted_filename[name_len] = '\0';

    free_bmp(bmp);
    return extracted_filename;
}

#pragma region standard
__declspec(dllexport) void hideMessage(
    const uint8_t* pixel_data, uint32_t pixel_data_size,
    const uint8_t* message, uint8_t* output_data)
{
    uint32_t msg_len = strlen((const char*)message) + 1; // Include null terminator
    uint32_t pixel_count = pixel_data_size / 3; // 3 bytes per pixel

    if (msg_len * 8 > pixel_count) {
        MessageBoxA(NULL, "Error: Message is too long to fit in the image.", "ERROR", MB_OK);
        return;
    }

    memcpy(output_data, pixel_data, pixel_data_size);

    uint32_t bit_idx = 0;
    for (uint32_t i = 0; i < msg_len; ++i) {
        for (int bit = 0; bit < 8; ++bit, ++bit_idx) {
            uint32_t pixel = bit_idx;
            if (pixel * 3 + 2 >= pixel_data_size) break;
            // Hide in blue channel (offset +2)
            output_data[pixel * 3 + 2] = (output_data[pixel * 3 + 2] & ~1) | ((message[i] >> bit) & 1);
        }
    }

    MessageBoxA(NULL, "Message hidden successfully in the image.", "SUCCESS", MB_OK);
}

__declspec(dllexport) void revealMessage(
    const uint8_t* pixel_data, uint32_t pixel_data_size,
    uint8_t* output, uint32_t max_len)
{
    uint32_t pixel_count = pixel_data_size / 3;
    uint32_t bit_idx = 0;
    for (uint32_t i = 0; i < max_len; ++i) {
        output[i] = 0;
        for (int bit = 0; bit < 8; ++bit, ++bit_idx) {
            uint32_t pixel = bit_idx;
            if (pixel * 3 + 2 >= pixel_data_size || pixel >= pixel_count) {
                output[i] = '\0';
                return;
            }
            output[i] |= (pixel_data[pixel * 3 + 2] & 1) << bit;
        }
        if (output[i] == '\0') break;
    }
    output[max_len - 1] = '\0';
}


__declspec(dllexport) void hideFile(
    const uint8_t* pixel_data, uint32_t pixel_data_size,
    const uint8_t* file_name, const uint8_t* file_data, uint32_t file_size,
    uint8_t* output_data)
{
    const char magic[4] = { 'S','T','G','F' };
    uint8_t method = 0x01; // 0x01 = Standard LSB

    uint32_t name_len = strlen((const char*)file_name);
    uint32_t total_bits = 32 + name_len * 8 + 32 + file_size * 8;
    uint32_t pixel_count = pixel_data_size / 3;

    if (total_bits > pixel_count) {
        MessageBoxA(NULL, "Error: File is too large to fit in the image.", "ERROR", MB_OK);
        return;
    }

    memcpy(output_data, pixel_data, pixel_data_size);

    uint32_t bit_idx = 0;
    // Encode magic header
    for (int i = 0; i < 4; ++i)
        for (int b = 0; b < 8; ++b, ++bit_idx)
            output_data[bit_idx * 3 + 2] = (output_data[bit_idx * 3 + 2] & ~1) | ((magic[i] >> b) & 1);
    // Encode method byte
    for (int b = 0; b < 8; ++b, ++bit_idx)
        output_data[bit_idx * 3 + 2] = (output_data[bit_idx * 3 + 2] & ~1) | ((method >> b) & 1);

    // Encode name length (32 bits)
    for (int b = 0; b < 32; ++b, ++bit_idx)
        output_data[bit_idx * 3 + 2] = (output_data[bit_idx * 3 + 2] & ~1) | ((name_len >> b) & 1);

    // Encode file name
    for (uint32_t i = 0; i < name_len; ++i)
        for (int b = 0; b < 8; ++b, ++bit_idx)
            output_data[bit_idx * 3 + 2] = (output_data[bit_idx * 3 + 2] & ~1) | ((file_name[i] >> b) & 1);

    // Encode file size (32 bits)
    for (int b = 0; b < 32; ++b, ++bit_idx)
        output_data[bit_idx * 3 + 2] = (output_data[bit_idx * 3 + 2] & ~1) | ((file_size >> b) & 1);

    // Encode file content
    for (uint32_t i = 0; i < file_size; ++i)
        for (int b = 0; b < 8; ++b, ++bit_idx)
            output_data[bit_idx * 3 + 2] = (output_data[bit_idx * 3 + 2] & ~1) | ((file_data[i] >> b) & 1);

    MessageBoxA(NULL, "File hidden successfully in the image.", "SUCCESS", MB_OK);
}

__declspec(dllexport) void extractFile(
    const uint8_t* pixel_data,
    uint32_t pixel_data_size,
    uint8_t* file_name,
    uint32_t file_name_bufsize,
    uint8_t* file_data,
    uint32_t file_data_bufsize,
    uint32_t* file_size)
{
    uint32_t bit_idx = 0;
    // Decode magic header
    char magic[4];
    for (int i = 0; i < 4; ++i) {
        magic[i] = 0;
        for (int b = 0; b < 8; ++b, ++bit_idx)
            magic[i] |= (pixel_data[bit_idx * 3 + 2] & 1) << b;
    }
    // Decode method byte
    uint8_t method = 0;
    for (int b = 0; b < 8; ++b, ++bit_idx)
        method |= (pixel_data[bit_idx * 3 + 2] & 1) << b;

    if (memcmp(magic, "STGF", 4) != 0 || method != 0x01) {
        MessageBoxA(NULL, "No file hidden with Standard LSB in this image.", "ERROR", MB_OK);
        return;
    }

    // Decode name length
    uint32_t name_len = 0;
    for (int b = 0; b < 32; ++b, ++bit_idx)
        name_len |= (pixel_data[bit_idx * 3 + 2] & 1) << b;
    if (name_len >= file_name_bufsize) name_len = file_name_bufsize - 1;

    // Decode file name
    for (uint32_t i = 0; i < name_len; ++i) {
        file_name[i] = 0;
        for (int b = 0; b < 8; ++b, ++bit_idx)
            file_name[i] |= (pixel_data[bit_idx * 3 + 2] & 1) << b;
    }
    file_name[name_len] = '\0';

    // Decode file size
    *file_size = 0;
    for (int b = 0; b < 32; ++b, ++bit_idx)
        *file_size |= (pixel_data[bit_idx * 3 + 2] & 1) << b;
    if (*file_size > file_data_bufsize) {
        MessageBoxA(NULL, "Error: Extracted file size exceeds buffer capacity.", "ERROR", MB_OK);
        return;
    }
    *file_size = file_data_bufsize;
    // Decode file content
    for (uint32_t i = 0; i < *file_size; ++i) {
        file_data[i] = 0;
        for (int b = 0; b < 8; ++b, ++bit_idx)
            file_data[i] |= (pixel_data[bit_idx * 3 + 2] & 1) << b;
    }

    MessageBoxA(NULL, "File extracted successfully from the image.", "SUCCESS", MB_OK);
}


#pragma endregion 


#pragma region multichannel
// Multi-Channel LSB: Spreads bits across R+G+B channels
__declspec(dllexport) void hide_message_multichannel(
    const uint8_t* pixel_data, uint32_t pixel_data_size,
    const uint8_t* message, uint8_t* output_data)
{
    uint32_t msg_len = strlen((const char*)message) + 1; // Include null terminator
    uint32_t total_bits = msg_len * 8;

    if (total_bits > pixel_data_size) {
        MessageBoxA(NULL, "Error: Message too long for multi-channel LSB.", "ERROR", MB_OK);
        return;
    }

    memcpy(output_data, pixel_data, pixel_data_size);

    for (uint32_t bit_idx = 0; bit_idx < total_bits; ++bit_idx) {
        uint32_t byte_idx = bit_idx / 8;
        uint8_t bit = (message[byte_idx] >> (bit_idx % 8)) & 1;
        output_data[bit_idx] = (output_data[bit_idx] & ~1) | bit;
    }

    MessageBoxA(NULL, "Message hidden successfully in the image.", "SUCCESS", MB_OK);
}

__declspec(dllexport) void reveal_message_multichannel(
    const uint8_t* pixel_data, uint32_t pixel_data_size,
    uint8_t* output, uint32_t max_len)
{
    uint32_t total_bits = pixel_data_size;
    uint32_t out_idx = 0;
    uint32_t bit_in_char = 0;
    output[0] = 0;

    for (uint32_t bit_idx = 0; bit_idx < total_bits && out_idx < max_len - 1; ++bit_idx) {
        output[out_idx] |= (pixel_data[bit_idx] & 1) << bit_in_char;
        bit_in_char++;
        if (bit_in_char == 8) {
            if (output[out_idx] == '\0') break;
            out_idx++;
            output[out_idx] = 0;
            bit_in_char = 0;
        }
    }
    output[max_len - 1] = '\0';
}

__declspec(dllexport) void hide_file_multichannel(
    const uint8_t* pixel_data, uint32_t pixel_data_size,
    const uint8_t* file_name, const uint8_t* file_data, uint32_t file_size,
    uint8_t* output_data)
{
    const char magic[4] = { 'S','T','G','F' };
    uint8_t method = 0x02; // 0x01 = Multichannell LSB
    uint32_t name_len = strlen((const char*)file_name);
    uint32_t total_bits = 40 + 32 + name_len * 8 + 32 + file_size * 8;

    if (total_bits > pixel_data_size) {
        MessageBoxA(NULL, "Error: File is too large to fit in the image.", "ERROR", MB_OK);
        return;
    }

    memcpy(output_data, pixel_data, pixel_data_size);

    uint32_t bit_idx = 0;
    // Encode magic header
    for (int i = 0; i < 4; ++i)
        for (int b = 0; b < 8; ++b, ++bit_idx)
            output_data[bit_idx] = (output_data[bit_idx] & ~1) | ((magic[i] >> b) & 1);
    // Encode method byte
    for (int b = 0; b < 8; ++b, ++bit_idx)
        output_data[bit_idx] = (output_data[bit_idx] & ~1) | ((method >> b) & 1);

    // Encode name length (32 bits)
    for (int b = 0; b < 32; ++b, ++bit_idx)
        output_data[bit_idx] = (output_data[bit_idx] & ~1) | ((name_len >> b) & 1);

    // Encode file name
    for (uint32_t i = 0; i < name_len; ++i)
        for (int b = 0; b < 8; ++b, ++bit_idx)
            output_data[bit_idx] = (output_data[bit_idx] & ~1) | ((file_name[i] >> b) & 1);

    // Encode file size (32 bits)
    for (int b = 0; b < 32; ++b, ++bit_idx)
        output_data[bit_idx] = (output_data[bit_idx] & ~1) | ((file_size >> b) & 1);

    // Encode file content
    for (uint32_t i = 0; i < file_size; ++i)
        for (int b = 0; b < 8; ++b, ++bit_idx)
            output_data[bit_idx] = (output_data[bit_idx] & ~1) | ((file_data[i] >> b) & 1);

    MessageBoxA(NULL, "File hidden successfully in the image.", "SUCCESS", MB_OK);
}

__declspec(dllexport) void extract_file_multichannel(
    const uint8_t* pixel_data, uint32_t pixel_data_size,
    uint8_t* file_name, uint32_t file_name_bufsize,
    uint8_t* file_data, uint32_t file_data_bufsize,
    uint32_t* file_size)
{
    uint32_t bit_idx = 0;
    // Decode magic header
    char magic[4];
    for (int i = 0; i < 4; ++i) {
        magic[i] = 0;
        for (int b = 0; b < 8; ++b, ++bit_idx)
            magic[i] |= (pixel_data[bit_idx] & 1) << b;
    }
    // Decode method byte
    uint8_t method = 0;
    for (int b = 0; b < 8; ++b, ++bit_idx)
        method |= (pixel_data[bit_idx] & 1) << b;

    if (memcmp(magic, "STGF", 4) != 0 || method != 0x02) {
        MessageBoxA(NULL, "No file hidden with Multichannel LSB in this image.", "ERROR", MB_OK);
        return;
    }
    // Decode name length
    uint32_t name_len = 0;
    for (int b = 0; b < 32; ++b, ++bit_idx)
        name_len |= (pixel_data[bit_idx] & 1) << b;
    if (name_len >= file_name_bufsize) name_len = file_name_bufsize - 1;

    // Decode file name
    for (uint32_t i = 0; i < name_len; ++i) {
        file_name[i] = 0;
        for (int b = 0; b < 8; ++b, ++bit_idx)
            file_name[i] |= (pixel_data[bit_idx] & 1) << b;
    }
    file_name[name_len] = '\0';

    // Decode file size
    *file_size = 0;
    for (int b = 0; b < 32; ++b, ++bit_idx)
        *file_size |= (pixel_data[bit_idx] & 1) << b;
    if (*file_size > file_data_bufsize) *file_size = file_data_bufsize;

    // Decode file content
    for (uint32_t i = 0; i < *file_size; ++i) {
        file_data[i] = 0;
        for (int b = 0; b < 8; ++b, ++bit_idx)
            file_data[i] |= (pixel_data[bit_idx] & 1) << b;
    }

    MessageBoxA(NULL, "File extracted successfully from the image.", "SUCCESS", MB_OK);
}
#pragma endregion
// generate de ai

//// Padded LSB: Adds padding bits to the message
//void hide_message_with_padding(const uint8_t* image_path, const uint8_t* message, const uint8_t* output_path) {
//	BMPImage* bmp = load_bmp(image_path);
//	if (!bmp) {
//		printf("Error: Unable to load image.\n");
//		return;
//	}
//	uint32_t msg_len = strlen(message) + 1; // Include null terminator
//	uint32_t pixel_count = bmp->header.image_size;
//	// Calculate padding
//	uint32_t padding = (8 - (msg_len % 8)) % 8;
//	uint32_t total_bits = msg_len * 8 + padding;
//	if (total_bits > pixel_count * 8) {
//		printf("Error: Message too long for padded LSB.\n");
//		free_bmp(bmp);
//		return;
//	}
//	// Hide message with padding
//	for (uint32_t i = 0; i < msg_len; ++i) {
//		for (int bit = 0; bit < 8; ++bit) {
//			uint32_t index = i * 8 + bit;
//			bmp->pixel_data[index] = (bmp->pixel_data[index] & ~1) | ((message[i] >> bit) & 1);
//		}
//	}
//	// Add padding bits
//	for (uint32_t i = msg_len * 8; i < total_bits; ++i) {
//		bmp->pixel_data[i] &= ~1; // Set to zero
//	}
//	save_bmp(output_path, bmp);
//	free_bmp(bmp);
//}   
//
//
//void reveal_message_with_padding(const uint8_t* image_path, uint8_t* output, uint32_t max_len) {
//	BMPImage* bmp = load_bmp(image_path);
//	if (!bmp) {
//		printf("Error: Unable to load image.\n");
//		return;
//	}
//	uint32_t i, bit;
//	for (i = 0; i < max_len; ++i) {
//		output[i] = 0;
//		for (bit = 0; bit < 8; ++bit) {
//			uint32_t index = i * 8 + bit;
//			output[i] |= (bmp->pixel_data[index] & 1) << bit;
//		}
//		if (output[i] == '\0') break;
//	}
//	output[max_len - 1] = '\0';
//	free_bmp(bmp);
//}   



//void hide_message_with_encryption(const uint8_t* image_path, const uint8_t* message, const uint8_t* output_path) {
//	// Encrypt the message using a simple XOR cipher
//	uint8_t key = 'K'; // Simple key for XOR encryption
//	uint8_t encrypted_message[256];
//	uint32_t msg_len = strlen(message) + 1; // Include null terminator
//	for (uint32_t i = 0; i < msg_len; ++i) {
//		encrypted_message[i] = message[i] ^ key;
//	}
//	// Hide the encrypted message in the image
//	//hideMessage(image_path, encrypted_message, output_path);
//}
//
//
//void reveal_message_with_decryption(const uint8_t* image_path, uint8_t* output, uint32_t max_len) {
//	// Reveal the encrypted message from the image
//	//revealMessage(image_path, output, max_len);
//	// Decrypt the message using the same XOR cipher
//	uint8_t key = 'K'; // Simple key for XOR encryption
//	for (uint32_t i = 0; i < strlen(output); ++i) {
//		output[i] ^= key;
//	}
//}
//
//void hide_message_with_compression(const uint8_t* image_path, const uint8_t* message, const uint8_t* output_path) {
//	// Compress the message using a simple RLE (Run-Length Encoding) algorithm
//	uint8_t compressed_message[256];
//	uint32_t compressed_len = 0;
//	uint32_t msg_len = strlen(message);
//	for (uint32_t i = 0; i < msg_len; ++i) {
//		uint8_t count = 1;
//		while (i + 1 < msg_len && message[i] == message[i + 1]) {
//			count++;
//			i++;
//		}
//		compressed_message[compressed_len++] = count;
//		compressed_message[compressed_len++] = message[i];
//	}
//	compressed_message[compressed_len] = '\0';
//	// Hide the compressed message in the image
//	//hideMessage(image_path, compressed_message, output_path);
//}
//
//
//void reveal_message_with_decompression(const uint8_t* image_path, uint8_t* output, uint32_t max_len) {
//	// Reveal the compressed message from the image
//	//revealMessage(image_path, output, max_len);
//	// Decompress the message using the same RLE algorithm
//	uint32_t decompressed_len = 0;
//	for (uint32_t i = 0; i < strlen(output); i += 2) {
//		uint8_t count = output[i];
//		uint8_t value = output[i + 1];
//		for (uint8_t j = 0; j < count; ++j) {
//			output[decompressed_len++] = value;
//		}
//	}
//	output[decompressed_len] = '\0';
//}
