
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;


public class HiScoreElement : UdonSharpBehaviour
{
    [SerializeField] public string playerName;
    [SerializeField] public int score;
    [SerializeField] TextMeshProUGUI textPlayerName;
    [SerializeField] TextMeshProUGUI textScore;
    void Start()
    {
        playerName = "playername";
        score = 0;
    }
}
