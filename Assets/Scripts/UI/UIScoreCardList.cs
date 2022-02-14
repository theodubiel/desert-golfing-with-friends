using UnityEngine;

public class UIScoreCardList : MonoBehaviour
{
    [SerializeField]
    private GameObject PlayerScoreItemPrefab;

    void OnEnable() {
        PlayerData.PlayerCountChange += RefreshScoreCards;
        RefreshScoreCards();
    }

    void OnDisable() {
        PlayerData.PlayerCountChange -= RefreshScoreCards;
    }

    void RefreshScoreCards() {
        for(int i = gameObject.transform.childCount - 1; i >= 0; i--) {
            Destroy(transform.GetChild(i).gameObject);
        }
        var playerDatas = GameObject.FindObjectsOfType<PlayerData>();
        foreach(var playerData in playerDatas) {
            var scoreItemPrefab = Instantiate(PlayerScoreItemPrefab, transform);
            var scoreItemScript = scoreItemPrefab.GetComponent<UIScoreCardItem>();
            scoreItemScript.Bind(playerData);
        }
    }
}
