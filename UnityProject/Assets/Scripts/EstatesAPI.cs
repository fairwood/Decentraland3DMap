using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class EstatesAPI
{
    public const string API_URL = DclMap.API_URL + "/estates";

    public static IEnumerator AsyncFetchAll()
    {
        //获取数量
        var www = new WWW(API_URL + "?status=open&limit=0");
        yield return www;
        if (www.error != null)
        {
            Debug.LogError(www.error);
            yield break;
        }

        var response = JsonConvert.DeserializeObject<EstatesResponse>(www.text);
        const int step = 20;
        var total = response.data.total;

        for (int i = 0; i <= total/step; i++)
        {
            DclMap.Instance.StartCoroutine(AsyncFetch(step, i * step));
        }
    }

    static IEnumerator AsyncFetch(int limit, int offset)
    {
        var www = new WWW(string.Format(API_URL + "?status=open&limit={0}&offset={1}", limit, offset));
        yield return www;
//        Debug.Log(www.text);

        var response = JsonConvert.DeserializeObject<EstatesResponse>(www.text);

        for (int i = 0; i < response.data.estates.Count; i++)
        {
            var estate = response.data.estates[i];

            EstateInfo estateInfo;
            var findInd = DclMap.EstateInfos.FindIndex(e => e.Estate.id == estate.id);
            if (findInd >= 0)
            {
                estateInfo = DclMap.EstateInfos[findInd];
                estateInfo.Update(estate);
            }
            else
            {
                estateInfo = new EstateInfo(estate);
                DclMap.EstateInfos.Add(estateInfo);
            }

            var data = estate.data;
            var parcels = data.parcels;
            for (int j = 0; j < parcels.Count; j++)
            {
                var coord = parcels[j];
                var index = DclMap.CoordinatesToIndex(coord.x, coord.y);
                DclMap.ParcelInfos[index] = new ParcelInfo(index)
                {
                    EstateInfo = estateInfo
                };
            }

        }
    }
}

public class EstatesResponse
{
    public bool ok;
    public EstatesData data;
}

public class EstatesData
{
    public List<Estate> estates;
    public int total;
}

public class Estate
{
    public string id;
    public string owner;
    public EstateData data;
    public Publication publication;
    public string token_id;
}

public class EstateData
{
    public string ipns;
    public string name;
    public List<Coordinates> parcels;
    public int version;
    public string description;
}