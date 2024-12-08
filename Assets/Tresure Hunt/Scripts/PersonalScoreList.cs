
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDK3.Persistence;
using VRC.SDKBase;
using VRC.Udon;

public class PersonalScoreList : UdonSharpBehaviour
{

    DataList Names = new DataList();

    DataList listOfTreasures = new DataList();
    DataDictionary dataBase = new DataDictionary();


    [SerializeField] private Transform treasuresPool;
    [SerializeField] private Transform listsRoot;


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
        PrintElements();
        //  FillValues();
    }
    public void InitiateItems()
    {
        dataBase.Clear();

        //Build a list of unique treasures
        for (int i = 0; i < treasuresPool.childCount; i++)
        {
            DataToken t = treasuresPool.GetChild(i).name;
            if (!listOfTreasures.Contains(t))
            {
                listOfTreasures.Add(t);
                dataBase.Add(t, 0);
            }
        }
    }

    public void FillDataBase()
    {

        DataList dbKeys = dataBase.GetKeys();

        for (int i = 0; i < dbKeys.Count; i++)
        {




            dataBase.SetValue(dbKeys[i], PlayerData.GetInt(Networking.LocalPlayer, dbKeys[i].String));
        }
    }

    public void PrintElements()
    {
        DeleteElements();
        //Generate name elements of each score board
        DataList dbKeys = dataBase.GetKeys();
        for (int i = 0; i < listOfTreasures.Count; i++)
        {

                GameObject newInstance = Instantiate(entryElementPrefab, listsRoot, false);



                string name = dbKeys[i].ToString();

                string score = dataBase[name].ToString();


                //      string name = dataBase[dbKeys[i]].DataList[p].DataDictionary[allPlayers[p].displayName].Int;
                //      int score = dataBase[dbKeys[i]].DataList[p].DataDictionary[allPlayers[p].displayName].Int;

                TextMeshProUGUI[] textFields = newInstance.transform.GetComponentsInChildren<TextMeshProUGUI>();


                textFields[0].text = name;
                textFields[1].text = score;



        }
    }

    public void FillValues()
    {
        DataList dbKeys = dataBase.GetKeys();
        for (int i = 0; i < listOfTreasures.Count; i++)
        {
            Transform currentHiScoreList = listsRoot.transform.GetChild(i);
            for (int p = 0; p < allPlayers.Length; p++)
            {

                Transform currentHiScoreListElementsRoot = currentHiScoreList.GetChild(1);
                GameObject newInstance = Instantiate(entryElementPrefab, currentHiScoreListElementsRoot.position, currentHiScoreListElementsRoot.rotation, currentHiScoreListElementsRoot);
                Debug.Log($"Creating elements{currentHiScoreListElementsRoot}");

                DataList nameKeys = dataBase[dbKeys[i]].DataList[p].DataDictionary.GetKeys();
                string name = nameKeys[0].ToString();
                DataList scoreKeys = dataBase[dbKeys[i]].DataList[p].DataDictionary.GetValues();
                string score = scoreKeys[0].ToString();


                TextMeshProUGUI[] textFields = newInstance.transform.GetComponentsInChildren<TextMeshProUGUI>();
                textFields[0].text = name;
                textFields[1].text = score.ToString();
            }
        }
        Debug.Log($"Fill Values");
    }





    public void DeleteElements()
    {



            foreach (Transform child in listsRoot)
            {
                Destroy(child.gameObject);
            }

    }


    public void PopulateListOfPlayers()
    {
        VRCPlayerApi.GetPlayers(allPlayers);
        foreach (VRCPlayerApi player in allPlayers)
        {
            if (player == null) continue;
            //          Debug.Log(player.displayName);
        }
        Debug.Log($"Deleting all boards");
    }

    public override void OnPlayerDataUpdated(VRCPlayerApi player, PlayerData.Info[] infos)
    {
        FillDataBase();
        PrintElements();
        Debug.Log($"{infos}");

    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {

        FillDataBase();
        //      PrintLists();
        PrintElements();
        //   FillValues();


    }
    public override void OnPlayerLeft(VRCPlayerApi player)
    {

        FillDataBase();
        //     PrintLists();
        PrintElements();
        //   FillValues();

    }

    public void SortListOfDictionaries()
    {

        //  listOfScoreboards.Sort();

        DataList dbKeys = dataBase.GetKeys();



        for (int i = 0; i < dbKeys.Count; i++)
        {

            DataList currentDataList = dataBase[dbKeys[i]].DataList;

            for (int l = 0; l < currentDataList.Count; l++)

            {
                DataList scoreKeys1 = dataBase[dbKeys[i]].DataList[l].DataDictionary.GetValues();
                int score1 = scoreKeys1[0].Int;

                for (int j = l + 1; j < currentDataList.Count; j++)

                {
                    DataList scoreKeys2 = dataBase[dbKeys[i]].DataList[j].DataDictionary.GetValues();
                    int score2 = scoreKeys1[0].Int;

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
