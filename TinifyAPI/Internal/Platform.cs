using System;
using System.Reflection;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

namespace TinifyAPI.Internal
{
    internal static class Platform
    {
        static private Assembly assembly = typeof(Platform).GetTypeInfo().Assembly;

        static public string ClientVersion
        {
            get
            {
                var name = assembly.GetName().Name;
                var version = assembly.GetName().Version;
                return name + "/" + version;
            }
        }

        static public string FrameworkVersion
        {
            get
            {
                /* Parsing based on:
                 * https://github.com/mono/mono/blob/master/mcs/class/referencesource/System/sys/system/runtime/versioning/FrameworkName.cs */
                var framework = assembly.GetCustomAttribute<TargetFrameworkAttribute>().FrameworkName;
                if (framework == null) return null;

                var components = framework.Split(',');

                var name = components[0].Trim();
                var version = "unknown";

                for (int i = 0; i < components.Length; i++)
                {
                    var pair = components[i].Split('=');
                    if (pair.Length != 2) continue;

                    if (pair[0].Trim().Equals("Version", StringComparison.OrdinalIgnoreCase))
                    {
                        version = pair[1].Trim().TrimStart('v');
                    }
                }

                var os = "Unknown";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    os = "Windows";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    os = "OSX";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    os = "Linux";
                }

                var details = RuntimeInformation.FrameworkDescription;

                return string.Format("{0}/{1} ({2} {3})", name, version, os, details);
            }
        }

        static public string UserAgent = (ClientVersion + " " + FrameworkVersion).Trim();
    }
}