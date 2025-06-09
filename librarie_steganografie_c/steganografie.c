#include "bmp_handler.h"
#include "steganografie.h"
#include <stdio.h>
#include <string.h>
#include <windows.h>


#define HEADER_METHOD_SIZE                  5
#define HEADER_FILE_SIZE				    12
static uint8_t extracted_filename[256];

#pragma region private_functions
static uint32_t rand32(void) {
    return ((uint32_t)rand() << 16) ^ ((uint32_t)rand());
}

static uint32_t password_to_seed(const uint8_t* password) {
    uint32_t hash = 5381;
    int c;

    while ((c = *password++)) {
        hash = ((hash << 5) + hash) + c; /* hash * 33 + c */
    }

    return hash;
}

// Fisher-Yates shuffle to generate pixel order
static void generate_pixel_order(uint32_t* order, uint32_t count, uint32_t seed) {
    // Initialize with sequential indices
    for (uint32_t i = 0; i < count; i++) {
        order[i] = i;
    }

    srand(seed);
    for (uint32_t i = count - 1; i > 0; --i) {
        uint32_t j = rand32() % (i + 1);
        // inline swap
        uint32_t tmp = order[i];
        order[i] = order[j];
        order[j] = tmp;
    }
}
#pragma endregion

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
    uint32_t  bit_idx = 0;
    char      magic[4] = { 0 };
    uint8_t   method = 0;
    uint32_t  name_len = 0;
    uint32_t  extracted_size = 0;
    // Decode magic header
    for (int i = 0; i < 4; ++i) {
        magic[i] = 0;
        for (int b = 0; b < 8; ++b, ++bit_idx)
            magic[i] |= (pixel_data[bit_idx * 3 + 2] & 1) << b;
    }
    // Decode method byte
    for (int b = 0; b < 8; ++b, ++bit_idx)
        method |= (pixel_data[bit_idx * 3 + 2] & 1) << b;

    if (memcmp(magic, "STGF", 4) != 0 || method != 0x01) {
        MessageBoxA(NULL, "No file hidden with Standard LSB in this image.", "ERROR", MB_OK);
        return;
    }

    // Decode name length
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

    // 5) Decode file size
    for (int b = 0; b < 32; ++b, ++bit_idx) {
        uint32_t pix = bit_idx;
        if (pix * 3 + 2 >= pixel_data_size) break;
        extracted_size |= (pixel_data[pix * 3 + 2] & 1) << b;
    }
    if (extracted_size > file_data_bufsize) {
        MessageBoxA(NULL, "Error: Extracted file size exceeds buffer capacity.", "ERROR", MB_OK);
        return;
    }

    // 6) Decode the file content *only* up to extracted_size
    for (uint32_t i = 0; i < extracted_size; ++i) {
        file_data[i] = 0;
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            uint32_t pix = bit_idx;
            if (pix * 3 + 2 >= pixel_data_size) {
                // truncated content
                break;
            }
            file_data[i] |= (pixel_data[pix * 3 + 2] & 1) << b;
        }
    }

    // 7) Return the real size
    *file_size = extracted_size;

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

#pragma region shuffle_standard

__declspec(dllexport) void hide_message_shuffle(
    const uint8_t* pixel_data, uint32_t pixel_data_size,
    const uint8_t* message, const uint8_t* password,
    uint8_t* output_data)
{
    uint32_t msg_len = strlen((const char*)message) + 1; // Include null terminator
    uint32_t pixel_count = pixel_data_size / 3; // 3 bytes per pixel

    if (msg_len * 8 > pixel_count) {
        MessageBoxA(NULL, "Error: Message is too long to fit in the image.", "ERROR", MB_OK);
        return;
    }

    memcpy(output_data, pixel_data, pixel_data_size);

    // Generate pixel order based on password
    uint32_t seed = password_to_seed(password);
    uint32_t* pixel_order = (uint32_t*)malloc(pixel_count * sizeof(uint32_t));
    generate_pixel_order(pixel_order, pixel_count, seed);

    // Embed message using scrambled order
    uint32_t bit_idx = 0;
    for (uint32_t i = 0; i < msg_len; ++i) {
        for (int bit = 0; bit < 8; ++bit, ++bit_idx) {
            uint32_t pixel = pixel_order[bit_idx];
            if (pixel * 3 + 2 >= pixel_data_size) break;
            // Hide in blue channel (offset +2)
            output_data[pixel * 3 + 2] = (output_data[pixel * 3 + 2] & ~1) | ((message[i] >> bit) & 1);
        }
    }

    free(pixel_order);
    MessageBoxA(NULL, "Message hidden successfully with password protection.", "SUCCESS", MB_OK);
}

