﻿using System.Collections;
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

    public float RefreshInterval = 60;

    private float nextRefreshTime = 1;
    
//    public readonly GameObject[] ParcelCubes = new GameObject[N];

    /// <summary>
    /// 储存所有Parcel的数据
    /// </summary>
    public static readonly ParcelInfo[] ParcelInfos = new ParcelInfo[N];

    public static readonly List<EstateInfo> EstateInfos = new List<EstateInfo>();

    public static readonly BoxCollider[] ParcelBoxColliders = new BoxCollider[N];
    
    public GameObject ParcelMouseTriggerPrefab;

    public GameObject SelectedCube;

    public Material CubeMaterial;

    #region Instancing Render
    
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
    
    #endregion

    public static int? HoveredParcelIndex;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
        CreateMouseTriggers();

        InvokeRepeating("UpdateColliders", 5, 5);
    }

    void Update()
    {
        // Update starting position buffer
        //        if (cachedInstanceCount != instanceCount || cachedSubMeshIndex != subMeshIndex)
        UpdateBuffers();

        // Render
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, instanceMaterial,
            new Bounds(Vector3.zero, new Vector3(10000.0f, 10000.0f, 10000.0f)), argsBuffer);

        HoveredParcelIndex = null;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        var hit = Physics.Raycast(ray, out hitInfo, 1e8f, 1 << 8, QueryTriggerInteraction.Collide);
        if (hit)
        {
            int index;
            if (int.TryParse(hitInfo.collider.name, out index))
            {
                var boxCldr = hitInfo.collider as BoxCollider;
                SelectedCube.transform.position = hitInfo.transform.position + new Vector3(0, boxCldr.center.y, 0);
                SelectedCube.transform.localScale = new Vector3(10, Mathf.Max(0.001f, boxCldr.size.y), 10);
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    var coord = IndexToCoordinates(index);
                    var price = GetPriceOfParcel(index);
                    Debug.Log("hit on " + coord + "|" + price);
                }

                HoveredParcelIndex = index;
            }
        }

        if (Time.realtimeSinceStartup > nextRefreshTime)
        {
            StartCoroutine(ParcelsAPI.AsyncFetchAll()); //从DCL官方API拉取地图数据
            StartCoroutine(EstatesAPI.AsyncFetchAll());
            nextRefreshTime = Time.realtimeSinceStartup + RefreshInterval;
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
            var price = GetPriceOfParcel(i);
            if (price >= 0)
            {
                var height = PriceToHeight(price);
                positions[i] = new Vector4(coord.x * 10, height / 2, coord.y * 10, height);
            }
            else
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
            var price = GetPriceOfParcel(i);
            float height;
            if (price >= 0)
            {
                height = Mathf.Clamp(PriceToHeight(price), 0, 1e6f);
            }
            else
            {
                height = 0;
            }
            ParcelBoxColliders[i].center = new Vector3(0, height / 2, 0);
            ParcelBoxColliders[i].size = new Vector3(10, height, 10);
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
        return Mathf.Pow(100000f / price, 3);
        return Mathf.Max(0.01f, Mathf.Pow(price, 0.5f));
    }

    public float GetPriceOfParcel(int index)
    {
        var parcelInfo = ParcelInfos[index];

        if (parcelInfo != null)
        {
            if (parcelInfo.EstateInfo != null)
            {
                if (parcelInfo.EstateInfo.Estate.publication != null && parcelInfo.EstateInfo.Estate.publication.status == "open")
                {
                    return (float) parcelInfo.EstateInfo.Estate.publication.price/parcelInfo.EstateInfo.Estate.data.parcels.Count;
                }
            }
            else if (parcelInfo.Parcel != null && parcelInfo.Parcel.publication != null && parcelInfo.Parcel.publication.status == "open")
            {
                return (float) parcelInfo.Parcel.publication.price;
            }
        }

        return -1;
    }
}

