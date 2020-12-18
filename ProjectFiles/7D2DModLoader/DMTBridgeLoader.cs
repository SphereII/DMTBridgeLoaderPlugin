using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using DMT;
using UnityEngine;

namespace DMTBridgeLoader
{

    [BepInPlugin("org.dmtbridgeloader.plugin", "DMT Bridge", "1.0.0.0")]
    public class DMTBridgeLoaderPlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            // Fix up the output log's double spaces
            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);

            Debug.Log("Initializing DMT Bridge Plugin");
            HookHarmony();
        }
        public void HookHarmony()
        {
            var assemblies = new List<Assembly>();
            var modPath = Application.platform != RuntimePlatform.OSXPlayer ? (Application.dataPath + "/../Mods") : (Application.dataPath + "/../../Mods");

            if (Directory.Exists(modPath))
            {
                Debug.Log("Start harmony loading: " + modPath);
                string[] directories = Directory.GetDirectories(modPath);

                foreach (string path in directories)
                {
                    try
                    {

                        var modinfoPath = path + "/ModInfo.xml";

                        if (File.Exists(modinfoPath))
                        {
                            var files = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);
                            foreach (var file in files)
                            {
                                Debug.Log("DLL found: " + file);
                                var assembly = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(file));
                                var instances = from t in assembly.GetTypes()
                                                where t.GetInterfaces().Contains(typeof(IHarmony))
                                                         && t.GetConstructor(Type.EmptyTypes) != null
                                                select Activator.CreateInstance(t) as IHarmony;

                                foreach (var instance in instances)
                                    instance.Start();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Failed loading modded DLL from " + Path.GetFileName(path));
                        Debug.Log("\t" + e.ToString());
                    }
                }

            }
        }

    }
}