__declspec(dllexport) void reveal_message_shuffle(
    const uint8_t* pixel_data, uint32_t pixel_data_size,
    const uint8_t* password,
    uint8_t* output, uint32_t max_len)
{
    uint32_t pixel_count = pixel_data_size / 3;

    // Generate the same pixel order used for hiding
    uint32_t seed = password_to_seed(password);
    uint32_t* pixel_order = (uint32_t*)malloc(pixel_count * sizeof(uint32_t));
    generate_pixel_order(pixel_order, pixel_count, seed);

    uint32_t bit_idx = 0;
    for (uint32_t i = 0; i < max_len; ++i) {
        output[i] = 0;
        for (int bit = 0; bit < 8; ++bit, ++bit_idx) {
            uint32_t pixel = pixel_order[bit_idx];
            if (pixel * 3 + 2 >= pixel_data_size || pixel >= pixel_count) {
                output[i] = '\0';
                free(pixel_order);
                return;
            }
            output[i] |= (pixel_data[pixel * 3 + 2] & 1) << bit;
        }
        if (output[i] == '\0') break;
    }
    output[max_len - 1] = '\0';
    free(pixel_order);
}

__declspec(dllexport) void hide_file_shuffle(
    const uint8_t* pixel_data,
    uint32_t        pixel_data_size,
    const uint8_t* file_name,
    const uint8_t* file_data,
    uint32_t        file_size,
    const uint8_t* password,
    uint8_t* output_data)
{
    const char magic[4] = { 'S','T','G','F' };
    const uint8_t method = 0x03;       // password-protected file
    const size_t  name_len = strlen((const char*)file_name);
    uint32_t      pixel_count = pixel_data_size / 3;
    uint64_t      total_bits =
        (4 + 1) * 8                 // magic + method
        + 32                          // name length
        + name_len * 8                // file name
        + 32                          // file size
        + (uint64_t)file_size * 8;    // file contents

    if (total_bits > pixel_count) {
        MessageBoxA(NULL, "Error: File is too large to fit in the image.", "ERROR", MB_OK);
        return;
    }

    // Copy the cover image’s pixels to the output buffer
    memcpy(output_data, pixel_data, pixel_data_size);

    // Build our 5-byte header array
    uint8_t header[5];
    memcpy(header, magic, 4);
    header[4] = method;

    // Generate shuffle order
    uint32_t seed = password_to_seed(password);
    uint32_t* pixel_order = malloc(pixel_count * sizeof(uint32_t));
    generate_pixel_order(pixel_order, pixel_count, seed);

    uint64_t bit_idx = 0;

    // 1) Encode magic+method header (5 bytes → 40 bits)
    for (int i = 0; i < 5; ++i) {
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            if (bit_idx >= pixel_count) goto cleanup;
            uint32_t pixel = pixel_order[bit_idx];
            uint8_t  bit = (header[i] >> b) & 1;
            output_data[pixel * 3 + 2] = (output_data[pixel * 3 + 2] & ~1) | bit;
        }
    }

    // 2) Encode file name length (32 bits)
    for (int b = 0; b < 32; ++b, ++bit_idx) {
        if (bit_idx >= pixel_count) goto cleanup;
        uint32_t pixel = pixel_order[bit_idx];
        uint8_t  bit = (name_len >> b) & 1;
        output_data[pixel * 3 + 2] = (output_data[pixel * 3 + 2] & ~1) | bit;
    }

    // 3) Encode file name bytes
    for (size_t i = 0; i < name_len; ++i) {
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            if (bit_idx >= pixel_count) goto cleanup;
            uint32_t pixel = pixel_order[bit_idx];
            uint8_t  bit = (file_name[i] >> b) & 1;
            output_data[pixel * 3 + 2] = (output_data[pixel * 3 + 2] & ~1) | bit;
        }
    }

    // 4) Encode file size (32 bits)
    for (int b = 0; b < 32; ++b, ++bit_idx) {
        if (bit_idx >= pixel_count) goto cleanup;
        uint32_t pixel = pixel_order[bit_idx];
        uint8_t  bit = (file_size >> b) & 1;
        output_data[pixel * 3 + 2] = (output_data[pixel * 3 + 2] & ~1) | bit;
    }

    // 5) Encode file content
    for (uint32_t i = 0; i < file_size; ++i) {
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            if (bit_idx >= pixel_count) goto cleanup;
            uint32_t pixel = pixel_order[bit_idx];
            uint8_t  bit = (file_data[i] >> b) & 1;
            output_data[pixel * 3 + 2] = (output_data[pixel * 3 + 2] & ~1) | bit;
        }
    }

