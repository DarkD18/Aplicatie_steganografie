#include "bmp_handler.h"
#include "steganografie.h"
#include <stdio.h>
#include <string.h>
#include <windows.h>

void hideMessage(const char* image_path, const char* message, const char* output_path) {
    BMPImage* bmp = load_bmp(image_path);
    if (!bmp) {
        MessageBoxA(NULL, "Unable to load image.\n", "ERROR", MB_OK);
        return;
    }

    size_t msg_len = strlen(message) + 1; // Include null terminator
    size_t pixel_count = 0;
    char debug_msg[256]; // Buffer to hold the formatted message

    // Calculate pixel count with padding
    size_t row_size = ((bmp->header.width * bmp->header.bit_count + 31) / 32) * 4; // Row size with padding
    pixel_count = row_size * abs(bmp->header.height); // Total bytes in pixel data

    // Debug: Log pixel count and message length
    //snprintf(debug_msg, sizeof(debug_msg), "msg_len: %zu, pixel_count: %zu", msg_len, pixel_count);
    //MessageBoxA(NULL, debug_msg, "DEBUG", MB_OK);

    // Ensure there is enough space in the image
    if (msg_len * 8 > pixel_count * 8) {
        MessageBoxA(NULL, "Message is too long to fit in the image.\n", "ERROR", MB_OK);
        free_bmp(bmp);
        return;
    }

    // Bounds checking: Ensure we don't write beyond pixel_data
    size_t max_bits = pixel_count * 8; // Total available bits
    for (size_t i = 0; i < msg_len; ++i) {
        for (int bit = 0; bit < 8; ++bit) {
            size_t index = i * 8 + bit;
            if (index >= max_bits) {
                snprintf(debug_msg, sizeof(debug_msg), "Index out of bounds: %zu (max: %zu)", index, max_bits);
                MessageBoxA(NULL, debug_msg, "ERROR", MB_OK);
                free_bmp(bmp);
                return;
            }
            bmp->pixel_data[index] = (bmp->pixel_data[index] & ~1) | ((message[i] >> bit) & 1);
        }
    }
    // Save the modified BMP
    if (!save_bmp(output_path, bmp)) {
        MessageBoxA(NULL, "Failed to save BMP file.\n", "ERROR", MB_OK);
    }
    else {
        //MessageBoxA(NULL, "Message hidden successfully in the image.", "SUCCESS", MB_OK);
    }

    // Debug: Log before freeing memory
    //MessageBoxA(NULL, "Freeing BMP memory.", "DEBUG", MB_OK);

    free_bmp(bmp);
}

void revealMessage(const char* image_path, char* output, size_t max_len)
{
    BMPImage* bmp = load_bmp(image_path);
    if (!bmp) {
        printf("Error: Unable to load image.\n");
        return;
    }

    // Decode the message from the LSBs of the pixel data
    size_t i, bit;
    for (i = 0; i < max_len; ++i) {
        output[i] = 0;
        for (bit = 0; bit < 8; ++bit) {
            output[i] |= (bmp->pixel_data[i * 8 + bit] & 1) << bit;
        }
        // Stop if null terminator is found in the extracted message
        if (output[i] == '\0') 
        {
            break;
        }
        
    }

    output[max_len - 1] = '\0';

    free_bmp(bmp);
}


// Ascunde orice fișier într-o imagine BMP
void hide_file(const char* cover_path, const char* file_path, const char* output_path) 
{
    FILE* log = fopen("D:\\debug_hide.txt", "w");
    if (!log) {
        MessageBoxA(NULL, "Nu pot crea fisierul debug_hide.txt", "EROARE fopen", MB_OK);
        return;
    }
    fprintf(log, "== START hide_file ==\n");

    fprintf(log, "cover_path: %s\n", cover_path);
    fprintf(log, "file_path: %s\n", file_path);
    fprintf(log, "output_path: %s\n", output_path);

    BMPImage* bmp = load_bmp(cover_path);
    if (!bmp) {
        fprintf(log, "ERROR: Failed to load BMP\n");
        fclose(log);
        return;
    }
    fprintf(log, "Loaded BMP OK\n");
    fprintf(log, "Image size (from header): %u\n", bmp->header.image_size);

    FILE* file = fopen(file_path, "rb");
    if (!file) {
        printf("Error: Could not open file to hide.\n");
        free_bmp(bmp);
        return;
    }

    // 1. Get file size
    fseek(file, 0, SEEK_END);
    uint32_t file_size = ftell(file);
    rewind(file);
    printf("File size: %u bytes\n", file_size);

    // 2. Get file name
    const char* file_name = strrchr(file_path, '\\');
    if (!file_name) file_name = strrchr(file_path, '/');
    file_name = file_name ? file_name + 1 : file_path;

    uint32_t name_len = strlen(file_name);
    printf("File name: %s (len: %u)\n", file_name, name_len);

    // 3. Total bits needed = (4 + name_len + 4 + file_size) * 8
    size_t total_bytes = 4 + name_len + 4 + file_size;
    size_t total_bits = total_bytes * 8;
    if (total_bits > bmp->header.image_size) {
        printf("ERROR: Not enough space in cover image (%zu needed, %u available)\n", total_bits, bmp->header.image_size);
        fclose(file);
        free_bmp(bmp);
        return;
    }

    fprintf(log, "file_size = %u\n", file_size);
    fprintf(log, "file_name = %s\n", file_name);
    fprintf(log, "name_len = %u\n", name_len);
    fprintf(log, "total_bytes = %zu\n", total_bytes);
    fprintf(log, "image size = %u\n", bmp->header.image_size);

    if (bmp->header.image_size < total_bits) {
        printf("Error: Cover image too small.\n");
        fclose(file);
        free_bmp(bmp);
        return;
    }
    
    printf("Copying metadata + data into buffer...\n");

    uint8_t* all_data = (uint8_t*)malloc(total_bytes);
    if (!all_data) {
        printf("Error: Memory allocation failed.\n");
        fclose(file);
        free_bmp(bmp);
        return;
    }

    // 4. Write metadata to buffer
    memcpy(all_data, &name_len, 4);                     // nume_len
    memcpy(all_data + 4, file_name, name_len);          // nume
    memcpy(all_data + 4 + name_len, &file_size, 4);     // file_size
    size_t read_bytes = fread(all_data + 4 + name_len + 4, 1, file_size, file);
    if (read_bytes != file_size) {
        printf("Error: Failed to read full file content (%zu/%u bytes read)\n", read_bytes, file_size);
        fclose(file);
        free(all_data);
        free_bmp(bmp);
        return;
    }

    // 5. Write bits to image
    size_t bit_index = 0;
    for (size_t i = 0; i < total_bytes; ++i) {
        for (int b = 0; b < 8; ++b) {
            if (bit_index >= bmp->header.image_size) {
                printf("ERROR: bit_index %zu exceeds image size %u\n", bit_index, bmp->header.image_size);
                fclose(file);
                free(all_data);
                free_bmp(bmp);
                return;
            }
            fprintf(log, "writing bits...\n");
            bmp->pixel_data[bit_index] = (bmp->pixel_data[bit_index] & ~1) | ((all_data[i] >> b) & 1);
            bit_index++;
        }
    }
    fprintf(log, "done writing, saving image\n");
    fclose(log);
    fclose(file);
    free(all_data);

    save_bmp(output_path, bmp);
    free_bmp(bmp);
}

