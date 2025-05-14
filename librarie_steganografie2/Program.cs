class Program
{
    [DllImport("librarie_steganografie_c.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void hideMessage(string imagePath, string message, string outputPath);

    static void Main(string[] args)
    {
        try
        {
            hideMessage("input.bmp", "hidden message", "output.bmp");
            Console.WriteLine("Function call succeeded.");
        }
        catch (DllNotFoundException ex)
        {
            Console.WriteLine($"DLL not found: {ex.Message}");
        }
        catch (EntryPointNotFoundException ex)
        {
            Console.WriteLine($"Function not found in DLL: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