cleanup:
    free(pixel_order);
    MessageBoxA(NULL, "File hidden successfully with password protection.", "SUCCESS", MB_OK);
}


__declspec(dllexport) void extract_file_shuffle(
    const uint8_t* pixel_data,
    uint32_t       pixel_data_size,
    const uint8_t* password,
    uint8_t* file_name,
    uint32_t       file_name_bufsize,
    uint8_t* file_data,
    uint32_t       file_data_bufsize,
    uint32_t* file_size)
{
    uint32_t pixel_count = pixel_data_size / 3;
    uint32_t seed = password_to_seed(password);
    uint32_t* pixel_order = malloc(pixel_count * sizeof(uint32_t));
    if (!pixel_order) {
        MessageBoxA(NULL, "Memory allocation failed.", "ERROR", MB_OK);
        return;
    }
    generate_pixel_order(pixel_order, pixel_count, seed);

    // 1) Decode magic + method (5 bytes → 40 bits)
    uint8_t header[5] = { 0 };
    uint64_t bit_idx = 0;
    for (int i = 0; i < 5; ++i) {
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            if (bit_idx >= pixel_count) {
                MessageBoxA(NULL, "Corrupted or incomplete data.", "ERROR", MB_OK);
                free(pixel_order);
                return;
            }
            uint32_t pix = pixel_order[bit_idx];
            header[i] |= (pixel_data[pix * 3 + 2] & 1) << b;
        }
    }
    if (memcmp(header, "STGF", 4) != 0 || header[4] != 0x03) {
        MessageBoxA(NULL, "No password‐protected file found or wrong password.", "ERROR", MB_OK);
        free(pixel_order);
        return;
    }

    // 2) Decode filename length (32 bits)
    uint32_t name_len = 0;
    for (int b = 0; b < 32; ++b, ++bit_idx) {
        if (bit_idx >= pixel_count) break;
        uint32_t pix = pixel_order[bit_idx];
        name_len |= (pixel_data[pix * 3 + 2] & 1) << b;
    }
    if (name_len >= file_name_bufsize) name_len = file_name_bufsize - 1;

    // 3) Decode filename
    for (uint32_t i = 0; i < name_len; ++i) {
        file_name[i] = 0;
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            if (bit_idx >= pixel_count) break;
            uint32_t pix = pixel_order[bit_idx];
            file_name[i] |= (pixel_data[pix * 3 + 2] & 1) << b;
        }
    }
    file_name[name_len] = '\0';

    // 4) Decode file size (32 bits)
    *file_size = 0;
    for (int b = 0; b < 32; ++b, ++bit_idx) {
        if (bit_idx >= pixel_count) break;
        uint32_t pix = pixel_order[bit_idx];
        *file_size |= (pixel_data[pix * 3 + 2] & 1) << b;
    }
    if (*file_size > file_data_bufsize) {
        MessageBoxA(NULL, "Extracted file size exceeds buffer.", "ERROR", MB_OK);
        free(pixel_order);
        return;
    }

    // 5) Decode file contents
    for (uint32_t i = 0; i < *file_size; ++i) {
        file_data[i] = 0;
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            if (bit_idx >= pixel_count) break;
            uint32_t pix = pixel_order[bit_idx];
            file_data[i] |= (pixel_data[pix * 3 + 2] & 1) << b;
        }
    }

    free(pixel_order);
    MessageBoxA(NULL, "File extracted successfully with password.", "SUCCESS", MB_OK);
}


#pragma endregion

#pragma region shuffle_multichannel

