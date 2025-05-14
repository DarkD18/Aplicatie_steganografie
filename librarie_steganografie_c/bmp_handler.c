#include <stdio.h>
#include <stdlib.h>
#include "bmp_handler.h"
#include <windows.h>

// Function to load a BMP file
BMPImage* load_bmp(const char* file_path) {
    FILE* file = fopen(file_path, "rb");
    if (!file) {
        MessageBoxA(NULL, "Error: Unable to open file.", "ERROR", MB_OK);
        return NULL;
    }

    BMPImage* bmp = (BMPImage*)malloc(sizeof(BMPImage));
    if (!bmp) {
        MessageBoxA(NULL, "Error: Memory allocation failed.", "ERROR", MB_OK);
        fclose(file);
        return NULL;
    }

    // Read the BMP header
    fread(&bmp->header, sizeof(BMPHeader), 1, file);

    // Check if it's a valid BMP file
    if (bmp->header.signature != 0x4D42) {  // 'BM' in little-endian
        MessageBoxA(NULL, "Error: Invalid BMP file.", "ERROR", MB_OK);
        free(bmp);
        fclose(file);
        return NULL;
    }

    // Handle image_size == 0 for uncompressed BMPs
    if (bmp->header.image_size == 0) {
        size_t row_size = ((bmp->header.width * bmp->header.bit_count + 31) / 32) * 4; // Row size with padding
        bmp->header.image_size = row_size * abs(bmp->header.height); // Total pixel data size
        char debug_msg[256];
        //snprintf(debug_msg, sizeof(debug_msg), "Calculated image_size: %u", bmp->header.image_size);
        //MessageBoxA(NULL, debug_msg, "DEBUG", MB_OK);
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
    if (fread(bmp->pixel_data, 1, bmp->header.image_size, file) != bmp->header.image_size) {
        MessageBoxA(NULL, "Error: Failed to read pixel data or size mismatch.", "ERROR", MB_OK);
        free(bmp->pixel_data);
        free(bmp);
        fclose(file);
        return NULL;
    }

    //MessageBoxA(NULL, "BMP file loaded successfully.", "INFO", MB_OK);
    fclose(file);
    return bmp;
}

int save_bmp(const char* output_path, BMPImage* bmp) {
    FILE* file = fopen(output_path, "wb");
    if (!file) {
        MessageBoxA(NULL, "Error: Unable to open file for writing.", "ERROR", MB_OK);
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

    //MessageBoxA(NULL, "File saved successfully.", "INFO", MB_OK);
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
