using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using File = System.IO.File;

namespace GeekDesk.Util
{
    public class FileUtil
    {
        private static readonly string NO_PATH = "{(.*)}";
        private static readonly string NO_ICO = "^,(.*)";
        private static readonly string HAVE_ICO = "(.*),(.*)";

        #region Native ShellLink COM Interfaces
        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        [ClassInterface(ClassInterfaceType.None)]
        private class ShellLink { }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLinkW
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cch, IntPtr pfd, uint fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cch);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cch);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cch);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cch, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
            void Resolve(IntPtr hwnd, uint fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        private const uint SLGP_UNCPRIORITY = 0x2;
        private const uint SLGP_RAWPATH = 0x4;
        private const int STGM_READ = 0;
        #endregion

        public static string GetTargetPathByLnk(string filePath)
        {
            try
            {
                IShellLinkW link = (IShellLinkW)new ShellLink();
                ((IPersistFile)link).Load(filePath, STGM_READ);
                StringBuilder sb = new StringBuilder(1024);
                link.GetPath(sb, sb.Capacity, IntPtr.Zero, SLGP_UNCPRIORITY);
                string path = sb.ToString();

                if (string.IsNullOrEmpty(path))
                {
                    return ParseShortcut(filePath);
                }
                if (Regex.IsMatch(path, NO_PATH))
                {
                    string parsed = ParseShortcut(filePath);
                    if (!string.IsNullOrEmpty(parsed)) path = parsed;
                }
                return path;
            }
            catch (Exception e)
            {
                LogUtil.WriteErrorLog(e, "获取目标路径失败! filePath=" + filePath);
                return null;
            }
        }

        /// <summary>
        /// 获取启动参数
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetArgByLnk(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    IShellLinkW link = (IShellLinkW)new ShellLink();
                    ((IPersistFile)link).Load(filePath, STGM_READ);
                    StringBuilder sb = new StringBuilder(1024);
                    link.GetArguments(sb, sb.Capacity);
                    return sb.ToString();
                }
            }
            catch (Exception)
            {
            }
            return "";
        }

        /// <summary>
        /// 获取iconpath
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetIconPathByLnk(string filePath)
        {
            try
            {
                IShellLinkW link = (IShellLinkW)new ShellLink();
                ((IPersistFile)link).Load(filePath, STGM_READ);
                StringBuilder sb = new StringBuilder(1024);
                link.GetIconLocation(sb, sb.Capacity, out _);
                string iconPath = sb.ToString();

                if (StringUtil.IsEmpty(iconPath)
                    || Regex.IsMatch(iconPath, NO_ICO)
                    || Regex.IsMatch(iconPath, NO_PATH))
                {
                    return null;
                }
                else
                {
                    return iconPath.Split(',')[0];
                }
            }
            catch (Exception e)
            {
                LogUtil.WriteErrorLog(e, "获取图标路径失败! filePath=" + filePath);
                return null;
            }
        }

        [DllImport("msi.dll", CharSet = CharSet.Auto)]
        static extern int MsiGetShortcutTarget(string targetFile, StringBuilder productCode, StringBuilder featureID, StringBuilder componentCode);

        public enum InstallState
        {
            NotUsed = -7,
            BadConfig = -6,
            Incomplete = -5,
            SourceAbsent = -4,
            MoreData = -3,
            InvalidArg = -2,
            Unknown = -1,
            Broken = 0,
            Advertised = 1,
            Removed = 1,
            Absent = 2,
            Local = 3,
            Source = 4,
            Default = 5
        }

        public const int MaxFeatureLength = 38;
        public const int MaxGuidLength = 38;
        public const int MaxPathLength = 1024;

        [DllImport("msi.dll", CharSet = CharSet.Auto)]
        static extern InstallState MsiGetComponentPath(string productCode, string componentCode, StringBuilder componentPath, ref int componentPathBufferSize);

        public static string ParseShortcut(string file)
        {
            StringBuilder product = new StringBuilder(MaxGuidLength + 1);
            StringBuilder feature = new StringBuilder(MaxFeatureLength + 1);
            StringBuilder component = new StringBuilder(MaxGuidLength + 1);

            MsiGetShortcutTarget(file, product, feature, component);

            int pathLength = MaxPathLength;
            StringBuilder path = new StringBuilder(pathLength);

            InstallState installState = MsiGetComponentPath(product.ToString(), component.ToString(), path, ref pathLength);
            if (installState == InstallState.Local)
            {
                return path.ToString();
            }
            else
            {
                return null;
            }
        }

        public static string MakeRelativePath(string fromPath, string toPath)
        {
            string relativePath = null;
            try
            {
                if (string.IsNullOrEmpty(toPath) || string.IsNullOrEmpty(fromPath)) return null;
                Uri file = new Uri(@toPath);
                // Must end in a slash to indicate folder
                Uri folder = new Uri(@fromPath);
                relativePath =
                Uri.UnescapeDataString(
                    folder.MakeRelativeUri(file)
                        .ToString()
                        .Replace('/', Path.DirectorySeparatorChar)
                    );
            }
            catch (Exception ex)
            {
                LogUtil.WriteErrorLog(ex, "建立相对路径出错:fromPath:" + fromPath + ",toPath:" + toPath);
            }
            return relativePath;
        }

        public static FileInfo GetFileByNameWithDir(string name, string dir)
        {
            DirectoryInfo d = new DirectoryInfo(dir);
            FileInfo[] files = d.GetFiles();
            foreach (FileInfo fi in files)
            {
                if (fi.Name.Equals(name))
                {
                    return fi;
                }
            }
            DirectoryInfo[] directs = d.GetDirectories();
            foreach (DirectoryInfo direct in directs)
            {
                return GetFileByNameWithDir(name, direct.FullName);
            }
            return null;
        }
    }
}