__declspec(dllexport) void hide_message_multichannel_shuffle(
    const uint8_t* pixel_data,
    uint32_t       pixel_data_size,
    const uint8_t* message,
    const uint8_t* password,
    uint8_t* output_data)
{
    uint32_t msg_len = (uint32_t)strlen((const char*)message) + 1;
    uint64_t total_bits = (uint64_t)msg_len * 8;
    if (total_bits > pixel_data_size) {
        MessageBoxA(NULL, "Message too long for multichannel+shuffle.", "ERROR", MB_OK);
        return;
    }

    memcpy(output_data, pixel_data, pixel_data_size);

    // Shuffle across *bytes*
    uint32_t seed = password_to_seed(password);
    uint32_t* order = malloc(pixel_data_size * sizeof(uint32_t));
    if (!order) {
        MessageBoxA(NULL, "Memory allocation failed.", "ERROR", MB_OK);
        return;
    }
    generate_pixel_order(order, pixel_data_size, seed);

    for (uint64_t bit_idx = 0; bit_idx < total_bits; ++bit_idx) {
        uint8_t bit = (message[bit_idx / 8] >> (bit_idx % 8)) & 1;
        uint32_t idx = order[bit_idx];
        output_data[idx] = (output_data[idx] & ~1) | bit;
    }

    free(order);
    MessageBoxA(NULL, "Message hidden (multichannel + shuffle).", "SUCCESS", MB_OK);
}

__declspec(dllexport) void reveal_message_multichannel_shuffle(
    const uint8_t* pixel_data,
    uint32_t       pixel_data_size,
    const uint8_t* password,
    uint8_t* output,
    uint32_t       max_len)
{
    uint32_t seed = password_to_seed(password);
    uint32_t* order = malloc(pixel_data_size * sizeof(uint32_t));
    if (!order) {
        MessageBoxA(NULL, "Memory allocation failed.", "ERROR", MB_OK);
        return;
    }
    generate_pixel_order(order, pixel_data_size, seed);

    uint64_t bit_idx = 0;
    uint32_t out_idx = 0, bit_in_char = 0;
    output[0] = 0;

    while (bit_idx < pixel_data_size && out_idx < max_len - 1) {
        uint32_t idx = order[bit_idx++];
        output[out_idx] |= (pixel_data[idx] & 1) << bit_in_char;
        if (++bit_in_char == 8) {
            if (output[out_idx] == '\0') break;
            bit_in_char = 0;
            output[++out_idx] = 0;
        }
    }
    output[max_len - 1] = '\0';

    free(order);
    MessageBoxA(NULL, "Message revealed (multichannel + shuffle).", "SUCCESS", MB_OK);
}

__declspec(dllexport) void hide_file_multichannel_shuffle(
    const uint8_t* pixel_data,
    uint32_t       pixel_data_size,
    const uint8_t* file_name,
    const uint8_t* file_data,
    uint32_t       file_size,
    const uint8_t* password,
    uint8_t* output_data)
{
    const char magic[4] = { 'S','T','G','F' };
    const uint8_t method = 0x04;  // new method code
    size_t name_len = strlen((const char*)file_name);
    uint64_t total_bits =
        (4 + 1) * 8
        + 32
        + name_len * 8
        + 32
        + (uint64_t)file_size * 8;

    if (total_bits > pixel_data_size) {
        MessageBoxA(NULL, "File too large for multichannel+shuffle.", "ERROR", MB_OK);
        return;
    }

    memcpy(output_data, pixel_data, pixel_data_size);

    // 5-byte header (magic + method)
    uint8_t header[5];
    memcpy(header, magic, 4);
    header[4] = method;

    uint32_t seed = password_to_seed(password);
    uint32_t* order = malloc(pixel_data_size * sizeof(uint32_t));
    if (!order) {
        MessageBoxA(NULL, "Memory allocation failed.", "ERROR", MB_OK);
        return;
    }
    generate_pixel_order(order, pixel_data_size, seed);

    uint64_t bit_idx = 0;
    // encode header
    for (int i = 0; i < 5; ++i)
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            uint32_t idx = order[bit_idx];
            uint8_t  bit = (header[i] >> b) & 1;
            output_data[idx] = (output_data[idx] & ~1) | bit;
        }
    // filename length
    for (int b = 0; b < 32; ++b, ++bit_idx) {
        uint32_t idx = order[bit_idx];
        uint8_t  bit = (name_len >> b) & 1;
        output_data[idx] = (output_data[idx] & ~1) | bit;
    }
    // filename
    for (size_t i = 0; i < name_len; ++i)
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            uint32_t idx = order[bit_idx];
            uint8_t  bit = (file_name[i] >> b) & 1;
            output_data[idx] = (output_data[idx] & ~1) | bit;
        }
    // file size
    for (int b = 0; b < 32; ++b, ++bit_idx) {
        uint32_t idx = order[bit_idx];
        uint8_t  bit = (file_size >> b) & 1;
        output_data[idx] = (output_data[idx] & ~1) | bit;
    }
    // file contents
    for (uint32_t i = 0; i < file_size; ++i)
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            uint32_t idx = order[bit_idx];
            uint8_t  bit = (file_data[i] >> b) & 1;
            output_data[idx] = (output_data[idx] & ~1) | bit;
        }

    free(order);
    MessageBoxA(NULL, "File hidden (multichannel + shuffle).", "SUCCESS", MB_OK);
}

