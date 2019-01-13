﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace QGameCenterLogic
{
    public class CheckDogUtils
    {
        #region 32位程序读写64注册表

        static UIntPtr HKEY_CLASSES_ROOT = (UIntPtr)0x80000000;
        static UIntPtr HKEY_CURRENT_USER = (UIntPtr)0x80000001;
        static UIntPtr HKEY_LOCAL_MACHINE = (UIntPtr)0x80000002;
        static UIntPtr HKEY_USERS = (UIntPtr)0x80000003;
        static UIntPtr HKEY_CURRENT_CONFIG = (UIntPtr)0x80000005;

        // 关闭64位（文件系统）的操作转向
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);
        // 开启64位（文件系统）的操作转向
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

        // 获取操作Key值句柄
        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint RegOpenKeyEx(UIntPtr hKey, string lpSubKey, uint ulOptions, int samDesired, out IntPtr phkResult);
        //关闭注册表转向（禁用特定项的注册表反射）
        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern long RegDisableReflectionKey(IntPtr hKey);
        //使能注册表转向（开启特定项的注册表反射）
        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern long RegEnableReflectionKey(IntPtr hKey);
        //获取Key值（即：Key值句柄所标志的Key对象的值）
        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int RegQueryValueEx(IntPtr hKey, string lpValueName, int lpReserved,
                                                  out uint lpType, System.Text.StringBuilder lpData,
                                                  ref uint lpcbData);
        #endregion
        internal static bool Verify()
        {
            var key = Registry.LocalMachine;
            var software = key.OpenSubKey("software\\game");

            try
            {
                var value = software.GetValue("code") as string;
                if (VerifyString(MachineCode.GetMachineCode(), value))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                var result = Get64BitRegistryKey("HKEY_LOCAL_MACHINE", "SOFTWARE\\game", "code");

                if (VerifyString(MachineCode.GetMachineCode(), result))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        
        private const string PublicKey = "<RSAKeyValue><Modulus>5Si72OdyRLzolJzWRg0vLhugF20UJrTRot35600vaUHV/SKy4n63G1H2rvOCNXfKTF776/EgMu+2xK/NyFJkEWPWj5UutqCDZjiLcqq66i1Nb3/W+jjso+1erbPHWimEEA8CKgzv4TTLNiSIsjxrlt+A75PCkqqFozxflE+Xplk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        private static bool VerifyString(string orgion, string content)
        {
            if (String.IsNullOrEmpty(orgion) || String.IsNullOrEmpty(content))
            {
                return false;
            }

            var oRSA4 = new RSACryptoServiceProvider();
            oRSA4.FromXmlString(PublicKey);
            var AOutputBuffer = Convert.FromBase64String(content);
            byte[] messagebytes = Encoding.UTF8.GetBytes(orgion);
            bool bVerify = oRSA4.VerifyData(messagebytes, "SHA1", AOutputBuffer);
            return bVerify;
        }

        private static string Get64BitRegistryKey(string parentKeyName, string subKeyName, string keyName)
        {
            int KEY_QUERY_VALUE = (0x0001);
            int KEY_WOW64_64KEY = (0x0100);
            int KEY_ALL_WOW64 = (KEY_QUERY_VALUE | KEY_WOW64_64KEY);

            try
            {
                //将Windows注册表主键名转化成为不带正负号的整形句柄（与平台是32或者64位有关）
                UIntPtr hKey = TransferKeyName(parentKeyName);

                //声明将要获取Key值的句柄
                IntPtr pHKey = IntPtr.Zero;

                //记录读取到的Key值
                StringBuilder result = new StringBuilder("".PadLeft(1024));
                uint resultSize = 1024;
                uint lpType = 0;

                //关闭文件系统转向 
                IntPtr oldWOW64State = new IntPtr();
                if (Wow64DisableWow64FsRedirection(ref oldWOW64State))
                {
                    //获得操作Key值的句柄
                    RegOpenKeyEx(hKey, subKeyName, 0, KEY_ALL_WOW64, out pHKey);

                    //关闭注册表转向（禁止特定项的注册表反射）
                    RegDisableReflectionKey(pHKey);

                    //获取访问的Key值
                    RegQueryValueEx(pHKey, keyName, 0, out lpType, result, ref resultSize);

                    //打开注册表转向（开启特定项的注册表反射）
                    RegEnableReflectionKey(pHKey);
                }

                //打开文件系统转向
                Wow64RevertWow64FsRedirection(oldWOW64State);

                //返回Key值
                return result.ToString().Trim();
            }
            catch
            {
                return null;
            }
        }

        private static UIntPtr TransferKeyName(string keyName)
        {
            switch (keyName)
            {
                case "HKEY_CLASSES_ROOT":
                    return HKEY_CLASSES_ROOT;
                case "HKEY_CURRENT_USER":
                    return HKEY_CURRENT_USER;
                case "HKEY_LOCAL_MACHINE":
                    return HKEY_LOCAL_MACHINE;
                case "HKEY_USERS":
                    return HKEY_USERS;
                case "HKEY_CURRENT_CONFIG":
                    return HKEY_CURRENT_CONFIG;
            }
            return HKEY_CLASSES_ROOT;
        }

    }
}
