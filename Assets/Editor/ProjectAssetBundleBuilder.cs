using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class ProjectAssetBundleBuilder
{
    private const string OutputDirectory="Assets/ArtRes/AB/PC";

    [MenuItem("AB包工具/构建全部热更资源")]
    public static void BuildWindowsPC()
    {
        AssetBundleBuild[] builds=
        {
            new AssetBundleBuild
            {
                assetBundleName="ui",
                assetNames=new[]
                {
                    "Assets/Prefabs/UI/StartPanel.prefab",
                    "Assets/Prefabs/UI/GamePanel.prefab",
                    "Assets/Prefabs/UI/EndPanel.prefab",
                    "Assets/Prefabs/UI/DialoguePanel.prefab",
                    "Assets/Prefabs/UI/SettingPanel.prefab",
                    "Assets/Prefabs/UI/Img_Hp.prefab",
                    "Assets/Prefabs/UI/Btn_Choice.prefab",
                }
            },
            new AssetBundleBuild
            {
                assetBundleName="player",
                assetNames=new[]
                {
                    "Assets/Prefabs/Player/Bullet.prefab",
                }
            },
            new AssetBundleBuild
            {
                assetBundleName="music",
                assetNames=new[]
                {
                    "Assets/Audio/Music/BackgroundMusic.mp3",
                    "Assets/Audio/Music/Floating Cities.mp3",
                }
            },
            new AssetBundleBuild
            {
                assetBundleName="sound",
                assetNames=new[]
                {
                    "Assets/Audio/Sound/dialogue_typewriter_loop.wav",
                    "Assets/Audio/Sound/dialogue_typewriter.wav",
                    "Assets/Audio/Sound/doorOpen_001.ogg",
                    "Assets/Audio/Sound/explosionCrunch_000.ogg",
                    "Assets/Audio/Sound/forceField_001.ogg",
                    "Assets/Audio/Sound/forceField_003.ogg",
                    "Assets/Audio/Sound/impactMetal_000.ogg",
                    "Assets/Audio/Sound/impactMetal_001.ogg",
                    "Assets/Audio/Sound/impactMetal_003.ogg",
                    "Assets/Audio/Sound/impactMetal_004.ogg",
                    "Assets/Audio/Sound/laserSmall_000.ogg",
                    "Assets/Audio/Sound/laserSmall_003.ogg",
                    "Assets/Audio/Sound/lowFrequency_explosion_000.ogg",
                }
            },

            new AssetBundleBuild
            {
            assetBundleName="hot",
                assetNames=new[]
                {
                    "Assets/Prefabs/Circle.prefab"
                }
            },
        };

        foreach(AssetBundleBuild build in builds)
        {
            foreach(string assetPath in build.assetNames)
            {
                if(AssetDatabase.LoadMainAssetAtPath(assetPath)==null)
                    throw new BuildFailedException("打包资源不存在: "+assetPath);
            }
        }

        Directory.CreateDirectory(OutputDirectory);

        AssetBundleManifest manifest=BuildPipeline.BuildAssetBundles(
            OutputDirectory,
            builds,
            BuildAssetBundleOptions.ChunkBasedCompression|
            BuildAssetBundleOptions.ForceRebuildAssetBundle,
            BuildTarget.StandaloneWindows64);

        if(manifest==null)
            throw new BuildFailedException("AssetBundle构建失败");

        string[] expectedBundles={"PC","ui","player","music","sound"};
        foreach(string bundleName in expectedBundles)
        {
            string bundlePath=Path.Combine(OutputDirectory,bundleName);
            if(!File.Exists(bundlePath))
                throw new BuildFailedException("构建结果缺少: "+bundlePath);
        }

        CreateABCompare.CreateABCompareFile("PC");
        AssetDatabase.Refresh();
        Debug.Log("全部热更资源构建完成: "+Path.GetFullPath(OutputDirectory));
    }
}