__declspec(dllexport) void extract_file_multichannel_shuffle(
    const uint8_t* pixel_data,
    uint32_t       pixel_data_size,
    const uint8_t* password,
    uint8_t* file_name,
    uint32_t       file_name_bufsize,
    uint8_t* file_data,
    uint32_t       file_data_bufsize,
    uint32_t* file_size)
{
    const char magic[4] = { 'S','T','G','F' };
    const uint8_t expected_method = 0x04;
    uint32_t seed = password_to_seed(password);
    uint32_t* order = malloc(pixel_data_size * sizeof(uint32_t));
    if (!order) {
        MessageBoxA(NULL, "Memory allocation failed.", "ERROR", MB_OK);
        return;
    }
    generate_pixel_order(order, pixel_data_size, seed);

    // header
    uint8_t header[5] = { 0 };
    uint64_t bit_idx = 0;
    for (int i = 0; i < 5; ++i)
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            uint32_t idx = order[bit_idx];
            header[i] |= (pixel_data[idx] & 1) << b;
        }
    if (memcmp(header, magic, 4) != 0 || header[4] != expected_method) {
        MessageBoxA(NULL, "No password‐protected multichannel file found or wrong password.", "ERROR", MB_OK);
        free(order);
        return;
    }

    // filename length
    uint32_t name_len = 0;
    for (int b = 0; b < 32; ++b, ++bit_idx) {
        uint32_t idx = order[bit_idx];
        name_len |= (pixel_data[idx] & 1) << b;
    }
    if (name_len >= file_name_bufsize) name_len = file_name_bufsize - 1;

    // filename
    for (uint32_t i = 0; i < name_len; ++i) {
        file_name[i] = 0;
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            uint32_t idx = order[bit_idx];
            file_name[i] |= (pixel_data[idx] & 1) << b;
        }
    }
    file_name[name_len] = '\0';

    // file size
    *file_size = 0;
    for (int b = 0; b < 32; ++b, ++bit_idx) {
        uint32_t idx = order[bit_idx];
        *file_size |= (pixel_data[idx] & 1) << b;
    }
    if (*file_size > file_data_bufsize) {
        MessageBoxA(NULL, "Extracted file size exceeds buffer.", "ERROR", MB_OK);
        free(order);
        return;
    }

    // file contents
    for (uint32_t i = 0; i < *file_size; ++i) {
        file_data[i] = 0;
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            uint32_t idx = order[bit_idx];
            file_data[i] |= (pixel_data[idx] & 1) << b;
        }
    }

    free(order);
    MessageBoxA(NULL, "File extracted (multichannel + shuffle).", "SUCCESS", MB_OK);
}

#pragma endregion

#pragma region wav_lsb
#define ERR(msg) do { MessageBoxA(NULL, msg, "Error", MB_OK | MB_ICONERROR); return; } while(0)

static uint32_t read_le32(const uint8_t* p) {
    return (uint32_t)p[0] | ((uint32_t)p[1] << 8)
        | ((uint32_t)p[2] << 16) | ((uint32_t)p[3] << 24);
}

static uint16_t read_le16(const uint8_t* p) {
    return (uint16_t)p[0] | ((uint16_t)p[1] << 8);
}


