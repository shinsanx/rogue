using UnityEditor;

public class QuickBuild {
    [MenuItem("Build/Build and Run (macOS)")]
    public static void BuildMac() {
        BuildPipeline.BuildPlayer(
            new[] { "Assets/Scenes/SampleScene.unity" },
            "Builds/macOS/MyGame.app",
            BuildTarget.StandaloneOSX,
            BuildOptions.AutoRunPlayer
        );
    }
}