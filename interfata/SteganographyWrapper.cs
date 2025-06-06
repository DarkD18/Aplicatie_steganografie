﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace interfata
{
    internal static class SteganographyWrapper
    {
        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hideMessage(
            IntPtr pixelData,
            uint pixelDataSize,
            [MarshalAs(UnmanagedType.LPStr)] string message,
            IntPtr outputData
        );

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void revealMessage(
            IntPtr pixelData,
            uint pixelDataSize,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder output,
            uint maxLen
        );

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hideFile(
            IntPtr pixelData,
            uint pixelDataSize,
            [MarshalAs(UnmanagedType.LPStr)] string fileName,
            IntPtr fileData,
            uint fileSize,
            IntPtr outputData
        );

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void extractFile(
            IntPtr pixelData,
            uint pixelDataSize,
           [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder fileName,
            uint fileNameBufsize,
            IntPtr fileData,
            uint fileDataBufsize,
            ref uint fileSize
        );
        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hide_message_multichannel(
            IntPtr pixelData,
            uint pixelDataSize,
            [MarshalAs(UnmanagedType.LPStr)] string message,
            IntPtr outputData
        );

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void reveal_message_multichannel(
            IntPtr pixelData,
            uint pixelDataSize,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder output,
            uint maxLen
        );

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hide_file_multichannel(
            IntPtr pixel_data, uint pixel_data_size,
            [MarshalAs(UnmanagedType.LPStr)] string file_name,
            IntPtr file_data, uint file_size,
            IntPtr output_data
        );

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void extract_file_multichannel(
            IntPtr pixelData,
            uint pixelDataSize,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder fileName,
            uint fileNameBufsize,
            IntPtr fileData,
            uint fileDataBufsize,
            ref uint fileSize
        );
    }
}