__declspec(dllexport)
void hideMessageInWav(
    const uint8_t* wav_data,
    uint32_t       wav_data_size,
    const uint8_t* message,
    uint8_t* output_data
) {
    if (!wav_data || !message || !output_data) ERR("Null pointer provided.");

    // must have at least RIFF + fmt + data headers
    if (wav_data_size < 44) ERR("Data too small to be a valid WAV.");

    // verify RIFF/WAVE signature
    if (memcmp(wav_data, "RIFF", 4) != 0 || memcmp(wav_data + 8, "WAVE", 4) != 0)
        ERR("Not a valid RIFF/WAVE file.");

    // scan for fmt & data
    uint32_t offset = 12;
    uint16_t audio_format = 0, bits_per_sample = 0;
    uint32_t data_off = 0, data_sz = 0;
    while (offset + 8 <= wav_data_size) {
        uint32_t chunk_id = read_le32(wav_data + offset);
        uint32_t chunk_sz = read_le32(wav_data + offset + 4);
        uint32_t next_chunk = offset + 8 + chunk_sz + (chunk_sz & 1);

        if (memcmp(wav_data + offset, "fmt ", 4) == 0) {
            if (chunk_sz < 16) ERR("Malformed fmt chunk.");
            audio_format = read_le16(wav_data + offset + 8 + 0);
            bits_per_sample = read_le16(wav_data + offset + 8 + 14);
        }
        else if (memcmp(wav_data + offset, "data", 4) == 0) {
            data_off = offset + 8;
            data_sz = chunk_sz;
            break;
        }

        if (next_chunk <= offset || next_chunk > wav_data_size) break;
        offset = next_chunk;
    }

    if (!data_off)                ERR("No data chunk found.");
    if (audio_format != 1)        ERR("Only PCM WAV supported.");
    if (bits_per_sample != 8 && bits_per_sample != 16)
        ERR("Only 8- or 16-bit PCM supported.");

    uint32_t bytes_per_sample = bits_per_sample / 8;
    uint32_t total_samples = data_sz / bytes_per_sample;
    uint32_t msg_len = (uint32_t)strlen((const char*)message) + 1;
    if (msg_len * 8 > total_samples) ERR("Message too long for this WAV.");

    // copy entire file to output
    memcpy(output_data, wav_data, wav_data_size);

    // embed: for each bit, flip LSB of the first byte of each sample
    uint32_t bit_idx = 0;
    for (uint32_t i = 0; i < msg_len; ++i) {
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            uint32_t sample_pos = data_off + bit_idx * bytes_per_sample;
            uint8_t  bit = (message[i] >> b) & 1;
            output_data[sample_pos] = (output_data[sample_pos] & ~1) | bit;
        }
    }

    MessageBoxA(NULL, "Message hidden successfully in WAV.", "SUCCESS", MB_OK);
}

__declspec(dllexport)
void revealMessageFromWav(
    const uint8_t* wav_data,
    uint32_t       wav_data_size,
    uint8_t* output,
    uint32_t       max_len
) {
    if (!wav_data || !output) ERR("Null pointer provided.");
    if (wav_data_size < 44) ERR("Data too small to be a valid WAV.");

    if (memcmp(wav_data, "RIFF", 4) != 0 || memcmp(wav_data + 8, "WAVE", 4) != 0)
        ERR("Not a valid RIFF/WAVE file.");

    uint32_t offset = 12;
    uint16_t audio_format = 0, bits_per_sample = 0;
    uint32_t data_off = 0, data_sz = 0;
    while (offset + 8 <= wav_data_size) {
        uint32_t id = read_le32(wav_data + offset);
        uint32_t sz = read_le32(wav_data + offset + 4);
        uint32_t nxt = offset + 8 + sz + (sz & 1);

        if (memcmp(wav_data + offset, "fmt ", 4) == 0) {
            if (sz < 16) ERR("Malformed fmt chunk.");
            audio_format = read_le16(wav_data + offset + 8 + 0);
            bits_per_sample = read_le16(wav_data + offset + 8 + 14);
        }
        else if (memcmp(wav_data + offset, "data", 4) == 0) {
            data_off = offset + 8;
            data_sz = sz;
            break;
        }

        if (nxt <= offset || nxt > wav_data_size) break;
        offset = nxt;
    }

    if (!data_off)           ERR("No data chunk found.");
    if (audio_format != 1)   ERR("Only PCM WAV supported.");
    if (bits_per_sample != 8 && bits_per_sample != 16)
        ERR("Only 8- or 16-bit PCM supported.");

    uint32_t bytes_per_sample = bits_per_sample / 8;
    uint32_t total_samples = data_sz / bytes_per_sample;

    uint32_t bit_idx = 0;
    for (uint32_t i = 0; i < max_len; ++i) {
        output[i] = 0;
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            if (bit_idx >= total_samples) {
                output[i] = '\0';
                return;
            }
            uint32_t pos = data_off + bit_idx * bytes_per_sample;
            output[i] |= (wav_data[pos] & 1) << b;
        }
        if (output[i] == '\0') break;
    }
    output[max_len - 1] = '\0';
    MessageBoxA(NULL, "Message revealed successfully from WAV.", "SUCCESS", MB_OK);
}

