using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class LevelBuilder : NetworkBehaviour
{
    private const float PlatformUndergroundOffset = 0.35f;
    private const float ColumnsSkyOffset = 25f;

    public GameObject PlatformPrefab;

    public GameObject ColumnCorePrefab;
    public GameObject ColumnDecorPrefab;


    [SerializeField] private Material _material;
    [SerializeField] private float _distance = 4;
    [SerializeField] private int _size = 6;
    [SerializeField] private int _angle = 60;

    private float _offsetY;

    [SyncVar] public int CurrentRestartIteration = 0;
    [SyncVar] public int colorId = 0;

    private int restartIteration = 0;
    private bool _isFirstStart = true;
    private bool _alreadyReceived;
    private bool _platformsDestructionComplete = false;
    private bool _columnsDestructionComplete = false;
    private Coroutine _currentLevelDestroyer;

    private readonly List<List<GameObject>> _platformLayers = new List<List<GameObject>>();
    private readonly List<GameObject> _columns = new List<GameObject>();

    private GameObject _spawnedGameObject;

    public struct Column
    {
        public NetworkInstanceId Id;
        public float RandomScale;
        public NetworkInstanceId[] DecorIds;
        public float[] DecorScaleX;
        public float[] DecorScaleZ;

        public Column(NetworkInstanceId id, float randomScale, NetworkInstanceId[] decorIds, float[] decorScaleX, float[] decorScaleZ)
        {
            Id = id;
            RandomScale = randomScale;
            DecorIds = decorIds;
            DecorScaleX = decorScaleX;
            DecorScaleZ = decorScaleZ;
        }
    }
    public class SyncListColumns : SyncListStruct<Column>
    {
    }

    public SyncListColumns SyncedColumns = new SyncListColumns();


    public struct PlatformLayer
    {
        public NetworkInstanceId[] PlatformIds;

        public PlatformLayer(NetworkInstanceId[] platformIds)
        {
            PlatformIds = platformIds;
        }
    }
    public class SyncListPlatformLayers : SyncListStruct<PlatformLayer>
    {
    }

    public SyncListPlatformLayers SyncedPlatforms = new SyncListPlatformLayers();



    public override void OnStartServer()
    {
        GenerateNewLevel(5, 60, 0, 2);
    }

    public void Update()
    {
        if (isServer && Input.GetKeyDown(KeyCode.L))
        {
            StartCoroutine(DebugRebuild());
        }
    }

    private IEnumerator DebugRebuild()
    {
        CmdDestroyCurrentLevel();
        yield return new WaitForSeconds(0.65f);   
        GenerateRandomLevel();
    }

    public void CmdDestroyCurrentLevel()
    {
        if (_currentLevelDestroyer != null)
        {
            StopCoroutine(_currentLevelDestroyer);
        }
        StartCoroutine(DestroyCurrentLevelCoroutine());

    }

    public IEnumerator DestroyCurrentLevelCoroutine()
    {
        StartCoroutine(DestroyColumnsAfterDelay(0.8f));
        yield return StartCoroutine(DestroyPlatformsOverTime(0.2f));
        while (!(_platformsDestructionComplete && _columnsDestructionComplete))
        {
            yield return new WaitForSeconds(0.05f);
        }
        yield return new WaitForSeconds(0.2f);

        _platformsDestructionComplete = false;
        _columnsDestructionComplete = false;

        SyncedPlatforms.Clear();
        foreach (var column in _columns)
        {
            //Debug.LogError("DESTROYED COLUMN");
            NetworkServer.Destroy(column);
        }
        SyncedColumns.Clear();
        RpcClearLists();
    }

    public IEnumerator DestroyColumnsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RpcDownriseColumns(11f);
        yield return new WaitForSeconds(0.5f);
        _columnsDestructionComplete = true;
    }

    [ClientRpc]
    private void RpcClearLists()
    {
        _platformLayers.Clear();
        _columns.Clear();
        //Debug.LogError("ERASED COLUMNS");
    }

    public void GenerateRandomLevel()
    {
        var size = Random.Range(4, 8);
        var radius = 360 / Random.Range(3, 9);
        var columnCount = Random.Range(size - 2, size + 4);
        var distance = Random.Range(2.5f, 6f);

        colorId = Random.Range(0, 4);
        GenerateNewLevel(size, radius, columnCount, distance);
    }

    public void GenerateNewLevel(int size, int angle, int columnsAmount, float distance)
    {
        CurrentRestartIteration++;
        StartCoroutine(GenerateNewLevelCoroutine(size, angle, columnsAmount, distance));
    }

    public IEnumerator GenerateNewLevelCoroutine(int size, int angle, int columnsAmount, float distance)
    {
        if (!_isFirstStart)
        {
            yield return new WaitForSeconds(2f);
        }

        yield return new WaitForSeconds(0.05f);

        CmdGenerateLevel(size, angle, columnsAmount, distance);
        RpcSyncColumns();
        RpcDownriseColumns(ColumnsSkyOffset);
        _currentLevelDestroyer = StartCoroutine(DestroyPlatformsOverTime(MatchManager.Instance.ShrinkDelay));
        _isFirstStart = false;
    }

    [ClientRpc]
    public void RpcSyncColumns()
    {
        //Debug.Log("STARTED RpcSyncColumns");
        //Debug.Log("restartIteration = " + restartIteration);
        //Debug.Log("currentRestartIteration = " + currentRestartIteration);
        if (restartIteration == CurrentRestartIteration)
        {
            return;
        }
        //Debug.Log("Passed IF");

        restartIteration = CurrentRestartIteration;
        //if (_alreadyReceived)  //TODO: on restart
        //{
        //    return;
        //}
        //_alreadyReceived = true;

        _spawnedGameObject = new GameObject("spawned");


        var columnsGameObject = new GameObject("columns");

        columnsGameObject.transform.parent = _spawnedGameObject.transform;
        foreach (var column in SyncedColumns)
        {
            var mainChunk = ClientScene.FindLocalObject(column.Id);
            _columns.Add(mainChunk);
            //Debug.LogError("ADDED COLUMN");
            mainChunk.transform.localScale = new Vector3(mainChunk.transform.localScale.x, mainChunk.transform.localScale.y + column.RandomScale, mainChunk.transform.localScale.z);

            var emptyObject = new GameObject("decors");
            emptyObject.transform.parent = mainChunk.transform;

            for (var i = 0; i < column.DecorIds.Length; i++)
            {
                var decorChunk = ClientScene.FindLocalObject(column.DecorIds[i]);

                decorChunk.transform.localScale = new Vector3(decorChunk.transform.localScale.x + column.DecorScaleX[i], decorChunk.transform.localScale.y + column.RandomScale, decorChunk.transform.localScale.z + column.DecorScaleZ[i]);


                decorChunk.transform.parent = emptyObject.transform;
            }
            mainChunk.transform.parent = columnsGameObject.transform;
        }
        //Debug.Log("FINISHED RpcSyncColumns");
    }

    [ClientRpc]
    private void RpcDownriseColumns(float offset)
    {
        //Debug.Log("STARTED RpcDownriseColumns");
        //Debug.Log("_columns = " + _columns);
        //Debug.Log("_columns.Count = " + _columns.Count);
        StartCoroutine(DownriseColumns(offset));
        //Debug.Log("FINISHED RpcDownriseColumns");
    }

    private IEnumerator DownriseColumns(float offset)
    {
        foreach (var column in _columns)
        {
            //Debug.LogError("DOWNRISING COLUMN");
            //Debug.Log("column = " + column);
            StartCoroutine(DownriseColumn(column, offset));
            yield return new WaitForSeconds(0.08f);

        }
        //Debug.Log("FINISHED DownriseColumns");
    }

    private static IEnumerator DownriseColumn(GameObject column, float offset)
    {


        for (var i = 0; i < 13; i++)
        {
            if (column == null)
            {
                break;
            }
            column.transform.Translate(Vector3.down * offset / 10);
            yield return new WaitForSeconds(0.015f - 0.001f * i);
        }
        for (var i = 0; i < 3; i++)
        {
            if (column == null)
            {
                break;
            }
            column.transform.Translate(Vector3.up * offset / 10);
            yield return new WaitForSeconds(0.04f);
        }

        //Debug.Log("FINISHED DownriseColumn");
    }


    [Command]
    public void CmdGenerateLevel(int size, int angle, int columnsAmount, float distance)
    {
        _angle = angle;
        _size = size;
        _distance = distance;

        for (var i = 0; i <= size; i++)
        {
            _platformLayers.Add(new List<GameObject>());
        }

        var initialPlatform = Instantiate(PlatformPrefab);

        _platformLayers[0].Add(initialPlatform);

        _offsetY = initialPlatform.transform.position.y;

        if (size > 1)
        {
            GeneratePlatformsRecursive(initialPlatform, 0);
        }


        initialPlatform.transform.Translate(Vector3.down * PlatformUndergroundOffset);

        SpawnColumnsOnRandomPlatforms(columnsAmount);
        NetworkServer.Spawn(initialPlatform);

        foreach (var platformLayer in _platformLayers)
        {
            var layerNetworkIds = new NetworkInstanceId[platformLayer.Count];

            for (var j = 0; j < platformLayer.Count; j++)
            {
                layerNetworkIds[j] =
                    platformLayer[j].GetComponent<NetworkIdentity>().netId;
            }
            SyncedPlatforms.Add(new PlatformLayer(layerNetworkIds));
        }

        RpcResync();
        RpcSyncColor();
        RpcSyncPlatformsOnClient();
        RpcUprisePlatforms();
    }

    [ClientRpc]
    public void RpcResync()
    {
        // This method automagically sync spawned objects' position. 
    }

    [ClientRpc]
    public void RpcSyncPlatformsOnClient()
    {
        if (!isServer)
        {
            _platformLayers.Clear();

            foreach (var syncListPlatformLayer in SyncedPlatforms)
            {
                _platformLayers.Add(new List<GameObject>());
                foreach (var platformId in syncListPlatformLayer.PlatformIds)
                {
                    _platformLayers[_platformLayers.Count - 1].Add(ClientScene.FindLocalObject(platformId));
                }
            }

            //Debug.Log("I got " + _platformLayers.Count + " of layers");
            //Debug.Log("_platformLayers[0].Count " + _platformLayers[0].Count);
            //Debug.Log("_platformLayers[0][0] " + _platformLayers[0][0]);
        }
    }

    [ClientRpc]
    public void RpcSyncColor()
    {
        SetColor();
    }

    private void SetColor()
    {
        Color32 color;
        switch (colorId)
        {
            case 0:
                {
                    color = new Color32(0xE1, 0x32, 0x00, 0xFF);
                    break;
                }
            case 1:
                {
                    color = new Color32(0xFF, 0x74, 0x00, 0xFF);
                    break;
                }
            case 2:
                {
                    color = new Color32(0x59, 0xCF, 0x2A, 0xFF);
                    break;
                }
            case 3:
                {
                    color = new Color32(0x78, 0xBF, 0xD4, 0xFF);
                    break;
                }
            default:
                {
                    color = new Color32(0xE1, 0x32, 0x00, 0xFF);
                    break;
                }
        }
        _material.SetColor("_Color", color);
    }

    [ClientRpc]
    private void RpcUprisePlatforms()
    {
        //Debug.Log("RpcUprisePlatforms");
        StartCoroutine(UprisePlatformLayers());
    }

    private IEnumerator UprisePlatformLayers()
    {
        //Debug.Log("UprisePlatformLayers");
        //Debug.Log("_platformLayers.Count = " + _platformLayers.Count);
        //Debug.Log("SyncedPlatforms.Count = " + SyncedPlatforms.Count);
        //yield return new WaitForSeconds(2f);
        foreach (var platformLayer in _platformLayers)
        {

            //Debug.Log("platformLayer.Count = " + platformLayer.Count);
            foreach (var platform in platformLayer)
            {
                //Debug.Log("platform = " + platform);
                StartCoroutine(UprisePlatform(platform));
                yield return new WaitForSeconds(0.012f);
            }
            yield return new WaitForSeconds(0.10f);
        }
    }

    private static IEnumerator UprisePlatform(GameObject platform)
    {
        for (var i = 0; i < 17; i++)
        {
            platform.transform.Translate(Vector3.up * PlatformUndergroundOffset / 10);
            yield return new WaitForSeconds(0.007f);
        }

        for (var i = 0; i < 7; i++)
        {
            platform.transform.Translate(Vector3.down * PlatformUndergroundOffset / 10);
            yield return new WaitForSeconds(0.004f);
        }
    }


    private void GeneratePlatformsRecursive(GameObject parentPlatform, int nestedIndex)
    {
        var sideAmount = (360 / _angle);

        if (nestedIndex == 0)
        {
            for (var i = 0; i < sideAmount; i++)
            {
                var platform = InstantiateNewPlatform(parentPlatform, i);
                _platformLayers[nestedIndex + 1].Add(platform);


                if (nestedIndex + 1 < _size)
                {
                    GeneratePlatformsRecursive(platform, nestedIndex + 1);
                }
            }
        }
        else
        {
            if (nestedIndex + 1 < _size)
            {
                var platformFront = InstantiateNewPlatform(parentPlatform, 0);
                _platformLayers[nestedIndex + 1].Add(platformFront);
                if (nestedIndex + 2 < _size)
                {
                    GeneratePlatformsRecursive(platformFront, nestedIndex + 1);
                }
            }


            var platformRight = InstantiateNewPlatform(parentPlatform, +1);
            _platformLayers[nestedIndex + 1].Add(platformRight);


            var switcher = 1;
            for (var i = 0; i < nestedIndex - 1; i++)
            {
                platformRight = InstantiateNewPlatform(platformRight, switcher);
                _platformLayers[nestedIndex + 1].Add(platformRight);

                switcher *= 0;
            }
        }
    }


    private GameObject InstantiateNewPlatform(GameObject parentPlatform, int rotationFactor)
    {
        var platformInstance = Instantiate(PlatformPrefab, parentPlatform.transform.position, parentPlatform.transform.rotation);


        var randomElevation = Random.Range(-0.08f, 0.2f);
        platformInstance.transform.position = new Vector3(platformInstance.transform.position.x, randomElevation + _offsetY - PlatformUndergroundOffset, platformInstance.transform.position.z);

        if (randomElevation < 0)
        {
            var platformRenderer = platformInstance.GetComponent<Renderer>();
            var color = platformRenderer.material.color;
            var remappedElevation = Remap(randomElevation, -0.075f, 0.175f, 0.5f, 1.5f);
            platformRenderer.material.color = new Color(color.r * remappedElevation, color.g * remappedElevation, color.b * remappedElevation);

        }

        platformInstance.transform.Rotate(Vector3.up, _angle * rotationFactor);
        platformInstance.transform.Translate(Vector3.forward * _distance, Space.Self);


        NetworkServer.Spawn(platformInstance);
        return platformInstance;
    }

    private void SpawnColumnsOnRandomPlatforms(int amount)
    {
        var platforms = _platformLayers.SelectMany(x => x).ToList();
        var indexes = GenerateRandomListIndexes(0, platforms.Count, amount);
        foreach (var index in indexes)
        {
            BuildColumnOnPlatform(platforms[index]);
        }
    }

    private void BuildColumnOnPlatform(GameObject parentPlatform)
    {
        var randomScale = Random.Range(-2f, +5f);

        var mainChunk = Instantiate(ColumnCorePrefab, parentPlatform.transform.position, parentPlatform.transform.rotation);

        mainChunk.transform.position = new Vector3(mainChunk.transform.position.x, mainChunk.transform.position.y + (mainChunk.transform.localScale.y + randomScale) * 0.45f + ColumnsSkyOffset, mainChunk.transform.position.z);

        NetworkServer.Spawn(mainChunk);


        var decorCount = Random.Range(1, 4);
        var decorIds = new NetworkInstanceId[decorCount];
        var scalesX = new float[decorCount];
        var scalesZ = new float[decorCount];
        for (var i = 0; i < decorCount; i++)
        {
            var randomElevation = Random.Range(-1f, 0.5f);

            var randomRotation =
                new Vector3(Random.Range(-5.5f, 5.5f), Random.Range(10, 40), Random.Range(-5.5f, 5.5f));

            var decorChunk = Instantiate(ColumnDecorPrefab, parentPlatform.transform.position, parentPlatform.transform.rotation);

            decorChunk.transform.position = new Vector3(decorChunk.transform.position.x, decorChunk.transform.position.y + randomElevation + (decorChunk.transform.localScale.y + randomScale) * 0.45f + ColumnsSkyOffset, decorChunk.transform.position.z);
            decorChunk.transform.rotation = Quaternion.Euler(randomRotation);


            NetworkServer.Spawn(decorChunk);
            decorIds[i] = decorChunk.GetComponent<NetworkIdentity>().netId;  //NetworkInstanceId?
            scalesX[i] = Random.Range(-0.4f, 0.2f);
            scalesZ[i] = Random.Range(-0.2f, 0.2f);
        }

        SyncedColumns.Add(new Column(mainChunk.GetComponent<NetworkIdentity>().netId, randomScale, decorIds, scalesX, scalesZ));
    }

    public static HashSet<int> GenerateRandomListIndexes(int from, int to, int count)
    {
        if (count > to)
        {
            count = to;
        }
        var candidates = new HashSet<int>();
        while (candidates.Count < count)
        {
            candidates.Add(Random.Range(from, to));
        }

        return candidates;
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    private IEnumerator DestroyPlatformsOverTime(float delay)
    {
        yield return new WaitForSeconds(delay);
        for (var i = _platformLayers.Count; i >= 0; i--)
        {
            yield return new WaitForSeconds(delay);
            RpcDownAndDestroyPlatformLayer(i);
        }
        yield return new WaitForSeconds(delay);
    }

    [ClientRpc]
    private void RpcDownAndDestroyPlatformLayer(int layerIndex)
    {
        //Debug.Log("I GOT RPC");
        StartCoroutine(DownAndDestroyPlatformLayer(layerIndex));
    }

    private IEnumerator DownAndDestroyPlatformLayer(int layer)
    {
        if (layer < _platformLayers.Count)
        {
            for (var i = 0; i < 20; i++)
            {
                foreach (var platform in _platformLayers[layer])
                {
                    if (platform != null)
                    {
                        platform.transform.Translate(Vector3.down * PlatformUndergroundOffset / 20);
                    }

                }
                yield return new WaitForSeconds(0.010f);

            }

            if (isServer)
            {
                foreach (var platform in _platformLayers[layer])
                {
                    NetworkServer.Destroy(platform);
                }
            }
            _platformLayers.RemoveAt(layer);
        }

        _platformsDestructionComplete = true;
    }
}
