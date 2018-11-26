using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class ParcelPublicationAPI
{
    public const string API_URL = DclMap.API_URL + "/parcels/{0}/{1}/publications";

    public static IEnumerator AsyncFetchAll()
    {
        var counter = new Counter();
        for (int i = 0; i <= DclMap.N; i++)//TODO：太可怕了
        {
            var coord = DclMap.IndexToCoordinates(i);
            DclMap.Instance.StartCoroutine(AsyncFetch(coord.x, coord.y, counter));
        }

        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (Time.frameCount %50  == 0) Debug.Log("rC="+counter.responseCount);
            if (counter.responseCount >= DclMap.N)
            {
                break;
            }
        }
    }

    public static IEnumerator AsyncFetch(int x, int y, Counter counter = null)
    {
        var www = new WWW(string.Format(API_URL, x, y));
        yield return www;

        var response = JsonConvert.DeserializeObject<ParcelPublicationsResponse>(www.text);

        var index = DclMap.CoordinatesToIndex(x, y);
        if (DclMap.ParcelInfos[index] == null) DclMap.ParcelInfos[index] = new ParcelInfo();
        DclMap.ParcelInfos[index].SoldPublications.Clear();

        for (int i = 0; i < response.data.Count; i++)
        {
            var publication = response.data[i];
            if (publication.status == "sold")
            {
                DclMap.ParcelInfos[index].SoldPublications.Add(publication);
            }
        }

        DclMap.ParcelInfos[index].LastFetchPublicationsTime = Time.realtimeSinceStartup;
        if (counter != null) counter.responseCount++;
    }

    public class Counter
    {
        public int responseCount;
    }
}

public class ParcelPublicationsResponse
{
    public bool ok;
    public List<Publication> data;
}