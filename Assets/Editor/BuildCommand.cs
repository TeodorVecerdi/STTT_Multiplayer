using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// 
/// Put me inside an Editor folder
/// 
/// Add a Build menu on the toolbar to automate multiple build for different platform
/// 
/// Use #define BUILD in your code if you have build specification 
/// Specify all your Target to build All
/// 
/// Install to Android device using adb install -r "pathofApk"
/// 
public class BuildCommand : MonoBehaviour {
    static BuildTarget[] targetToBuildAll = {
        BuildTarget.StandaloneWindows64
    };

    public static string ProductName => PlayerSettings.productName;

    private static string BuildPathRoot {
        get {
            string path = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 7), "Builds");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }

    static string LastBuildVersionCode {
        get => PlayerPrefs.GetString("LastVersionCode", "-1");
        set => PlayerPrefs.SetString("LastVersionCode", value);
    }

    static BuildTargetGroup ConvertBuildTarget(BuildTarget buildTarget) {
        switch (buildTarget) {
            case BuildTarget.StandaloneOSX:
            case BuildTarget.iOS:
                return BuildTargetGroup.iOS;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneLinux:
            case BuildTarget.StandaloneWindows64:
            case BuildTarget.StandaloneLinux64:
            case BuildTarget.StandaloneLinuxUniversal:
                return BuildTargetGroup.Standalone;
            case BuildTarget.Android:
                return BuildTargetGroup.Android;
            case BuildTarget.WebGL:
                return BuildTargetGroup.WebGL;
            case BuildTarget.WSAPlayer:
                return BuildTargetGroup.WSA;
            case BuildTarget.PS4:
                return BuildTargetGroup.PS4;
            case BuildTarget.XboxOne:
                return BuildTargetGroup.XboxOne;
            case BuildTarget.tvOS:
                return BuildTargetGroup.tvOS;
            case BuildTarget.Switch:
                return BuildTargetGroup.Switch;
            default:
                return BuildTargetGroup.Standalone;
        }
    }
    static string GetExtension(BuildTarget buildTarget) {
        switch (buildTarget) {
            case BuildTarget.StandaloneOSX:
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return ".exe";
            case BuildTarget.iOS:
                break;
            case BuildTarget.Android:
                return ".apk";
            case BuildTarget.StandaloneLinux:
                break;
            case BuildTarget.WebGL:
                break;
            case BuildTarget.WSAPlayer:
                break;
            case BuildTarget.StandaloneLinux64:
                break;
            case BuildTarget.StandaloneLinuxUniversal:
                break;
            case BuildTarget.PS4:
                break;
            case BuildTarget.XboxOne:
                break;
            case BuildTarget.tvOS:
                break;
            case BuildTarget.Switch:
                break;
            case BuildTarget.NoTarget:
                break;
        }

        return ".unknown";
    }
    static BuildPlayerOptions GetDefaultPlayerOptions() {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();

        List<string> listScenes = new List<string>();
        foreach (var s in EditorBuildSettings.scenes) {
            if (s.enabled)
                listScenes.Add(s.path);
        }

        buildPlayerOptions.scenes = listScenes.ToArray();
        buildPlayerOptions.options = BuildOptions.None;

        // To define
        // buildPlayerOptions.locationPathName = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\\LightGunBuild\\Android\\LightGunMouseArcadeRoom.apk";
        // buildPlayerOptions.target = BuildTarget.Android;

        return buildPlayerOptions;
    }

    static void DefaultBuild(BuildTarget buildTarget) {
        BuildTargetGroup targetGroup = ConvertBuildTarget(buildTarget);

        string path = Path.Combine(BuildPathRoot, ProductName + "_" + buildTarget + "@" + PlayerSettings.bundleVersion);
        string name = ProductName + GetExtension(buildTarget);

        string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols + ";BUILD");

        BuildPlayerOptions buildPlayerOptions = GetDefaultPlayerOptions();

        buildPlayerOptions.locationPathName = Path.Combine(path, name);
        buildPlayerOptions.target = buildTarget;

        EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, buildTarget);

        string result = buildPlayerOptions.locationPathName + ": " + BuildPipeline.BuildPlayer(buildPlayerOptions);
        Debug.Log(result);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols);

        LastBuildVersionCode = PlayerSettings.bundleVersion;
        ZipBuild(path, PlayerSettings.bundleVersion);
        EditorUtility.RevealInFinder(path);
    }

    [MenuItem("Build/1. Build Win64 %#&b", false, 0)]
    static void BuildWin64() {
        DefaultBuild(BuildTarget.StandaloneWindows64);
    }

    [MenuItem("Build/2. Build Win32", false, 0)]
    static void BuildWin32() {
        DefaultBuild(BuildTarget.StandaloneWindows);
    }

    [MenuItem("Build/3. Build All", false, 0)]
    static void BuildAll() {
        List<BuildTarget> buildTargetLeft = new List<BuildTarget>(targetToBuildAll);

        if (buildTargetLeft.Contains(EditorUserBuildSettings.activeBuildTarget)) {
            DefaultBuild(EditorUserBuildSettings.activeBuildTarget);
            buildTargetLeft.Remove(EditorUserBuildSettings.activeBuildTarget);
        }

        foreach (var b in buildTargetLeft) {
            DefaultBuild(b);
        }
    }

    [MenuItem("Build/4. Get Version", false, 20)]
    static void BuildNumber() {
        Debug.Log("Current/Last: " + PlayerSettings.bundleVersion + "/" + LastBuildVersionCode);
    }

    [MenuItem("Build/5.1. Increase Major Version", false, 20)]
    static void MajorBuildNumberUp() {
        var s = PlayerSettings.bundleVersion.Split('.');
        var s2 = $"{int.Parse(s[0]) + 1}.{s[1]}";
        PlayerSettings.bundleVersion = s2;
        BuildNumber();
    }
    

    [MenuItem("Build/5.2. Decrease Major Version", false, 20)]
    static void MajorBuildNumberDown() {
        var s = PlayerSettings.bundleVersion.Split('.');
        var s2 = $"{int.Parse(s[0]) - 1}.{s[1]}";
        PlayerSettings.bundleVersion = s2;
        BuildNumber();
    }

    [MenuItem("Build/6.1. Increase Minor Version", false, 20)]
    static void MinorBuildNumberUp() {
        var s = PlayerSettings.bundleVersion.Split('.');
        var s2 = $"{s[0]}.{int.Parse(s[1]) + 1}";
        PlayerSettings.bundleVersion = s2;
        BuildNumber();
    }

    [MenuItem("Build/6.2. Decrease Minor Version", false, 20)]
    static void MinorBuildNumberDown() {
        var s = PlayerSettings.bundleVersion.Split('.');
        var s2 = $"{s[0]}.{int.Parse(s[1]) - 1}";
        PlayerSettings.bundleVersion = s2;
        BuildNumber();
    }
    
    static void ZipBuild(string path, string version) {
        var t = new Thread(delegate () {
            Process p = new Process();
            p.StartInfo.FileName = @"B:/Program Files/7-Zip/7z.exe";
            p.StartInfo.Arguments = $@"a {path}.zip {path}\* & pause";
            Debug.Log(p.StartInfo.Arguments);
            p.StartInfo.CreateNoWindow = true;
//            p.StartInfo.UseShellExecute = false;
            p.Start();
            p.WaitForExit();
            p.StartInfo.FileName = @"C:/Users/Teodor Vecerdi/scoop/shims/hub.exe";
            p.StartInfo.Arguments = $"release create -a {path}.zip -m \"Release {version}\" {version}";
            p.Start();
        });
        t.Start();
    }

    [MenuItem("Build/7. Test Zip")]
    static void Test() {
        string path = Path.Combine(BuildPathRoot, ProductName + "_" + BuildTarget.StandaloneWindows64 + "@" + PlayerSettings.bundleVersion);
        string version = PlayerSettings.bundleVersion;
        var t = new Thread(delegate () {
            Process p = new Process();
            p.StartInfo.FileName = @"B:/Program Files/7-Zip/7z.exe";
            p.StartInfo.Arguments = $@"a {path}.zip {path}\* & pause";
            Debug.Log(p.StartInfo.Arguments);
            p.StartInfo.CreateNoWindow = true;
//            p.StartInfo.UseShellExecute = false;
            p.Start();
            p.WaitForExit();
            p.StartInfo.FileName = @"C:/Users/Teodor Vecerdi/scoop/shims/hub.exe";
            p.StartInfo.Arguments = $"release create -a {path}.zip -m \"Release {version}\" {version}";
            p.Start();
        });
        t.Start();
    } 
    [MenuItem("Build/8. Test Release")]
    static void TestRelease() {
        string path = Path.Combine(BuildPathRoot, ProductName + "_" + BuildTarget.StandaloneWindows64 + "@" + PlayerSettings.bundleVersion);
        string version = PlayerSettings.bundleVersion;
        var t = new Thread(delegate () {
            Process p = new Process();
            p.StartInfo.FileName = @"C:/Users/Teodor Vecerdi/scoop/shims/hub.exe";
            p.StartInfo.Arguments = $"release create -a {path}.zip -m \"Release {version}\" {version}";
            Debug.Log(p.StartInfo.Arguments);
//            p.StartInfo.CreateNoWindow = true;
//            p.StartInfo.UseShellExecute = false;
            p.Start();
        });
        t.Start();
    } 
}