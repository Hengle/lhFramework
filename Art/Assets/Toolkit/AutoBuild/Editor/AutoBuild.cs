using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using CodeStage.Maintainer;
using CodeStage.Maintainer.Issues;

public static class AutoBuild  {

    [MenuItem("Tools/AutoBuild/BuildSource")]
    public static void BuildSource(){

        var dic = GetCommandArgs();
        string mode = dic["mode"];
        string platform = dic["platform"];
        if (mode.ToLower()=="debug")
        {
            if (dic.ContainsKey("maintainer_filePath"))
            {
                var records = IssuesFinder.StartSearch(false);
                using (StreamWriter sr = File.CreateText(dic["maintainer_filePath"]))
                {
                    sr.Write(ReportsBuilder.GenerateReport(IssuesFinder.MODULE_NAME, records));
                }
            }
        }
        if (dic.ContainsKey("currentLevel"))
            QualitySettings.SetQualityLevel(System.Convert.ToInt32(dic["currentLevel"]));
        lhFramework.Tools.Bundle.BundleBuildManager bundleManager = new lhFramework.Tools.Bundle.BundleBuildManager();
        bundleManager.buildTarget = System.Convert.ToInt32(platform);
        bundleManager.BuildPackage();
        bundleManager = null;
    }
    private static Dictionary<string, string> GetCommandArgs()
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].Contains("="))
            {
                var split = args[i].Split('=');
                dic.Add(split[0], split[1]);
            }
        }
        return dic;
    }
}
