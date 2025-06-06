#include <stdio.h>
#include <stdlib.h>
#include "bmp_handler.h"
#include <windows.h>

BMPImage* load_bmp(const char* file_path) {
    FILE* file = fopen(file_path, "rb");
    if (!file) {
        MessageBoxA(NULL, "Error: Unable to open file.", "ERROR", MB_OK);
        return NULL;
    }

    // Read the BMP signature
    unsigned short signature;
    fread(&signature, sizeof(unsigned short), 1, file);

    // Check if the file is a valid BMP
    if (signature != 0x4D42) { // 'BM' in little-endian
        MessageBoxA(NULL, "Error: The file is not a valid BMP file.", "ERROR", MB_OK);
        fclose(file);
        return NULL;
    }

    // Allocate memory for the BMP structure
    BMPImage* bmp = (BMPImage*)malloc(sizeof(BMPImage));
    if (!bmp) {
        MessageBoxA(NULL, "Error: Memory allocation failed.", "ERROR", MB_OK);
        fclose(file);
        return NULL;
    }

    // Read the BMP header in one step
    if (fread(&bmp->header, sizeof(BMPHeader), 1, file) != 1) {
        MessageBoxA(NULL, "Error: Failed to read BMP header.", "ERROR", MB_OK);
        free(bmp);
        fclose(file);
        return NULL;
    }

    // Log the header fields for debugging
    char debug_msg[256];
    snprintf(debug_msg, sizeof(debug_msg),
        "Debug: file_size=%u, data_offset=%u, header_size=%u, width=%d, height=%d, bit_count=%u, compression=%u, image_size=%u",
        bmp->header.file_size, bmp->header.data_offset, bmp->header.header_size, bmp->header.width, bmp->header.height,
        bmp->header.bit_count, bmp->header.compression, bmp->header.image_size);
    MessageBoxA(NULL, debug_msg, "DEBUG", MB_OK);

    // Validate header fields
    if (bmp->header.width <= 0 || bmp->header.height == 0 || bmp->header.bit_count == 0) {
        MessageBoxA(NULL, "Error: Invalid BMP header fields.", "ERROR", MB_OK);
        free(bmp);
        fclose(file);
        return NULL;
    }

    // Get the file size
    fseek(file, 0, SEEK_END);
    long file_size = ftell(file);
    rewind(file);

    // Validate data_offset
    if (bmp->header.data_offset < sizeof(BMPHeader) || bmp->header.data_offset >= file_size) {
        snprintf(debug_msg, sizeof(debug_msg), "Error: Invalid data offset in BMP header. data_offset=%u, file_size=%ld",
            bmp->header.data_offset, file_size);
        MessageBoxA(NULL, debug_msg, "ERROR", MB_OK);
        free(bmp);
        fclose(file);
        return NULL;
    }

    // Handle image_size == 0 or invalid for uncompressed BMPs
    if (bmp->header.image_size == 0 || bmp->header.image_size > file_size) {
        size_t row_size = ((bmp->header.width * bmp->header.bit_count + 31) / 32) * 4; // Row size with padding
        bmp->header.image_size = row_size * abs(bmp->header.height); // Total pixel data size

        // Validate calculated image_size
        if (bmp->header.data_offset + bmp->header.image_size > file_size) {
            snprintf(debug_msg, sizeof(debug_msg), "Error: Calculated image size is invalid. image_size=%u, file_size=%ld",
                bmp->header.image_size, file_size);
            MessageBoxA(NULL, debug_msg, "ERROR", MB_OK);
            free(bmp);
            fclose(file);
            return NULL;
        }
    }

    // Allocate memory for pixel data
    bmp->pixel_data = (unsigned char*)malloc(bmp->header.image_size);
    if (!bmp->pixel_data) {
        MessageBoxA(NULL, "Error: Memory allocation for pixel data failed.", "ERROR", MB_OK);
        free(bmp);
        fclose(file);
        return NULL;
    }

    // Move file pointer to the pixel data and read it
    fseek(file, bmp->header.data_offset, SEEK_SET);
    size_t bytes_read = fread(bmp->pixel_data, 1, bmp->header.image_size, file);
    if (bytes_read != bmp->header.image_size) {
        snprintf(debug_msg, sizeof(debug_msg), "Error: Failed to read pixel data. Expected: %u, Read: %zu",
            bmp->header.image_size, bytes_read);
        MessageBoxA(NULL, debug_msg, "ERROR", MB_OK);
        free(bmp->pixel_data);
        free(bmp);
        fclose(file);
        return NULL;
    }

    fclose(file);
    return bmp;
}

int save_bmp(const char* output_path, BMPImage* bmp) {
    FILE* file = fopen(output_path, "wb");
    if (!file) {
        char debug_msg[256];
        snprintf(debug_msg, sizeof(debug_msg), "Error: Unable to open file for writing: %s", output_path);
        MessageBoxA(NULL, debug_msg, "ERROR", MB_OK);
        return 0; // Failure
    }

    // Write BMP header
    if (fwrite(&bmp->header, sizeof(BMPHeader), 1, file) != 1) {
        MessageBoxA(NULL, "Error: Failed to write BMP header.", "ERROR", MB_OK);
        fclose(file);
        return 0; // Failure
    }

    // Write pixel data
    if (fwrite(bmp->pixel_data, 1, bmp->header.image_size, file) != bmp->header.image_size) {
        MessageBoxA(NULL, "Error: Failed to write BMP pixel data.", "ERROR", MB_OK);
        fclose(file);
        return 0; // Failure
    }

    char debug_msg[256];
    snprintf(debug_msg, sizeof(debug_msg), "File saved successfully: %s", output_path);
    MessageBoxA(NULL, debug_msg, "INFO", MB_OK);

    fclose(file);
    return 1; // Success
}

// Function to free BMP resources
void free_bmp(BMPImage* bmp) {
    if (!bmp) return;

    if (bmp->pixel_data) {
        char debug_msg[256];
        //snprintf(debug_msg, sizeof(debug_msg), "Freeing pixel_data at address: %p", bmp->pixel_data);
        //MessageBoxA(NULL, debug_msg, "DEBUG", MB_OK);
        free(bmp->pixel_data);
        bmp->pixel_data = NULL; // Avoid double free
    }

    char debug_msg[256];
    //snprintf(debug_msg, sizeof(debug_msg), "Freeing BMP structure at address: %p", bmp);
    //MessageBoxA(NULL, debug_msg, "DEBUG", MB_OK);
    free(bmp);
}
