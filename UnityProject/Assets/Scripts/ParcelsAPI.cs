﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ParcelsAPI
{
    public const string API_URL = DclMap.API_URL + "/parcels";

    public static IEnumerator AsyncFetchAllOpen()
    {
        //获取数量
        var www = new WWW(API_URL+"?status=open&limit=0");
        yield return www;
//        Debug.Log(www.text);
        if (www.error != null)
        {
            Debug.LogError(www.error);
            yield break;
        }

        var response = JsonConvert.DeserializeObject<ParcelsResponse>(www.text);
        const int step = 20;
        var total = response.data.total;

        for (int i = 0; i <= total / step; i++)
        {
            DclMap.Instance.StartCoroutine(AsyncFetchOpen(step, i * step));
        }
    }

    static IEnumerator AsyncFetchOpen(int limit, int offset)
    {
        var www = new WWW(string.Format(API_URL + "?status=open&limit={0}&offset={1}", limit, offset));
        yield return www;

        var response = JsonConvert.DeserializeObject<ParcelsResponse>(www.text);

        for (int i = 0; i < response.data.parcels.Count; i++)
        {
            var parcel = response.data.parcels[i];

            var index = DclMap.CoordinatesToIndex(parcel.x, parcel.y);
            DclMap.ParcelInfos[index].Update(parcel);
        }
    }

    public static IEnumerator AsyncFetch(int x, int y)
    {
        var www = new WWW(string.Format(API_URL + "/{0}/{1}", x, y));
        yield return www;

        var response = JsonConvert.DeserializeObject<SingleParcelResponse>(www.text);

        var parcel = response.data;

        var index = DclMap.CoordinatesToIndex(parcel.x, parcel.y);
        DclMap.ParcelInfos[index].Update(parcel);
    }
}

public class ParcelsResponse
{
    public bool ok;
    public ParcelsData data;
}
public class SingleParcelResponse
{
    public bool ok;
    public Parcel data;
}

public class ParcelsData
{
    public List<Parcel> parcels;
    public int total;
}
public class Parcel
{
    public string id;
    public int x, y;
    public double? auction_price;
    public string district_id;
    public string owner;
    public Data data;
    public string auction_owner;
    public ParcelTags tags;
    //last_transferred_at
    public string estate_id;
    //update_operator
    public Publication publication;

    public class Data
    {
        public string ipns;
        public string name;
        public int version;
        public string description;
    }
}

public class ParcelTags
{
    public ParcelProximity proximity;
}

public class ParcelProximity
{
    public ParcelProximityDistrictData road;
    public ParcelProximityDistrictData plaza;
}
public class ParcelProximityDistrictData
{
    public string district_id;
    public int distance;
}