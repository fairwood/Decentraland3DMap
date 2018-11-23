using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// https://docs.decentraland.org/blockchain-interactions/api/
/// </summary>
public class DclMap : MonoBehaviour
{
    public static DclMap Instance { get; private set; }

    public const string API_URL = "https://api.decentraland.org/v1";

    public const int N = 301 * 301;
    
//    public readonly GameObject[] ParcelCubes = new GameObject[N];

    /// <summary>
    /// 储存所有Parcel的数据
    /// </summary>
    public readonly ParcelInfo[] ParcelInfos = new ParcelInfo[N];

    public readonly List<EstateInfo> EstateInfos = new List<EstateInfo>();
    
    public GameObject ParcelPrefab;

    public Material CubeMaterial;


    public int instanceCount = DclMap.N;
    public Mesh instanceMesh;
    public Material instanceMaterial;
    public int subMeshIndex = 0;
    private int cachedInstanceCount = -1;
    private int cachedSubMeshIndex = -1;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer scaleBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        StartCoroutine(ParcelsAPI.AsyncFetchAll()); //从DCL官方API拉取地图数据
        StartCoroutine(EstatesAPI.AsyncFetchAll());

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
    }

    void Update()
    {
        // Update starting position buffer
        //        if (cachedInstanceCount != instanceCount || cachedSubMeshIndex != subMeshIndex)
        UpdateBuffers();

        // Render
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial, new Bounds(Vector3.zero, new Vector3(10000.0f, 10000.0f, 10000.0f)), argsBuffer);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var hitPos = ray.GetPoint(-ray.origin.y / ray.direction.y);
            var coord = new Coordinates(Mathf.RoundToInt(hitPos.x / 10), Mathf.RoundToInt(hitPos.z / 10));
            Debug.Log("hit on " + coord);
        }
    }

    void UpdateBuffers()
    {
        // Ensure submesh index is in range
        if (instanceMesh != null)
            subMeshIndex = Mathf.Clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1);

        // Positions
        if (positionBuffer != null)
            positionBuffer.Release();
        positionBuffer = new ComputeBuffer(instanceCount, 16);
        Vector4[] positions = new Vector4[instanceCount];
        for (int i = 0; i < instanceCount; i++)
        {
            var coord = IndexToCoordinates(i);
            var hasInfo = false;
            var parcelInfo = ParcelInfos[i];
            if (parcelInfo != null)
            {
                if (parcelInfo.Parcel != null)
                {
                    if (parcelInfo.Parcel.publication != null)
                    {
                        var publication = parcelInfo.Parcel.publication;
                        var height = publication == null || publication.status != "open"
                            ? 0
                            : PriceToHeight((float) publication.price);
                        positions[i] = new Vector4(coord.x * 10, height / 2, coord.y * 10, height);
                        hasInfo = true;
                    }
                }
                else
                {
                    var estateInfo = parcelInfo.EstateInfo;
                    if (estateInfo != null)
                    {
                        var publication = estateInfo.Estate.publication;
                        var height = publication == null || publication.status != "open" ? 0 : PriceToHeight((float)publication.price/estateInfo.Estate.data.parcels.Count);
                        positions[i] = new Vector4(coord.x * 10, height / 2, coord.y * 10, height);
                        hasInfo = true;
                    }
                }
            }
            if (!hasInfo)
            {
                positions[i] = new Vector4(coord.x * 10, -1, coord.y * 10, 0);
            }
        }
        positionBuffer.SetData(positions);
        instanceMaterial.SetBuffer("positionBuffer", positionBuffer);

        // Scales
        //        if (scaleBuffer != null)
        //            scaleBuffer.Release();
        //        scaleBuffer = new ComputeBuffer(instanceCount, 16);
        //        Vector4[] scales = new Vector4[instanceCount];
        //        for (int i = 0; i < instanceCount; i++)
        //        {
        //            
        //            scales[i] = new Vector4(10, 0, 10, 5f);
        //        }
        //        scaleBuffer.SetData(scales);
        //        instanceMaterial.SetBuffer("scaleBuffer", scaleBuffer);

        // Indirect args
        if (instanceMesh != null)
        {
            args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)instanceCount;
            args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
            args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }
        argsBuffer.SetData(args);

        cachedInstanceCount = instanceCount;
        cachedSubMeshIndex = subMeshIndex;
    }

    public IEnumerator AsyncFetchParcels()
    {
        const int step = 10;
        for (int x = -150; x <= 150; x += step)
        {
            var www = new WWW(string.Format(API_URL + "/map?nw={0},150&se={1},-150", x, Mathf.Min(150, x + step - 1)));
            yield return www;
            Debug.Log(www.text);
            if (www.error == null)
            {
                var mapResponse = JsonConvert.DeserializeObject<MapResponse>(www.text);

                for (int i = 0; i < mapResponse.data.assets.parcels.Count; i++)
                {
                    var parcel = mapResponse.data.assets.parcels[i];

                    var index = CoordinatesToIndex(parcel.x, parcel.y);
                    ParcelInfos[index] = new ParcelInfo
                    {
                        Parcel = parcel
                    };
                }
                yield break;
                yield return new WaitForSecondsRealtime(0.25f);
            }
            else
            {
                Debug.LogError(www.error);
            }
        }
    }

    public GameObject CreateParcelCube(int x, int y, double auction_price)
    {
        var go = Instantiate(ParcelPrefab, transform);
        var height = Mathf.Log10((float) auction_price);
        go.transform.position = new Vector3(x * 10, height, y * 10);
        var parcelObject = go.GetComponent<ParcelObject>();
        parcelObject.Cube.transform.position = new Vector3(0, -height / 2, 0);
        parcelObject.Cube.transform.localScale = new Vector3(10, Mathf.Abs(height), 10);
        return go;
    }

    public static int CoordinatesToIndex(int x, int y)
    {
        return (x + 150) * 301 + y + 150;
    }

    public static int CoordinatesToIndex(Coordinates coordinates)
    {
        return CoordinatesToIndex(coordinates.x, coordinates.y);
    }

    public static Coordinates IndexToCoordinates(int index)
    {
        return new Coordinates(index / 301 - 150, index % 301 - 150);
    }

    public static float PriceToHeight(float price)
    {
        return Mathf.Pow(100000f / price, 3);
        return Mathf.Max(0.01f, Mathf.Pow(price, 0.5f));
    }

    public float GetPriceOfParcel(int index)
    {
        var parcelInfo = ParcelInfos[index];
        return parcelInfo != null && parcelInfo.Parcel.auction_price != null ? (float)parcelInfo.Parcel.auction_price : 0;
    }
}

