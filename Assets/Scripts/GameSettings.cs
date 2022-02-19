using System;
using UnityEngine;

[Serializable]
public class GameSettingsData
{
    public static string FILENAME = "game-settings";
    public float swingSensitivity;
    public float masterVolume;
    public ulong startingLevel;
    public bool fullScreen;

    public static readonly GameSettingsData Default = new GameSettingsData() {
        masterVolume = GameSettings.MASTER_VOLUME_DEFAULT,
        swingSensitivity = GameSettings.SWING_SENSITIVITY_DEFAULT,
        startingLevel = GameSettings.STARTING_LEVEL_DEFAULT,
        fullScreen = GameSettings.FULLSCREEN_DEFAULT
    };
}

public class GameSettings
{
    // Master Volume
    public static float MAX_MASTER_VOLUME = 1.0f;
    public static float MIN_MASTER_VOLUME = 0.0f;
    public static float MASTER_VOLUME_DEFAULT = 0.5f;
    public static float MASTER_VOLUME = MASTER_VOLUME_DEFAULT;

    // Swing Sensitivity
    public static float MAX_SWING_SENSITIVITY = 1.5f;
    public static float MIN_SWING_SENSITIVITY = 0.5f;
    public static float SWING_SENSITIVITY_DEFAULT = 1.0f;
    public static float SWING_SENSITIVITY = SWING_SENSITIVITY_DEFAULT;


    // Starting Level
    public static ulong MAX_STARTING_LEVEL = 1000000;
    public static ulong MIN_STARTING_LEVEL = 1;
    public static ulong STARTING_LEVEL_DEFAULT = 1;
    public static ulong STARTING_LEVEL = STARTING_LEVEL_DEFAULT;

    // Starting Level
    public static bool FULLSCREEN_DEFAULT = true;
    public static bool FULLSCREEN = FULLSCREEN_DEFAULT;

    public static void Apply(GameSettingsData gameSettingsData) {
        MASTER_VOLUME = gameSettingsData.masterVolume;
        SWING_SENSITIVITY = gameSettingsData.swingSensitivity;
        STARTING_LEVEL = gameSettingsData.startingLevel;
        FULLSCREEN = gameSettingsData.fullScreen;
        SetFullScreen(FULLSCREEN);
    }

    public static void SetFullScreen(bool isFullScreen) {
        if (Screen.fullScreen != isFullScreen) {
            var mode = isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, mode);
        }
    }

    public static void ApplySavedSettings() {
        Apply(FileManager.LoadData<GameSettingsData>(GameSettingsData.FILENAME, GameSettingsData.Default));
    }
}
