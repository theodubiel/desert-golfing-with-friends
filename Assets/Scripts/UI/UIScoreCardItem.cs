using UnityEngine;
using TMPro;

public class UIScoreCardItem : MonoBehaviour
{
    [SerializeField]
    private TMP_Text playerName;

    [SerializeField]
    private TMP_Text strokeCount;

    [SerializeField]
    private TMP_Text averageStrokes;

    [SerializeField]
    private TMP_Text holesCompleted;

    private PlayerData playerData;

    public void Bind(PlayerData playerData) {
        this.playerData = playerData;
        playerData.PlayerDataChange += OnPlayerDataUpdated;
        playerData.PlayerSettingsChange += OnPlayerSettingsUpdate;
        OnPlayerSettingsUpdate(playerData.settings.Value);
        OnPlayerDataUpdated(playerData.data.Value);
    }

    void OnDestroy() {
        playerData.PlayerDataChange -= OnPlayerDataUpdated;
        playerData.PlayerSettingsChange += OnPlayerSettingsUpdate;
    }

    void OnPlayerSettingsUpdate(PlayerSerializedSettings settings) {
        playerName.text = settings.playerName.ToString();
        playerName.color = settings.playerColor;
    }

    void OnPlayerDataUpdated(PlayerSerializedData data) {
        if (data.completeCurrentHole) {
            strokeCount.text = string.Format(
                "{0} ({1})",
                data.currentHoleStrokes.ToString(),
                data.totalStrokes.ToString()
            );
        } else {
            strokeCount.text = string.Format(
                "{0} ({1})",
                data.currentHoleStrokes.ToString(),
                (data.totalStrokes + data.currentHoleStrokes).ToString()
            );
        }
        averageStrokes.text = (((float)data.totalStrokes) / (Mathf.Max(data.holesMade, 1f))).ToString("0.00");
        holesCompleted.text = data.holesMade.ToString();
    }
}
