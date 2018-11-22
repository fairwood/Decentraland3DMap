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

    public readonly Matrix4x4[] CubeMatrix4X4s = new Matrix4x4[N];

    public GameObject ParcelPrefab;

    public Material CubeMaterial;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        for (int index = 0; index < N; index++)
        {
            var coordinates = IndexToCoordinates(index);
            var go = CreateParcelCube(coordinates.x, coordinates.y, 1);//TODO:不需要真的生成物体
            CubeMatrix4X4s[index] = go.transform.worldToLocalMatrix;
            Destroy(go);
        }

        StartCoroutine(AsyncFetchParcels()); //从DCL官方API拉取地图数据
    }
    
    void Update()
    {
//            Graphics.DrawMeshInstanced(PrimitiveHelper.Cube, 0, CubeMaterial, CubeMatrix4X4s, 1023);
//        for (int i = 0; i < N; i++)
//        {
//            Graphics.DrawMesh(PrimitiveHelper.Cube, ParcelCubes[i].transform.worldToLocalMatrix, CubeMaterial, 0);
//        }
    }

    public IEnumerator AsyncFetchParcels()
    {
        var www = new WWW(API_URL + "/map?nw=10,12&se=12,10");
        yield return www;
        Debug.Log(www.text);

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
}

public struct Coordinates
{
    public int x, y;

    public Coordinates(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

public class MapResponse
{
    public bool ok;
    public MapData data;
}

public class MapData
{
    public MapAssets assets;
    public int total;
}

public class MapAssets
{
    public List<Parcel> parcels;
    //estates
}
public class Parcel
{
    public string id;
    public int x, y;
    public double? auction_price;
    public string district_id;
    public string owner;
    //data
    public string auction_owner;
    //tags
    //last_transferred_at
    //estate_id
    //update_operator
    //publication
}