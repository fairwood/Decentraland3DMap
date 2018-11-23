using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class DistrictsAPI
{
    public const string API_URL = DclMap.API_URL + "/districts";

    public static IEnumerator AsyncFetchAll()
    {
        var www = new WWW(API_URL);
        yield return www;
        Debug.Log(www.text);
        if (www.error != null)
        {
            Debug.LogError(www.error);
            yield break;
        }

//        var response = JsonConvert.DeserializeObject<DistrictsResponse>(www.text);
    }
}

public class DistrictsResponse
{
    public bool ok;
    public List<District> data;
}

public class District
{
    public string id;
    public string name;
    public string description;
    public string link;
    public bool @public;
    public int parcel_count;
    public int priority;
    public string center;
}