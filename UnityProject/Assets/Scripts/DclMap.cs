using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// https://docs.decentraland.org/blockchain-interactions/api/
/// </summary>
public class DclMap : MonoBehaviour
{
    public static DclMap Instance { get; private set; }

    public const string API_URL = "https://api.decentraland.org/v1";

    public const int N = 301 * 301;

    public float RefreshInterval = 60;

    public Color PriceColor;

    private float nextRefreshTime = 1;

    public enum EDataToVisualize
    {
        AskingPrice,
        LastDealPrice
    }

    public static EDataToVisualize DataToVisualize;

    public static bool FilterOnlyRoadside;

//    public readonly GameObject[] ParcelCubes = new GameObject[N];

    /// <summary>
    /// 储存所有Parcel的数据
    /// </summary>
    public static readonly ParcelInfo[] ParcelInfos = new ParcelInfo[N];

    public static readonly bool[] IsRoad = new bool[N];

    public static readonly List<EstateInfo> EstateInfos = new List<EstateInfo>();

    public static readonly BoxCollider[] ParcelBoxColliders = new BoxCollider[N];
    public static readonly bool[] NeedToParcelBoxColliders = new bool[N];

    public GameObject ParcelMouseTriggerPrefab;

    public GameObject SelectedCube;

    public Material CubeMaterial;

    public Texture2D TxtrBaseMap;

    #region Instancing Render

    public int instanceCount = DclMap.N;
    public Mesh instanceMesh;
    public Material instanceMaterial;
    public int subMeshIndex = 0;
    private int cachedInstanceCount = -1;
    private int cachedSubMeshIndex = -1;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer colorBuffer;
    private ComputeBuffer scaleBuffer;
    private ComputeBuffer matrixBuffer;
    private ComputeBuffer argsBuffer;

    private uint[] args = new uint[5] {0, 0, 0, 0, 0};
    private Vector4[] positions = null;
    private Vector4[] colors = null;
    private Vector4[] scales = null;
    private Matrix4x4[] matrixs = null;
    private float[] priceHeight = null;
    private float nextUpdateBufferTime;

    private bool bUpdatePositionBuffer = true;
    private bool bUpdateColorBuffer = true;
    private bool bUpdateMatrixBuffer = true;
    private bool bUpdateScaleBuffer = true;
    private bool bArgsBuffer = true;

//    private float euler_Y = 0f;

    #endregion

    public BoxCollider HoveredBoxCollider;
    public static int? HoveredParcelIndex;

    public delegate void OnParcelCubeClickHandler(int index);

    public event OnParcelCubeClickHandler OnParcelCubeClick;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        positions = new Vector4[instanceCount];
        colors = new Vector4[instanceCount];
        scales = new Vector4[instanceCount];
        matrixs = new Matrix4x4[instanceCount];
        priceHeight = new float[instanceCount];

        for (int i = 0; i < N; i++)
        {
            ParcelInfos[i] = new ParcelInfo(i);
        }

        CreateMouseTriggers();

        //        InvokeRepeating("UpdateColliders", 5, 5);

        UpdateBuffers(bUpdatePositionBuffer, bUpdateColorBuffer, bUpdateMatrixBuffer, bUpdateScaleBuffer, bArgsBuffer);

        ReadMapBaseFromPNG();

//        StartCoroutine(ParcelPublicationAPI.AsyncFetchAllOpen()); 次数太多，吃不消
    }

    void Update()
    {
        // Update starting position buffer
        //        if (cachedInstanceCount != instanceCount || cachedSubMeshIndex != subMeshIndex)

        UpdateBuffers(bUpdatePositionBuffer, bUpdateColorBuffer, bUpdateMatrixBuffer, bUpdateScaleBuffer, bArgsBuffer);

        // Render
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial,
            new Bounds(Vector3.zero, new Vector3(10000.0f, 10000.0f, 10000.0f)), argsBuffer);

        HoveredParcelIndex = null;
        
        if (HoveredBoxCollider)
        {
            int index;
            if (int.TryParse(HoveredBoxCollider.name, out index))
            {
                SelectedCube.transform.position = HoveredBoxCollider.transform.position + new Vector3(0, HoveredBoxCollider.center.y, 0);
                SelectedCube.transform.localScale = new Vector3(10, Mathf.Max(0.001f, HoveredBoxCollider.size.y), 10);
                SelectedCube.SetActive(true);
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    var coord = IndexToCoordinates(index);
                    var price = GetPriceOfParcel(index);
                    Debug.Log("hit on " + coord + "|" + price);
                    for (int dx = -5; dx <= 5; dx++)
                    {
                        for (int dy = -5; dy <= 5; dy++)
                        {
                            var x = coord.x + dx;
                            var y = coord.y + dy;
                            if (-150 <= x && x <= 150 && -150 <= y && y <= 150)
                            {
                                var parcelInfo = ParcelInfos[CoordinatesToIndex(x, y)];
                                if (parcelInfo == null ||
                                    Time.realtimeSinceStartup > parcelInfo.LastFetchPublicationsTime + 30)
                                {
                                    StartCoroutine(ParcelPublicationAPI.AsyncFetch(x, y));
                                    StartCoroutine(ParcelsAPI.AsyncFetch(x, y)); //直接用map API会浪费
                                }
                            }
                        }
                    }
//                    StartCoroutine(MapAPI.AsyncFetch(coord.y + 5, coord.x - 5, coord.y - 5, coord.x + 5, null));//这样用有点浪费，还是一个一个获取比较省

                    if (OnParcelCubeClick != null)
                    {
                        OnParcelCubeClick.Invoke(index);
                    }
                }

                HoveredParcelIndex = index;
            }
        }
        else
        {
            SelectedCube.SetActive(false);
        }

        if (Time.realtimeSinceStartup > nextRefreshTime)
        {
            StartCoroutine(ParcelsAPI.AsyncFetchAllOpen()); //从DCL官方API拉取地图数据
            StartCoroutine(EstatesAPI.AsyncFetchAllOpen());
            nextRefreshTime = Time.realtimeSinceStartup + RefreshInterval;
        }