__declspec(dllexport)
void hideFileInWav(
    const uint8_t* wav_data,
    uint32_t       wav_data_size,
    const uint8_t* file_name,
    const uint8_t* file_data,
    uint32_t       file_size,
    uint8_t* output_data
) {
    if (!wav_data || !file_name || !file_data || !output_data) ERR("Null pointer provided.");

    // Must be at least RIFF header + fmt + data headers
    if (wav_data_size < 44) ERR("Data too small to be valid WAV.");

    // 1) Validate RIFF/WAVE
    if (memcmp(wav_data, "RIFF", 4) != 0 ||
        memcmp(wav_data + 8, "WAVE", 4) != 0)
        ERR("Not a valid RIFF/WAVE file.");

    // 2) Walk chunks to find fmt & data
    uint32_t offset = 12;
    uint16_t audio_format = 0, bits_per_sample = 0;
    uint32_t data_off = 0, data_sz = 0;

    while (offset + 8 <= wav_data_size) {
        uint32_t id = read_le32(wav_data + offset);
        uint32_t sz = read_le32(wav_data + offset + 4);
        uint32_t next = offset + 8 + sz + (sz & 1);
        if (memcmp(wav_data + offset, "fmt ", 4) == 0) {
            if (sz < 16) ERR("Malformed fmt chunk.");
            audio_format = read_le16(wav_data + offset + 8 + 0);
            bits_per_sample = read_le16(wav_data + offset + 8 + 14);
        }
        else if (memcmp(wav_data + offset, "data", 4) == 0) {
            data_off = offset + 8;
            data_sz = sz;
            break;
        }
        if (next <= offset || next > wav_data_size) break;
        offset = next;
    }

    if (!data_off)           ERR("No data chunk found.");
    if (audio_format != 1)   ERR("Only PCM WAV supported.");
    if (bits_per_sample != 8 && bits_per_sample != 16)
        ERR("Only 8- or 16-bit PCM supported.");

    uint32_t bps = bits_per_sample / 8;
    uint32_t total_samples = data_sz / bps;
    uint32_t capacity_bytes = total_samples / 8;         // one LSB per sample
    uint32_t name_len = (uint32_t)strlen((const char*)file_name);
    uint64_t needed = 4 + 1                   // STGF + method
        + 4                      // name length
        + name_len
        + 4                      // data size
        + file_size;
    if (needed > capacity_bytes) ERR("Payload + filename too large for WAV.");

    // 3) Copy entire WAV into output
    memcpy(output_data, wav_data, wav_data_size);

    // 4) LSB-embed in data chunk
    uint32_t bit_idx = 0;
    // a) “STGF”
    const char magic[4] = { 'S','T','G','F' };
    for (int i = 0; i < 4; ++i)
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            uint32_t pos = data_off + bit_idx * bps;
            uint8_t  bit = (magic[i] >> b) & 1;
            output_data[pos] = (output_data[pos] & ~1) | bit;
        }
    // b) method = 0x01
    for (int b = 0; b < 8; ++b, ++bit_idx) {
        uint32_t pos = data_off + bit_idx * bps;
        output_data[pos] = (output_data[pos] & ~1) | ((0x01 >> b) & 1);
    }
    // c) name length (32-bit LE)
    for (int b = 0; b < 32; ++b, ++bit_idx) {
        uint32_t pos = data_off + bit_idx * bps;
        output_data[pos] = (output_data[pos] & ~1) | ((name_len >> b) & 1);
    }
    // d) filename bytes
    for (uint32_t i = 0; i < name_len; ++i)
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            uint32_t pos = data_off + bit_idx * bps;
            output_data[pos] = (output_data[pos] & ~1) | ((file_name[i] >> b) & 1);
        }
    // e) payload size (32-bit LE)
    for (int b = 0; b < 32; ++b, ++bit_idx) {
        uint32_t pos = data_off + bit_idx * bps;
        output_data[pos] = (output_data[pos] & ~1) | ((file_size >> b) & 1);
    }
    // f) payload bytes
    for (uint32_t i = 0; i < file_size; ++i)
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            uint32_t pos = data_off + bit_idx * bps;
            output_data[pos] = (output_data[pos] & ~1) | ((file_data[i] >> b) & 1);
        }

    MessageBoxA(NULL, "File hidden successfully in WAV.", "SUCCESS", MB_OK);
}

