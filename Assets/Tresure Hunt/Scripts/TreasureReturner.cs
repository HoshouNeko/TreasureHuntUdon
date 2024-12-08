
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Persistence;




public class TreasureReturner : UdonSharpBehaviour
{
    private VRCPlayerApi _localPlayer;
    [SerializeField] public TreasureHuntController treasureHuntController;
    [SerializeField] public GameObject pickAnimation;
    void Start()
    {
        _localPlayer = Networking.LocalPlayer;

    }

    public override void Interact()
    {
        // Convert name of this to int and call method on main controller to return
        treasureHuntController.ReturnItem(int.Parse(gameObject.name));

        // Check current score, add 1 and save persistance
        string parentName = gameObject.transform.parent.name;
        var currentValue = PlayerData.GetInt(_localPlayer, parentName);
        PlayerData.SetInt(parentName, currentValue + 1);

        Debug.Log($"Tresure {parentName} found, updating score to {PlayerData.GetInt(_localPlayer, parentName)}");
    }

    public void OnDisable()
    {
        Instantiate(pickAnimation, gameObject.transform.position, gameObject.transform.rotation);
    }
}
