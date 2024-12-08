
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Data;
using VRC.SDK3.Persistence;
using TMPro;




public class Scoreboard : UdonSharpBehaviour
{
    DataList listOfTreasures = new DataList();
    DataDictionary dataBase = new DataDictionary();

    [SerializeField] private Transform treasuresPool;
    [SerializeField] private Transform listsRoot;

    [SerializeField] GameObject hiScoreListPrefab;
    [SerializeField] GameObject entryElementPrefab;

    VRCPlayerApi[] allPlayers = new VRCPlayerApi[1];
    void Start()
    {
        SendCustomEventDelayedSeconds("Initiate", 1);
    }


    public void Initiate()
    {
        InitiateItems();
        FillDataBase();
        PrintLists();
        SendCustomEventDelayedFrames("PrintElements", 1);
    }
    public void InitiateItems()
    {
        dataBase.Clear();
        listOfTreasures.Clear();

        //Build a list of unique treasures
        for (int i = 0; i < treasuresPool.childCount; i++)
        {
            DataToken t = treasuresPool.GetChild(i).name;
            if (!listOfTreasures.Contains(t))
            {
                listOfTreasures.Add(t);
            }
        }

        //Fill root database with list of treasures
        for (int i = 0; i < listOfTreasures.Count; i++)
        {
            DataToken t = new DataList();
            dataBase.SetValue(listOfTreasures[i], t);
            Debug.Log($"Creating datalist {i} ");
        }

    }

    public void FillDataBase()
    {
        DataList dbKeys = dataBase.GetKeys();
        for (int i = 0; i < listOfTreasures.Count; i++)
        {
            dataBase[dbKeys[i]].DataList.Clear();
        }

        //Get all players and populate database
        allPlayers = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
        VRCPlayerApi.GetPlayers(allPlayers);


        for (int i = 0; i < dbKeys.Count; i++)
        {
              //  DataDictionary itemDB = dataBase[dbKeys[i]].DataDictionary;
            for (int p = 0; p < allPlayers.Length; p++)
            {
                DataToken t = new DataDictionary();
                dataBase[dbKeys[i]].DataList.Add(t);
                dataBase[dbKeys[i]].DataList[p].DataDictionary.SetValue(allPlayers[p].displayName, PlayerData.GetInt(allPlayers[p], dbKeys[i].String));
            }
        }
        SortListOfDictionaries();
    }



    public void PrintLists()
    {
        //Generate score boards for each prop
        DataList dbKeys = dataBase.GetKeys();
        for (int i = 0; i < dbKeys.Count; i++)
        {           
            GameObject newInstance = Instantiate(hiScoreListPrefab, listsRoot, false);
            newInstance.GetComponentInChildren<TextMeshProUGUI>().text = dbKeys[i].ToString();
        }
        Debug.Log($"Creating ScoreBoards");
    }
    public void PrintElements()
    {
         DeleteElements();
        //Generate name elements of each score board
        DataList dbKeys = dataBase.GetKeys();
        for (int i = 0; i < listOfTreasures.Count; i++)
        {
            Transform currentHiScoreList = listsRoot.transform.GetChild(i);
            for (int p = 0; p < allPlayers.Length; p++)
            {
                Transform currentHiScoreListElementsRoot = currentHiScoreList.GetChild(1);
                GameObject newInstance = Instantiate(entryElementPrefab, currentHiScoreListElementsRoot, false);
                DataList nameKeys = dataBase[dbKeys[i]].DataList[p].DataDictionary.GetKeys();
                string name = nameKeys[0].ToString();
                DataList scoreKeys = dataBase[dbKeys[i]].DataList[p].DataDictionary.GetValues();
                string score = scoreKeys[0].ToString();
                TextMeshProUGUI[] textFields = newInstance.transform.GetComponentsInChildren<TextMeshProUGUI>();
                textFields[0].text = name;
                textFields[1].text = score;
            }
        }
    }





    public void ResetBoards()
    {
        DataList dbKeys = dataBase.GetKeys();
            foreach (Transform child in listsRoot) 
        {
            Destroy(child.gameObject);
        }
        PrintLists();
    }

    public void DeleteElements()
    {


        DataList dbKeys = dataBase.GetKeys();
        for (int i = 0; i < listOfTreasures.Count; i++)
        {

            Transform currentHiScoreList = listsRoot.transform.GetChild(i);
            Transform currentHiScoreListElementsRoot = currentHiScoreList.GetChild(1);
            foreach (Transform child in currentHiScoreListElementsRoot)
            {
                Destroy(child.gameObject);
            }
        }
    }


    public void PopulateListOfPlayers()
    {
        VRCPlayerApi.GetPlayers(allPlayers);
        foreach (VRCPlayerApi player in allPlayers)
        {
            if (player == null) continue;
        }
        Debug.Log($"Deleting all boards");
    }

    public override void OnPlayerDataUpdated(VRCPlayerApi player, PlayerData.Info[] infos)
    {
        FillDataBase();
        PrintElements();
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
            FillDataBase();
            PrintElements();
    }
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
            FillDataBase();
            PrintElements();
    }

    public void SortListOfDictionaries()
    {
        DataList dbKeys = dataBase.GetKeys();
        for (int i = 0; i < dbKeys.Count; i++)
        {
            DataList currentDataList = dataBase[dbKeys[i]].DataList;
            for (int l = 0; l < currentDataList.Count; l++)
            {
                DataList scoreKeys1 = dataBase[dbKeys[i]].DataList[l].DataDictionary.GetValues();
                int score1 = scoreKeys1[0].Int;
                for (int j = 0; j < currentDataList.Count; j++)
                {
                    DataList scoreKeys2 = dataBase[dbKeys[i]].DataList[j].DataDictionary.GetValues();
                    int score2 = scoreKeys2[0].Int;
                    if (score1 > score2)
                    {
                        DataToken tmp = dataBase[dbKeys[i]].DataList[j];
                        dataBase[dbKeys[i]].DataList[j] = dataBase[dbKeys[i]].DataList[l];
                        dataBase[dbKeys[i]].DataList[l] = tmp;
                    }
                }
            }
        }
    }
}


