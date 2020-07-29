using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;
using System;

public class AutoIncrementBuildVersion : MonoBehaviour
{

	/// <summary>
	/// build后处理
	/// </summary>
	[PostProcessBuildAttribute(1)]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
	{
		IncrementBuildVersion();
	}

	/// <summary>
	/// build前处理
	/// </summary>
	[PostProcessSceneAttribute(2)]
	public static void OnPostProcessScene2()
	{
		IncrementBuildVersion();	//changed in build but not save
	}

	static void IncrementBuildVersion()
	{
		string currentVersion = PlayerSettings.bundleVersion;

		try
		{
			int major = Convert.ToInt32(currentVersion.Split('.')[0]);
			int minor = Convert.ToInt32(currentVersion.Split('.')[1]);
			int build = Convert.ToInt32(currentVersion.Split('.')[2]) + 1;

			PlayerSettings.bundleVersion = major + "." + minor + "." + build;
			PlayerSettings.Android.bundleVersionCode += 1;
			PlayerSettings.iOS.buildNumber = build.ToString();
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogError(e);
			UnityEngine.Debug.LogError("AutoIncrementBuildVersion script failed. Make sure your current bundle version is in the format X.X.X (e.g. 1.0.0) and not X.X (1.0) or X (1).");
		}
	}

}
