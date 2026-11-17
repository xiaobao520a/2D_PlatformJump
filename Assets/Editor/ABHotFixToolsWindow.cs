using UnityEditor;
using UnityEngine;

/// <summary>
/// AB热更工具窗口：选择平台后生成服务器清单或客户端默认资源
/// </summary>
public class ABHotFixToolsWindow : EditorWindow
{
    private enum E_ABPlatform
    {
        PC,
        Android,
        IOS
    }

    private E_ABPlatform platform = E_ABPlatform.PC;

    [MenuItem("AB包工具/热更工具")]
    public static void Open()
    {
        ABHotFixToolsWindow window = GetWindow<ABHotFixToolsWindow>("AB热更工具");
        window.minSize = new Vector2(420, 210);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(6);

        platform = (E_ABPlatform)EditorGUILayout.EnumPopup("目标平台", platform);

        string abRoot = Application.dataPath + "/ArtRes/AB/" + platform;
        string streamingRoot = "Assets/StreamingAssets/AB/" + platform;

        EditorGUILayout.Space(4);

        EditorGUILayout.HelpBox(
            "打包输出：\n" + abRoot + "\n\n" +
            "安装包目录：\n" + streamingRoot,
            MessageType.Info);

        EditorGUILayout.Space(6);

        if (GUILayout.Button("1. 创建资源对比文件", GUILayout.Height(28)))
        {
            CreateABCompare.CreateABCompareFile(platform.ToString());
        }

        if (GUILayout.Button("2. 生成默认资源（先在Project里选中要随包发布的包）", GUILayout.Height(28)))
        {
            GenerateDefaultAB.GenerateDefaultABToStreamingAssets(platform.ToString());
        }

        if (GUILayout.Button("3. 复制AB包和比较文件到远端服务器", GUILayout.Height(28)))
        {
            CopyABToRemoteServer.Copy(platform.ToString());
        }

        EditorGUILayout.Space(4);

        EditorGUILayout.HelpBox(
            "第一步生成服务器清单；第二步生成安装包默认资源；第三步选择服务器根目录并自动复制到AB/平台目录。",
            MessageType.None);
    }
}
