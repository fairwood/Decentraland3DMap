using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class MapAPI
{
    public const string API_URL = DclMap.API_URL + "/map";

    public static IEnumerator AsyncFetchParcels(MonoBehaviour holder)
    {
        const int step = 1;
        var counter = new Counter();
        for (int x = -150; x <= 150; x += step)
        {
            holder.StartCoroutine(AsyncFetch(150, x, -150, Mathf.Min(150, x+step-1), counter));
        }

        while (true)
        {
            yield return new WaitForEndOfFrame();

            if (counter.responseCount >= Mathf.CeilToInt(301f / step))
            {
                break;
            }
        }
    }

    static IEnumerator AsyncFetch(int n, int w, int s, int e, Counter counter)
    {
        var www = new WWW(string.Format(API_URL + "?nw={0},{1}&se={2},{3}", w,n, e,s));
        yield return www;
//        Debug.Log(counter.responseCount + "|"+ www.text);
        counter.responseCount++;
        if (www.error == null)
        {
            var mapResponse = JsonConvert.DeserializeObject<MapResponse>(www.text);

            for (int i = 0; i < mapResponse.data.assets.parcels.Count; i++)
            {
                var parcel = mapResponse.data.assets.parcels[i];

                var index = DclMap.CoordinatesToIndex(parcel.x, parcel.y);
                DclMap.ParcelInfos[index] = new ParcelInfo
                {
                    Parcel = parcel
                };
            }
        }
        else
        {
            Debug.LogError(www.error);
        }
    }

    class Counter
    {
        public int responseCount;
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
    public List<Estate> estates;
}

public class Publication
{
    public string tx_hash;

    public string tx_status;

    public string owner;

    public double price;

    public string expires_at;

    public string status;

    public string buyer;

    public string contract_id;

    public string block_time_created_at;

    public string block_time_updated_at;

    public string asset_type;

    public string asset_id;

    public string marketplace_address;
    /*
    "tx_hash":"0x3b6784bbf7b8a98d91213483b092e58f83cac5c62fe5a510c5c8f4b1bab31bff",
    "tx_status":"confirmed",
    "owner":"0xf902d068920234957d2908b8b0156e61c0bea2c2",
    "price":17585,
    "expires_at":1543532400000,
    "created_at":"2018-11-15T16:32:20.446",
    "updated_at":"2018-11-15T16:32:20.446",
    "status":"open",
    "buyer":null,
    "contract_id":"0xe5ad15facb00b77fb78c0771589399fdd180bf1eaaa323182fb45360a2145de4",
    "block_number":6643718,
    "block_time_created_at":1541360305000,
    "block_time_updated_at":null,
    "asset_type":"parcel",
    "asset_id":"55,-11",
    "marketplace_address":"0x8e5660b4ab70168b5a6feea0e0315cb49c8cd539"*/
}


public struct Coordinates
{
    public int x, y;

    public Coordinates(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        return string.Format("({0},{1})", x, y);
    }
}