//        print("FilterOnlyRoadside " + FilterOnlyRoadside);
    }

    void UpdatePositonBuffer(bool b)
    {
        if (b == false)
        {
            return;
        }

        if (positionBuffer != null)
            positionBuffer.Release();
        positionBuffer = new ComputeBuffer(instanceCount, 16);

        for (int i = 0; i < instanceCount; i++)
        {
            var coord = IndexToCoordinates(i);
            var height = GetHeightOfParcel(i);
            if (height >= 0)
            {
                positions[i] = new Vector4(coord.x * 10, height / 2, coord.y * 10, height);
            }
            else
            {
                positions[i] = new Vector4(coord.x * 10, -1, coord.y * 10, 0);
            }
        }

        positionBuffer.SetData(positions);
        instanceMaterial.SetBuffer("positionBuffer", positionBuffer);
    }

    void UpdateColorBuffer(bool b)
    {
        if (b == false)
        {
            return;
        }

        if (colorBuffer != null)
            colorBuffer.Release();
        colorBuffer = new ComputeBuffer(instanceCount, 16);

        for (int i = 0; i < instanceCount; i++)
        {
            var height = GetHeightOfParcel(i);
            if (height >= 0)
            {
                colors[i] = PriceColor;
            }
            else
            {
                colors[i] = new Vector4(1f, 1f, 1f, 1f);
            }
        }

        colorBuffer.SetData(colors);
        instanceMaterial.SetBuffer("colorBuffer", colorBuffer);
    }

    void UpdateMatrixBuffer(bool b)
    {
        if (b == false)
        {
            return;
        }

        if (matrixBuffer != null)
            matrixBuffer.Release();
        matrixBuffer = new ComputeBuffer(instanceCount, 16 * 16);

        for (int i = 0; i < instanceCount; i++)
        {
            matrixs[i] = Matrix4x4.identity;
        }

        matrixBuffer.SetData(matrixs);
        instanceMaterial.SetBuffer("matrixBuffer", matrixBuffer);
    }

    void UpdateScaleBuffer(bool b)
    {
        if (b == false)
        {
            return;
        }

        if (scaleBuffer != null)
            scaleBuffer.Release();
        scaleBuffer = new ComputeBuffer(instanceCount, 16);

        for (int i = 0; i < instanceCount; i++)
        {
            var height = GetHeightOfParcel(i);
            scales[i] = new Vector4(10, height, 10, 5f);
        }

        scaleBuffer.SetData(scales);
        instanceMaterial.SetBuffer("scaleBuffer", scaleBuffer);
    }

    void UpdateArgsBuffer(bool b)
    {
        if (b == false)
        {
            return;
        }

        // Ensure submesh index is in range
        if (instanceMesh != null)
            subMeshIndex = Mathf.Clamp(subMeshIndex, 0, instanceMesh.subMeshCount - 1);

        // Indirect args
        if (instanceMesh != null)
        {
            args[0] = (uint) instanceMesh.GetIndexCount(subMeshIndex);
            args[1] = (uint) instanceCount;
            args[2] = (uint) instanceMesh.GetIndexStart(subMeshIndex);
            args[3] = (uint) instanceMesh.GetBaseVertex(subMeshIndex);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }

        argsBuffer.SetData(args);

        cachedInstanceCount = instanceCount;
        cachedSubMeshIndex = subMeshIndex;
    }

    void UpdatePriceHeight()
    {
        for (int i = 0; i < instanceCount; i++)
        {
            float h = GetHeightOfParcel(i);
            if (h != priceHeight[i])
            {
                priceHeight[i] = h;
                nextUpdateBufferTime = Time.realtimeSinceStartup - 1e-6f;
                NeedToParcelBoxColliders[i] = true;
            }
        }
    }

    void UpdateBuffers(bool bUpdatePositionBuffer, bool bUpdateColorBuffer, bool bUpdateMatrixBuffer,
        bool bUpdateScaleBuffer, bool bArgsBuffer)
    {
        UpdatePriceHeight();
        if (Time.realtimeSinceStartup >= nextUpdateBufferTime) //如果不刷新，就有可能全部消失
        {
            UpdatePositonBuffer(bUpdatePositionBuffer);
            UpdateColorBuffer(bUpdateColorBuffer);
            UpdateMatrixBuffer(bUpdateMatrixBuffer);
            UpdateScaleBuffer(bUpdateScaleBuffer);
            UpdateArgsBuffer(bArgsBuffer);

            UpdateColliders();

            nextUpdateBufferTime = Time.realtimeSinceStartup + 5f;
        }
    }

    void CreateMouseTriggers()
    {
        for (int i = 0; i < N; i++)
        {
            var go = Instantiate(ParcelMouseTriggerPrefab, transform);
            go.name = i.ToString();
            var coord = IndexToCoordinates(i);
            go.transform.position = new Vector3(coord.x * 10, 0, coord.y * 10);
            ParcelBoxColliders[i] = go.GetComponent<BoxCollider>();
        }
    }

    void UpdateColliders()
    {
        for (int i = 0; i < N; i++)
        {
            if (NeedToParcelBoxColliders[i])
            {
                float height = GetHeightOfParcel(i);
                if (height >= 0)
                {
                    height = Mathf.Clamp(height, 0, 1e6f);
                }
                else
                {
                    height = 0;
                }

                ParcelBoxColliders[i].center = new Vector3(0, height / 2, 0);
                ParcelBoxColliders[i].size = new Vector3(10, height, 10);
                NeedToParcelBoxColliders[i] = false;
            }
        }
    }

    public GameObject CreateParcelCube(int x, int y, double auction_price)
    {
        var go = Instantiate(ParcelMouseTriggerPrefab, transform);
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
        return Mathf.Pow(300000f / price, 2);
    }

    public static float PriceToHeight2(float price)
    {
        return Mathf.Max(0.01f, Mathf.Pow(price, 0.5f));
    }

    public static float GetPriceOfParcel(int index)
    {
        var parcelInfo = ParcelInfos[index];

        if (parcelInfo != null)
        {
            if (parcelInfo.EstateInfo != null)
            {
                if (parcelInfo.EstateInfo.Estate.publication != null &&
                    parcelInfo.EstateInfo.Estate.publication.status == "open")
                {
                    return (float) parcelInfo.EstateInfo.Estate.publication.price /
                           parcelInfo.EstateInfo.Estate.data.parcels.Count;
                }
            }
            else if (parcelInfo.Parcel != null && parcelInfo.Parcel.publication != null &&
                     parcelInfo.Parcel.publication.status == "open")
            {
                return (float) parcelInfo.Parcel.publication.price;
            }
        }

        return -1;
    }

    public static float GetLastDealPrice(int index)
    {
        var parcelInfo = ParcelInfos[index];

        if (parcelInfo != null)
        {
            var lastDeal = parcelInfo.SoldPublications.FirstOrDefault(p => p.status == "sold");
            return lastDeal != null ? (float) lastDeal.price : -1;
        }

        return -1;
    }

    public static float GetHeightOfParcel(int index)
    {
        float height = -1;

        var parcelInfo = ParcelInfos[index];

        if (parcelInfo != null)
        {
            if (FilterOnlyRoadside && !parcelInfo.IsRoadside()) return height;

            if (DataToVisualize == EDataToVisualize.AskingPrice)
            {
                var price = GetPriceOfParcel(index);
                if (price >= 0)
                {
                    height = PriceToHeight(price);
                }
            }
            else if (DataToVisualize == EDataToVisualize.LastDealPrice)
            {
                var price = GetLastDealPrice(index);
                if (price >= 0)
                {
                    height = PriceToHeight2(price);
                }
            }
        }

        return height;
    }

    public void ReadMapBaseFromPNG()
    {
        var colors = TxtrBaseMap.GetPixels();
        for (int i = 0; i < N; i++)
        {
            var clr = colors[i];
            if (clr.r > 0.8f)
            {
                IsRoad[i] = true;
            }
        }
    }
}