void extract_file(const char* stego_path, const char* output_file_path_hint) {
    BMPImage* bmp = load_bmp(stego_path);
    if (!bmp) {
        printf("Error: Could not load stego image.\n");
        return;
    }

    size_t bit_index = 0;

    // 1. Extract name_len
    uint32_t name_len = 0;
    for (int b = 0; b < 32; ++b)
        name_len |= (bmp->pixel_data[bit_index++] & 1) << b;

    // 2. Extract name
    char* name = (char*)malloc(name_len + 1);
    for (uint32_t i = 0; i < name_len; ++i) {
        uint8_t byte = 0;
        for (int b = 0; b < 8; ++b)
            byte |= (bmp->pixel_data[bit_index++] & 1) << b;
        name[i] = byte;
    }
    name[name_len] = '\0';

    if (name_len == 0 || name_len > 255) {
        printf("Error: Invalid file name length: %u\n", name_len);
        free_bmp(bmp);
        return;
    }

    // 3. Extract file_size
    uint32_t file_size = 0;
    for (int b = 0; b < 32; ++b)
        file_size |= (bmp->pixel_data[bit_index++] & 1) << b;

    if (bit_index + (uint64_t)file_size * 8 > bmp->header.image_size) {
        printf("ERROR: Extracted file size is too large for image.\n");
        free(name);
        free_bmp(bmp);
        return;
    }
    // 4. Create output file (use embedded name)
    char output_full_path[512];
    snprintf(output_full_path, sizeof(output_full_path), "%s\\%s", output_file_path_hint, name);
    FILE* out = fopen(output_full_path, "wb");
    if (!out) {
        printf("Error: Cannot create file: %s\n", name);
        free(name);
        free_bmp(bmp);
        return;
    }

    // 5. Extract data
    for (uint32_t i = 0; i < file_size; ++i) {
        uint8_t byte = 0;
        for (int b = 0; b < 8; ++b)
            byte |= (bmp->pixel_data[bit_index++] & 1) << b;
        fwrite(&byte, 1, 1, out);
    }

    fclose(out);
    free(name);
    free_bmp(bmp);
}
static char extracted_filename[256]; 

const char* get_embedded_filename(const char* stego_path) {
    BMPImage* bmp = load_bmp(stego_path);
    if (!bmp) return NULL;

    size_t bit_index = 0;
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

// Multi-Channel LSB: Spreads bits across R+G+B channels
void hide_message_multichannel(const char* image_path, const char* message, const char* output_path)
{
    BMPImage* bmp = load_bmp(image_path);
    if (!bmp) {
        printf("Error: Unable to load image.\n");
        return;
    }

    size_t msg_len = strlen(message) + 1; // Include null terminator
    size_t max_capacity = bmp->header.image_size / 3; // 3 channels = 3x capacity

    if (msg_len > max_capacity) {
        printf("Error: Message too long for multi-channel LSB.\n");
        free_bmp(bmp);
        return;
    }

    // Spread each bit across R, G, B channels
    for (size_t i = 0; i < msg_len; ++i) {
        for (int bit = 0; bit < 8; ++bit) {
            int pixel_index = i * 3 + (bit % 3); // Cycle through R,G,B
            bmp->pixel_data[pixel_index] = (bmp->pixel_data[pixel_index] & ~1) | ((message[i] >> bit) & 1);
        }
    }

    save_bmp(output_path, bmp);
    free_bmp(bmp);
}

void reveal_message_multichannel(const char* image_path, char* output, size_t max_len)
{
    BMPImage* bmp = load_bmp(image_path);
    if (!bmp) {
        printf("Error: Unable to load image.\n");
        return;
    }

    for (size_t i = 0; i < max_len; ++i) {
        output[i] = 0;
        for (int bit = 0; bit < 8; ++bit) {
            int pixel_index = i * 3 + (bit % 3); // Match the hiding pattern
            output[i] |= (bmp->pixel_data[pixel_index] & 1) << bit;
        }
        if (output[i] == '\0') break;
    }
    output[max_len - 1] = '\0';
    free_bmp(bmp);
}