__declspec(dllexport)
void extractFileFromWav(
    const uint8_t* wav_data,
    uint32_t       wav_data_size,
    uint8_t* file_name,
    uint32_t       file_name_bufsize,
    uint8_t* file_data,
    uint32_t       file_data_bufsize,
    uint32_t* file_size
) {
    if (!wav_data || !file_name || !file_data || !file_size) ERR("Null pointer provided.");
    if (wav_data_size < 44) ERR("Data too small to be valid WAV.");
    if (memcmp(wav_data, "RIFF", 4) != 0 || memcmp(wav_data + 8, "WAVE", 4) != 0)
        ERR("Not a valid WAV.");

    // locate data chunk
    uint32_t offset = 12, data_off = 0, data_sz = 0; uint16_t fmt = 0, bps16 = 0;
    while (offset + 8 <= wav_data_size) {
        uint32_t id = read_le32(wav_data + offset);
        uint32_t sz = read_le32(wav_data + offset + 4);
        uint32_t nxt = offset + 8 + sz + (sz & 1);
        if (memcmp(wav_data + offset, "fmt ", 4) == 0 && sz >= 16) {
            fmt = read_le16(wav_data + offset + 8);
            bps16 = read_le16(wav_data + offset + 8 + 14);
        }
        else if (memcmp(wav_data + offset, "data", 4) == 0) {
            data_off = offset + 8; data_sz = sz; break;
        }
        if (nxt <= offset || nxt > wav_data_size) break;
        offset = nxt;
    }
    if (!data_off) ERR("No data chunk found.");
    if (fmt != 1) ERR("Only PCM WAV supported.");
    if (bps16 != 8 && bps16 != 16) ERR("Only 8/16-bit supported.");

    uint32_t bps = bps16 / 8;
    uint32_t samples = data_sz / bps;
    uint32_t cap_bytes = samples / 8;

    // 1) read magic
    uint32_t bit_idx = 0; char magic[4] = { 0 };
    for (int i = 0; i < 4; ++i)
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            uint32_t pos = data_off + bit_idx * bps;
            magic[i] |= (wav_data[pos] & 1) << b;
        }
    // 2) method
    uint8_t method = 0;
    for (int b = 0; b < 8; ++b, ++bit_idx) {
        uint32_t pos = data_off + bit_idx * bps;
        method |= (wav_data[pos] & 1) << b;
    }
    if (memcmp(magic, "STGF", 4) != 0 || method != 0x01)
        ERR("No STGF payload or wrong method.");

    // 3) name length
    uint32_t name_len = 0;
    for (int b = 0; b < 32; ++b, ++bit_idx) {
        uint32_t pos = data_off + bit_idx * bps;
        name_len |= (wav_data[pos] & 1) << b;
    }
    if (name_len + 4 > cap_bytes) ERR("Malformed name length.");
    if (name_len >= file_name_bufsize) name_len = file_name_bufsize - 1;

    // 4) filename
    for (uint32_t i = 0; i < name_len; ++i) {
        uint8_t ch = 0;
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            uint32_t pos = data_off + bit_idx * bps;
            ch |= (wav_data[pos] & 1) << b;
        }
        file_name[i] = ch;
    }
    file_name[name_len] = '\0';

    // 5) payload size
    uint32_t payload_size = 0;
    for (int b = 0; b < 32; ++b, ++bit_idx) {
        uint32_t pos = data_off + bit_idx * bps;
        payload_size |= (wav_data[pos] & 1) << b;
    }
    if (payload_size + 4 > cap_bytes) ERR("Payload size exceeds capacity.");
    if (payload_size > file_data_bufsize) ERR("Output buffer too small.");

    // 6) payload bytes
    for (uint32_t i = 0; i < payload_size; ++i) {
        uint8_t ch = 0;
        for (int b = 0; b < 8; ++b, ++bit_idx) {
            uint32_t pos = data_off + bit_idx * bps;
            ch |= (wav_data[pos] & 1) << b;
        }
        file_data[i] = ch;
    }

    *file_size = payload_size;
    MessageBoxA(NULL, "File extracted from WAV successfully.", "SUCCESS", MB_OK);
}

#pragma endregion