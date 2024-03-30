using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EuroTruckTrainer
{
    internal class EuroTrainer
    {
        private const int PROCESS_ALL_ACCESS = 0x1F0FFF;

        private IntPtr moneyPointer;

        private Process Process;
        private IntPtr processHandle;

        public uint Money { get; private set; } = 0;

        public EuroTrainer()
        {
            SetProcess();
            SetMoneyAdressPointer();
            GetMoney();
        }

        private void SetProcess()
        {
            Process = Process.GetProcessesByName("eurotrucks2")[0];
            processHandle = Kernel32.OpenProcess(PROCESS_ALL_ACCESS, false, Process.Id);
        }

        private void SetMoneyAdressPointer()
        {
            IntPtr moduleBaseAddress = Process!.MainModule!.BaseAddress; // The module base address is the starting point for the pointer chain.

            IntPtr pointer = IntPtr.Add(moduleBaseAddress, 0x01D079E8); // BASE_OFFSET

            int[] offsets = [0x10, 0]; // Offsets

            // Reading the pointer chain.
            for (int i = 0; i < offsets.Length; i++)
            {
                byte[] buffer = new byte[8]; // Use 8 bytes for 64-bit addresses.
                if (!Kernel32.ReadProcessMemory(processHandle, pointer, buffer, buffer.Length, out _))
                {
                    Console.WriteLine($"Failed to read from address: {pointer.ToString("X")}");
                    return;
                }

                pointer = (IntPtr)(long)BitConverter.ToUInt64(buffer, 0);
                pointer = IntPtr.Add(pointer, offsets[i]);
            }

            moneyPointer = IntPtr.Add(pointer, 0x10); //final Offset
        }

        public bool GetMoney()
        {
            // Assuming the value at the final pointer is a 4-byte integer.
            byte[] valueBuffer = new byte[4];
            if (!Kernel32.ReadProcessMemory(processHandle, moneyPointer, valueBuffer, valueBuffer.Length, out _))
            {
                Console.WriteLine($"Failed to read from final address: {moneyPointer.ToString("X")}");
                return false;
            }

            Money = BitConverter.ToUInt32(valueBuffer, 0);
            Console.WriteLine($"Current value: {Money}");
            return true;
        }

        public bool SetMoney(uint newValue)
        {
            if(newValue > uint.MaxValue || newValue < uint.MinValue)
            {
                Console.WriteLine("Value out of border.");
                return false;
            }

            byte[] newValueBuffer = BitConverter.GetBytes(newValue);
            if (!Kernel32.WriteProcessMemory(processHandle, moneyPointer, newValueBuffer, newValueBuffer.Length, out _))
            {
                Console.WriteLine($"Failed to write to address: {moneyPointer.ToString("X")}");
                return false;
            }

            Console.WriteLine($"New value written: {Money}");
            return true;
        }

        public bool SetMinusOneForNegativeBalance() => SetMoney(uint.MaxValue);
    }

    internal static class Kernel32
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);
    }
}
