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
        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hideMessage(
            [MarshalAs(UnmanagedType.LPStr)] string imagePath,
            [MarshalAs(UnmanagedType.LPStr)] string message,
            [MarshalAs(UnmanagedType.LPStr)] string outputPath);

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void revealMessage(
            [MarshalAs(UnmanagedType.LPStr)] string imagePath,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder output,
            int maxLen);
        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hide_file(
            [MarshalAs(UnmanagedType.LPStr)] string coverImagePath,
            [MarshalAs(UnmanagedType.LPStr)] string fileToHidePath,
            [MarshalAs(UnmanagedType.LPStr)] string outputImagePath);

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void extract_file(
            [MarshalAs(UnmanagedType.LPStr)] string stegoImagePath,
            [MarshalAs(UnmanagedType.LPStr)] string outputFilePath);

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr get_embedded_filename(string stegoImagePath);

        public static string GetEmbeddedFileName(string stegoImagePath)
        {
            IntPtr ptr = get_embedded_filename(stegoImagePath);
            return Marshal.PtrToStringAnsi(ptr);
        }
        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void hide_message_multichannel(
            [MarshalAs(UnmanagedType.LPStr)] string imagePath,
            [MarshalAs(UnmanagedType.LPStr)] string message,
            [MarshalAs(UnmanagedType.LPStr)] string outputPath);

        [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void reveal_message_multichannel(
            [MarshalAs(UnmanagedType.LPStr)] string imagePath,
            [MarshalAs(UnmanagedType.LPStr)] StringBuilder output,
            int maxLen);
    }
}
