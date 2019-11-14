using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using UnityEditor.Build.Reporting;


public class BuildToHololenAndStandAlone
{
    [MenuItem("HEVS/Build HEVSoLens Apps")]
    public static void BuildGame()
    {
        GameObject.FindObjectOfType<HEVS.Configuration>().debugImpersonateNode = false;

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
        // Standalone app
        string[] levels = new string[] { "Assets/UniSA/Examples/Scenes/Graph.unity" };
      
        BuildPlayerOptions buildPlayerOptionsStandalone = new BuildPlayerOptions();
        buildPlayerOptionsStandalone.scenes = levels;
        buildPlayerOptionsStandalone.locationPathName = "HEVS App/HEVS.exe";
        buildPlayerOptionsStandalone.target = BuildTarget.StandaloneWindows;
        buildPlayerOptionsStandalone.options = BuildOptions.AllowDebugging;

        BuildReport reportStandalone = BuildPipeline.BuildPlayer(buildPlayerOptionsStandalone);
        BuildSummary summaryStandalone = reportStandalone.summary;

        if(summaryStandalone.result == BuildResult.Succeeded)
        {
           UnityEngine.Debug.Log("Standalone Build succeeded: " + summaryStandalone.totalSize + " bytes");
        }
        else
        {
            UnityEngine.Debug.Log("Standalone Build failed");
        }

        // Copy a file from the project folder to the build folder, alongside the built game.
        //  FileUtil.CopyFileOrDirectory("Assets/Templates/Readme.txt", path + "Readme.txt");

        // Run the game (Process class from System.Diagnostics).
        //Process proc = new Process();
        //proc.StartInfo.FileName = path + "/HEVS.exe";
        //proc.Start();

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);

        // UWP App
        EditorUserBuildSettings.wsaSubtarget = WSASubtarget.HoloLens;
        EditorUserBuildSettings.wsaArchitecture = "x86";
        EditorUserBuildSettings.wsaUWPBuildType = WSAUWPBuildType.D3D;
        EditorUserBuildSettings.wsaBuildAndRunDeployTarget = WSABuildAndRunDeployTarget.LocalMachine;

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = levels;
        buildPlayerOptions.locationPathName = "HEVS HoloApp";
        buildPlayerOptions.target = BuildTarget.WSAPlayer;
        buildPlayerOptions.options = BuildOptions.AllowDebugging;

        BuildReport reportWSA = BuildPipeline.BuildPlayer(buildPlayerOptions);

        BuildSummary summaryWSA = reportWSA.summary;


        if (summaryWSA.result == BuildResult.Succeeded)
        {
            UnityEngine.Debug.Log("WSA Build succeeded: " + summaryWSA.totalSize + " bytes");
        }

        else
        {
            UnityEngine.Debug.Log("WSA Build failed");
        }
    }
    
}

