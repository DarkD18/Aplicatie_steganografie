#ifndef BMP_HANDLER_H
#define BMP_HANDLER_H

#include <stdint.h>

// BMP header structure
#pragma pack(push, 1) // Ensure no padding
typedef struct {
    uint16_t signature;       // File signature ('BM')
    uint32_t file_size;       // File size in bytes
    uint16_t reserved1;       // Reserved (0)
    uint16_t reserved2;       // Reserved (0)
    uint32_t data_offset;     // Offset to pixel data
    uint32_t header_size;     // Header size (40 bytes)
    int32_t width;            // Image width
    int32_t height;           // Image height
    uint16_t planes;          // Number of planes (1)
    uint16_t bit_count;       // Bits per pixel (e.g., 24 for RGB)
    uint32_t compression;     // Compression (0 for uncompressed)
    uint32_t image_size;      // Image size (may be 0 for uncompressed)
    int32_t x_pixels_per_meter; // Horizontal resolution
    int32_t y_pixels_per_meter; // Vertical resolution
    uint32_t colors_used;     // Number of colors used
    uint32_t colors_important;// Number of important colors
} BMPHeader;
#pragma pack(pop)

// BMP image structure
typedef struct {
    BMPHeader header;
    unsigned char* pixel_data;
} BMPImage;

// Function declarations
BMPImage* load_bmp(const char* file_path);
int save_bmp(const char* file_path, BMPImage* bmp);
void free_bmp(BMPImage* bmp);

#endif // BMP_HANDLER_H
