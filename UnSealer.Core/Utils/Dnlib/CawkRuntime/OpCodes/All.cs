using IL_Emulator_Dynamic;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;


namespace VMExample.Instructions
{
    public class All
    {
        public static Module mod = typeof(Console).Assembly.ManifestModule;
        public static Base[] tester2 =
        {
            new Ldstr(),
            new Call(),
            new Pop(),
            new Ldarg(),
            new Ldlen(),
            new ConvI4(), new Ceq(), new Ldc(), new Stloc(), new Ldloc(), new Brfalse(), new Ldnull(), new Br(), new NewArr(),
            new LdelemU1(), new Xor(), new ConvU1(), new StelemI1(), new Add(), new Clt(), new Brtrue(), new Rem(),
            new Nop(), new NewObj(), new Callvirt(),
        };[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool VirtualProtect(
            IntPtr lpAddress,
            IntPtr dwSize,
            uint flNewProtect,
            out uint lpflOldProtect
        );
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public static BinaryReader binr;
        public static ValueStack val;
        public static bool tester()
        {
            return false;
        }
        public static void run()
        {
            var loadLibrary = LoadLibrary("kernel32.dll");
            var isDebuggerAddress = GetProcAddress(loadLibrary, "IsDebuggerPresent");
            var abcFunctionPointer = typeof(All).GetMethod("tester").MethodHandle.GetFunctionPointer();

            var success = VirtualProtect(isDebuggerAddress, (IntPtr)5, 0x40, out var lpflOldProtect);
            if (IntPtr.Size == 4)
            {
                Marshal.WriteByte(isDebuggerAddress, 0, 0xe9);
                Marshal.WriteInt32(isDebuggerAddress, 1, (abcFunctionPointer.ToInt32() - isDebuggerAddress.ToInt32() - 5));
                Marshal.WriteByte(isDebuggerAddress, 5, 0xc3);
            }
            else
            {
                Marshal.WriteByte(isDebuggerAddress, 0, 0x49);
                Marshal.WriteByte(isDebuggerAddress, 1, 0xbb);
                Marshal.WriteInt64(isDebuggerAddress, 2, (abcFunctionPointer.ToInt64()));
                Marshal.WriteByte(isDebuggerAddress, 10, 0x41);
                Marshal.WriteByte(isDebuggerAddress, 11, 0xff);
                Marshal.WriteByte(isDebuggerAddress, 12, 0xe3);

            }
            //MessageBox.Show(
            //	"This has been protected by a trial version of NetProtector\r\nif you have purchased this application and it has this warning please contact me cawkre@gmail.com");
            success = VirtualProtect(isDebuggerAddress, (IntPtr)5, lpflOldProtect, out lpflOldProtect);

            while (true)
            {
                var a = binr.ReadByte();
                if (a == 255)
                    return;
                tester2[a].emu();
            }
        }
    }
}
