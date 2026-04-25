using System;
using System.Runtime.InteropServices;

namespace LidController
{
    public static class PowerInterop
    {
        public static readonly Guid GUID_BUTTON_SUBGROUP = new Guid("4f971e89-eebd-4455-a8de-9e59040e7347");
        public static readonly Guid GUID_LIDCLOSE_ACTION = new Guid("5ca83367-6e45-459f-a27b-476b1d01c936");

        [DllImport("powrprof.dll")]
        public static extern uint PowerGetActiveScheme(
            IntPtr UserRootPowerKey,
            out IntPtr ActivePolicyGuid);

        [DllImport("powrprof.dll")]
        public static extern uint PowerSetActiveScheme(
            IntPtr UserRootPowerKey,
            ref Guid SchemeGuid);

        [DllImport("powrprof.dll")]
        public static extern uint PowerReadACValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            out uint AcValueIndex);

        [DllImport("powrprof.dll")]
        public static extern uint PowerReadDCValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            out uint DcValueIndex);

        [DllImport("powrprof.dll")]
        public static extern uint PowerWriteACValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            uint AcValueIndex);

        [DllImport("powrprof.dll")]
        public static extern uint PowerWriteDCValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            uint DcValueIndex);

        [DllImport("kernel32.dll")]
        public static extern IntPtr LocalFree(IntPtr hMem);

        public static Guid? GetActiveScheme()
        {
            IntPtr activePolicyPtr;
            uint res = PowerGetActiveScheme(IntPtr.Zero, out activePolicyPtr);
            if (res == 0)
            {
                Guid activePolicy = (Guid)Marshal.PtrToStructure(activePolicyPtr, typeof(Guid));
                LocalFree(activePolicyPtr); // Must free the memory allocated by PowerGetActiveScheme
                return activePolicy;
            }
            return null;
        }

        public static bool ReadLidCloseAction(Guid scheme, out uint acValue, out uint dcValue)
        {
            acValue = 1; // Default to Sleep (1)
            dcValue = 1;
            
            Guid subGroup = GUID_BUTTON_SUBGROUP;
            Guid setting = GUID_LIDCLOSE_ACTION;
            
            uint res1 = PowerReadACValueIndex(IntPtr.Zero, ref scheme, ref subGroup, ref setting, out acValue);
            uint res2 = PowerReadDCValueIndex(IntPtr.Zero, ref scheme, ref subGroup, ref setting, out dcValue);
            
            return res1 == 0 && res2 == 0;
        }

        public static bool WriteLidCloseAction(Guid scheme, uint acValue, uint dcValue)
        {
            Guid subGroup = GUID_BUTTON_SUBGROUP;
            Guid setting = GUID_LIDCLOSE_ACTION;

            uint res1 = PowerWriteACValueIndex(IntPtr.Zero, ref scheme, ref subGroup, ref setting, acValue);
            uint res2 = PowerWriteDCValueIndex(IntPtr.Zero, ref scheme, ref subGroup, ref setting, dcValue);

            if (res1 == 0 && res2 == 0)
            {
                // Apply the changes
                uint res3 = PowerSetActiveScheme(IntPtr.Zero, ref scheme);
                return res3 == 0;
            }
            return false;
        }
    }
}
