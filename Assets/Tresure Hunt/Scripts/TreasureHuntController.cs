using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;


public class TreasureHuntController : UdonSharpBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Disable if you want to use only random spawner. Will disable return button")]
    [SerializeField]
    private bool isTreasureHunt = true;

    [Tooltip("Adds spinning animation to treasure")]
    [SerializeField] private bool isAnimated = true;

    [Tooltip("How many treasures can be active at the same time")]
    [SerializeField]
    private int amountOfTreasuresActive = 1;




    [Tooltip("Range of randomly generated timer value to spawn. Set both to 0 to make treasures spawn immediately")]
    [Header("Timer Settings")]
    public float timerMin = 0f;
    public float timerMax = 0f;

    [UdonSynced] private bool[] isTreasureActive;
    [UdonSynced] private Vector3[] treasurePositions;
    [UdonSynced] private float spawnTimer;
     private BoxCollider[] boxColliders;
     private GameObject[] gameObjectsToSpawn;


    [Header("Utilities. Do not change anything here")]
    [Tooltip("Transform object used to position things")]
    [SerializeField] private Transform spawnTransform;
    [Tooltip("Button with script that calls event for despawn.")]
    [SerializeField] private GameObject despawnerPrefab;
    [Tooltip("Prefab with spin animation")]
    [SerializeField] private GameObject containerPrefab;
    [Tooltip("this is where all prepared treasures will be relocated")]
    [SerializeField] private Transform treasuresContainer;
    [Tooltip("Parent of all zones where treasures will be spawned. Zones designated by box colliders")]
    [SerializeField]
    private Transform placesPool;

    [Tooltip("Place your treasures here")]
    [SerializeField]
    private Transform treasuresPool;

    private VRCPlayerApi _localPlayer;
    void Start()
    {
        _localPlayer = Networking.LocalPlayer;
        Initiate();

    }

    void Update()
    {
        if (Networking.IsOwner(gameObject))
        {
            if (spawnTimer > 0)
            {
                spawnTimer -= Time.deltaTime;
            }
            else
            {
                SpawnTreasure();
            }
        }
    }

    #region Public Methods
    public void Initiate()
    {

        boxColliders = placesPool.GetComponentsInChildren<BoxCollider>();
        gameObjectsToSpawn = new GameObject[treasuresPool.childCount];
        // Add parent container
        int itemCount = treasuresPool.childCount;
        for (int i = 0; i < itemCount; i++)
        {
            GameObject newInstance = Instantiate(containerPrefab);
            if (!isAnimated) { newInstance.transform.GetComponent<Animator>().enabled = false; }
            newInstance.name = treasuresPool.GetChild(0).name;
            treasuresPool.GetChild(0).transform.SetParent(newInstance.transform.GetChild(0), false);
            newInstance.transform.SetParent(treasuresContainer, false);
        }
        // Populate object pools from children
        for (int i = 0; i < itemCount; i++)
        {
            gameObjectsToSpawn[i] = treasuresContainer.GetChild(i).gameObject;
            gameObjectsToSpawn[i].SetActive(false);
        }

        // Construct data arrays to match pool
        isTreasureActive = new bool[itemCount];
        treasurePositions = new Vector3[itemCount];

        // Create return buttons for each treasure
        if (isTreasureHunt)
        {
            for (int i = 0; i < gameObjectsToSpawn.Length; i++)
            {
                Transform parentTransform = gameObjectsToSpawn[i].transform.GetChild(0).GetChild(0);
                GameObject newInstance = Instantiate(despawnerPrefab, parentTransform.position, parentTransform.rotation, parentTransform);
                ScaleToMatchBounds(newInstance, gameObjectsToSpawn[i].transform.GetChild(0).GetChild(0).gameObject);
                newInstance.name = i.ToString();
            }
        }


        StartSpawnCycle();
    }
    public void StartSpawnCycle()
    {
        if (timerMin == 0 && timerMax == 0)
        {
            SpawnTreasure();
        }
        else
        {
            spawnTimer = Random.Range(timerMin, timerMax);
        }
    }

    //Check isActive state, VRCSync and update scene
    public void ReplicateStates()
    {
        for (int i = 0;i < gameObjectsToSpawn.Length; i++)
        {
            gameObjectsToSpawn[i].SetActive(isTreasureActive[i]);
            VRCObjectSync vrcSyncToSpawn = gameObjectsToSpawn[i].GetComponent<VRCObjectSync>();
            if (vrcSyncToSpawn == null)
           {
            //    Debug.Log($"{gameObjectsToSpawn[i].name} is not using VRCSync, replicating using transform.");
                gameObjectsToSpawn[i].transform.position = treasurePositions[i];
           }
                else
           {
            //    Debug.Log($"{gameObjectsToSpawn[i].name} using VRCSync, replicating using TeleportTo.");
                spawnTransform.position = treasurePositions[i];
                vrcSyncToSpawn.FlagDiscontinuity();
                vrcSyncToSpawn.TeleportTo(spawnTransform);
           }
            
        }

    }
    public void ReturnItem(int indexToReturn)
    {
        TakeOwnership();
        isTreasureActive[indexToReturn] = false;
        RequestSerialization();
        SendCustomNetworkEvent(NetworkEventTarget.All, "ReplicateStates");
    }


    #endregion
    private void SpawnTreasure()
    {
        if (!Networking.IsOwner(gameObject)) { return; }
        if (!isTreasureAvailable())
        {
          //  Debug.Log("No treasures available!");
            StartSpawnCycle();
            return;
        }

        // Count active treasures
        int activeCount = 0;
        foreach (GameObject obj in gameObjectsToSpawn)
        {
            if (obj.activeSelf) activeCount++;
        }

        // Generate new random data for new spawn
        if (activeCount < amountOfTreasuresActive)
        {
            BoxCollider randomCollider = boxColliders[Random.Range(0, boxColliders.Length)];
            Vector3 randomPosition = GetRandomPositionInBoxCollider(randomCollider);
            int treasureIndexToSpawn = GetRandomTreasureThatIsNotActive();
            isTreasureActive[treasureIndexToSpawn] = true;
            treasurePositions[treasureIndexToSpawn] = randomPosition;
            Debug.Log($"New random data generated! New prop is {gameObjectsToSpawn[treasureIndexToSpawn].name}. Coordinates are {randomPosition} ");
        }

        // Start the next cycle and serialize
        StartSpawnCycle();
        RequestSerialization();
        SendCustomNetworkEvent(NetworkEventTarget.All, "ReplicateStates");
    }

    #region Utilites

    //This checks isActive array for false values, and returns one of them random as int
    int GetRandomTreasureThatIsNotActive()
    {
        int treasureIndexToSpawn = 0;

        int falseCount = 0;
        for (int i = 0; i < isTreasureActive.Length; i++)
        {
            if (!isTreasureActive[i])
            {
                falseCount++;
            }
        }

        if (falseCount == 0)
        {
            Debug.LogWarning("All values in the array are true.");
            return -1;
        }

        int randomFalseIndex = UnityEngine.Random.Range(0, falseCount);
        for (int i = 0; i < isTreasureActive.Length; i++)
        {
            if (!isTreasureActive[i])
            {
                if (randomFalseIndex == 0)
                {
                    treasureIndexToSpawn = i;
                }
                randomFalseIndex--;
            }
        }
        return treasureIndexToSpawn;
    }

    //Generates random position inside boxcollider
    Vector3 GetRandomPositionInBoxCollider(BoxCollider boxCollider)
    {
        Vector3 extents = boxCollider.size / 2f;
        Vector3 point = new Vector3(
            Random.Range(-extents.x, extents.x),
            Random.Range(-extents.y, extents.y),
            Random.Range(-extents.z, extents.z)
        );

        return boxCollider.transform.TransformPoint(point);
    }

    //Method to scale return buttons to prop bounds
    private void ScaleToMatchBounds(GameObject objectToScale, GameObject referenceObject)
    {
        Renderer referenceRenderer = referenceObject.GetComponentInChildren<Renderer>();
        Renderer objectRenderer = objectToScale.GetComponent<Renderer>();
        Bounds parentBounds = referenceRenderer.bounds;
        Bounds childBounds = objectRenderer.bounds;

        // Calculate scale factors for each axis
        Vector3 scaleFactors = new Vector3(
            parentBounds.size.x / childBounds.size.x,
            parentBounds.size.y / childBounds.size.y,
            parentBounds.size.z / childBounds.size.z
        );

        // Apply the scale to the child object
        objectToScale.transform.localScale = Vector3.Scale(objectToScale.transform.localScale, scaleFactors);
    }
    public bool isTreasureAvailable()
    {
        for (int i = 0; i < treasuresContainer.childCount; i++)
        {

            if (!treasuresContainer.GetChild(i).gameObject.activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    private void TakeOwnership()
    {
        if (!Networking.IsOwner(gameObject)) { Networking.SetOwner(_localPlayer, gameObject); }
    }

    #endregion



}
