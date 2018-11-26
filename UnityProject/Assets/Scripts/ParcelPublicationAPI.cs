using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class ParcelPublicationAPI
{
    public const string API_URL = DclMap.API_URL + "/parcels/{0}/{1}/publications";
    
    static IEnumerator AsyncFetch(int x, int y)
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
    }
}

public class ParcelPublicationsResponse
{
    public bool ok;
    public List<Publication> data;
}