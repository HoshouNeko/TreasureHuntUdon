
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;
using System;
using UnityEngine.Serialization;






public class PersistancePrize : UdonSharpBehaviour
{


    [Tooltip("List of treasure names to check in persistance data")]
    [SerializeField] private string[] treasureNames;
    [Tooltip("List of points every treasure costs. Make sure to match position with Treasures names list")]
    [SerializeField] private int[] treasurePoints;

    [Tooltip("How much total points to unlock the prize")]
    [SerializeField] private int prizeCost;


    private int totalScore;
    private bool isPersistanceDataLoaded;


    
    [UdonSynced, FieldChangeCallback(nameof(isElegibleForPrize))]
    private bool _isElegibleForPrize;


    public bool isElegibleForPrize
    {
        set
        {
            Debug.Log($"isElegibleForPrize changed, setting {value}");
            prizeContainer.SetActive(value);
            _isElegibleForPrize = value;
        }
        get => _isElegibleForPrize;
    }


    [Header("Utilities")]
    [Tooltip("Object that will be enabled if elegible for win")]
    [SerializeField] GameObject prizeContainer;
    [Tooltip("Audio that plays if player won")]
    [SerializeField] AudioSource winAudioSource;




    private VRCPlayerApi _localPlayer;
    private VRCPlayerApi _owner;
    void Start()
    {
        _localPlayer = Networking.LocalPlayer;

    }

    public void calculateTotalScore()
    {
        totalScore = 0;
        for (int i = 0; i < treasureNames.Length; i++)
        {

            totalScore = totalScore + PlayerData.GetInt(_localPlayer, treasureNames[i]) * treasurePoints[i];
        }
        if (totalScore >= prizeCost)
        {

            SendCustomNetworkEvent(target:VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "WinSequence");
            isElegibleForPrize = true;
            RequestSerialization();
         //   prizeContainer.SetActive(true);
        }
    }

    public override void OnPlayerRestored(VRCPlayerApi player)
    {
        _owner = Networking.GetOwner(gameObject);
        isPersistanceDataLoaded = true;
        Debug.Log($"OnPlayerRestored isElegibleForPrize {isElegibleForPrize}");
        // if (_isElegibleForPrize) {prizeContainer.SetActive(true);}
    }

    public override void OnPlayerDataUpdated(VRCPlayerApi player, PlayerData.Info[] infos)
    {

        if (player.isLocal && !isElegibleForPrize)
        {
            calculateTotalScore();
            Debug.Log($"OnPlayerDataUpdated");
        }
    }

    public void WinSequence()
    {
        winAudioSource.transform.SetPositionAndRotation(_owner.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position, _owner.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation);
        winAudioSource.Play();
    }

}


