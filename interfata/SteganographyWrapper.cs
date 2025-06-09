using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace interfata
{
    internal static class SteganographyWrapper
    {
        /******************************** Standard LSB DLL includes ***************************************/
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

        /******************************** Multichannell LSB DLL includes ***************************************/
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
        /******************************** Standard LSB shuffle DLL includes ***************************************/

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern void hide_message_shuffle(
        IntPtr pixelData,
        uint pixelDataSize,
        [MarshalAs(UnmanagedType.LPStr)] string message,
        [MarshalAs(UnmanagedType.LPStr)] string password,
        IntPtr outputData
        );

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void reveal_message_shuffle(
        IntPtr pixelData,
        uint pixelDataSize,
        [MarshalAs(UnmanagedType.LPStr)] string password,
        [MarshalAs(UnmanagedType.LPStr)] StringBuilder soutput,
        uint maxLen
        );

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
         public static extern void hide_file_shuffle(
            IntPtr pixelData,
            uint pixelDataSize,
            [MarshalAs(UnmanagedType.LPStr)] string fileName,
            IntPtr fileData,
            uint fileSize,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            IntPtr outputData
        );
        
        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void extract_file_shuffle(
            IntPtr pixelData,
            uint pixelDataSize,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder fileName,
            uint fileNameBufsize,
            IntPtr fileData,
            uint fileDataBufsize,
            ref uint fileSize
        );

        /******************************** Multuchannel LSB shuffle DLL includes ***************************************/
        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hide_message_multichannel_shuffle(
           IntPtr pixelData,
           uint pixelDataSize,
           [MarshalAs(UnmanagedType.LPStr)] string message,
           [MarshalAs(UnmanagedType.LPStr)] string password,
           IntPtr outputData
       );

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void reveal_message_multichannel_shuffle(
            IntPtr pixelData,
            uint pixelDataSize,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder output,
            uint maxLen
        );

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hide_file_multichannel_shuffle(
            IntPtr pixel_data, uint pixel_data_size,
            [MarshalAs(UnmanagedType.LPStr)] string file_name,
            IntPtr file_data, uint file_size,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            IntPtr output_data
        );

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void extract_file_multichannel_shuffle(
            IntPtr pixelData,
            uint pixelDataSize,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder fileName,
            uint fileNameBufsize,
            IntPtr fileData,
            uint fileDataBufsize,
            ref uint fileSize
        );
        /******************************** WAV lsb  DLL includes ***************************************/
        const string Dll = "librarie_steganografie_c.dll";

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void hideMessageInWav(
            IntPtr wavData,
            uint wavDataSize,
            [MarshalAs(UnmanagedType.LPStr)] string message,
            IntPtr outputData
        );

        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void revealMessageFromWav(
            IntPtr wavData,
            uint wavDataSize,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder output,
            uint maxLen
        );

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hideFileInWav(
               IntPtr wavData,
               uint wavDataSize,
               [MarshalAs(UnmanagedType.LPStr)] string fileName,
               IntPtr fileData,
               uint fileSize,
               IntPtr outputData
           );

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void extractFileFromWav(
            IntPtr wavData,
            uint wavDataSize,
            [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder fileName,
            uint fileNameBufsize,
            IntPtr fileData,
            uint fileDataBufsize,
            out uint fileSize
        );

        /******************************** WAV lsb shuffle DLL includes ***************************************/
        [DllImport(Dll, EntryPoint = "hideMessageShuffleInWav", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void hideMessageShuffleInWav(
       IntPtr wavData, uint wavDataSize,
       [MarshalAs(UnmanagedType.LPStr)] string message,
       [MarshalAs(UnmanagedType.LPStr)] string password,
       IntPtr outputData
         );

        [DllImport(Dll, EntryPoint = "revealMessageShuffleFromWav", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void revealMessageShuffleFromWav(
            IntPtr wavData, uint wavDataSize,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder output ,
            uint maxLen
        );

        [DllImport(Dll, EntryPoint = "hideFileShuffleInWav", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void hideFileShuffleInWav(
            IntPtr wavData, uint wavDataSize,
            [MarshalAs(UnmanagedType.LPStr)] string fileName,
            IntPtr fileData, uint fileSize,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            IntPtr outputData
        );

        [DllImport(Dll, EntryPoint = "extractFileShuffleFromWav", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void extractFileShuffleFromWav(
            IntPtr wavData, uint wavDataSize,
            [MarshalAs(UnmanagedType.LPStr)] string password,
            StringBuilder fileName, uint fileNameBufsize,
            IntPtr fileData, uint fileDataBufsize,
            out uint fileSize
        );
    }
}
