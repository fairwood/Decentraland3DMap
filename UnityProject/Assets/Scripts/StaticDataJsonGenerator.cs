using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class StaticDataJsonGenerator : MonoBehaviour
{
    private bool isFinished = false;
    private bool isFinished1 = false;

    IEnumerator Start()
    {
        path = Application.dataPath + "/Resources";
        yield return StartCoroutine(AsyncFetchWholeMapParcelsBySteps(this));
        yield return StartCoroutine(StaticDataJsonGenerator.DistrictsAPI.AsyncFetchAll());
        isFinished = true;
        Debug.Log("Fetch0 Finish");

        yield return StartCoroutine(ParcelPublicationAPI.AsyncFetchAll(this));
        isFinished1 = true;
        Debug.Log("Fetch1 Finish");
    }

    IEnumerator GenerateJsonFiles()
    {
        while (true)
        {
            if (isFinished) break;
            yield return new WaitForEndOfFrame();
        }

        var json = JsonConvert.SerializeObject(Parcels, Formatting.None);
        File.WriteAllText(Path.Combine(path, "parcels.txt"), json);

        json = JsonConvert.SerializeObject(Estates, Formatting.None);
        File.WriteAllText(Path.Combine(path, "estates.txt"), json);

        json = JsonConvert.SerializeObject(Districts, Formatting.None);
        File.WriteAllText(Path.Combine(path, "districts.txt"), json);

        Debug.Log("Generate files complete!");
    }

    IEnumerator GenerateJsonFiles1()
    {
        while (true)
        {
            if (isFinished1) break;
            yield return new WaitForEndOfFrame();
        }

        var path1 = path + "/Publications";

        if (!Directory.Exists(path1)) Directory.CreateDirectory(path1);

        foreach (var publicationHistory in PublicationHistories)
        {
            var filename = publicationHistory.id + ".txt";
            var json = JsonConvert.SerializeObject(publicationHistory.publications, Formatting.None);
            File.WriteAllText(Path.Combine(path1, filename), json);
        }

        Debug.Log("Generate files 1 complete!");
    }

    static string path;

    private bool hasClickGenerate = false;
    private bool hasClickGenerate1 = false;

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 300, 600));

//        GUILayout.Label("Path");
//        path = GUILayout.TextField(path);

        if (!hasClickGenerate && GUILayout.Button("Generate0"))
        {
            hasClickGenerate = true;
            StartCoroutine(GenerateJsonFiles());
        }

        if (counter != null)
        {
            GUILayout.Label(string.Format("Fetch0 Progress: {0}/301", counter.responseCount));
        }

        if (!hasClickGenerate1 && GUILayout.Button("Generate1"))
        {
            hasClickGenerate1 = true;
            StartCoroutine(GenerateJsonFiles1());
        }
        if (counter1 != null)
        {
            GUILayout.Label(string.Format("Fetch1 Progress: {0}/{1}", counter1.responseCount, DclMap.N));
        }
        else
        {
            GUILayout.Label(string.Format("Fetch1 Progress: {0}/{1}", "Not Started", DclMap.N));
        }

        GUILayout.EndArea();
    }

    public static Parcel[] Parcels = new Parcel[DclMap.N];
    public static List<Estate> Estates = new List<Estate>();
    public static List<District> Districts = new List<District>();
    public static List<ParcelPublicationAPI.PublicationHistory> PublicationHistories = new List<ParcelPublicationAPI.PublicationHistory>();

    private static MapAPI.Counter counter;
    private static ParcelPublicationAPI.Counter counter1;

    public const string API_URL = DclMap.API_URL + "/map";

    public static IEnumerator AsyncFetchWholeMapParcelsBySteps(MonoBehaviour holder)
    {
        const int step = 1;
        counter = new MapAPI.Counter();
        for (int x = -150; x <= 150; x += step)
        {
            holder.StartCoroutine(AsyncFetch(150, x, -150, Mathf.Min(150, x + step - 1), counter));
            yield return new WaitForSecondsRealtime(0.2f);
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

    public static IEnumerator AsyncFetch(int n, int w, int s, int e, MapAPI.Counter counter)
    {
        var url = string.Format(API_URL + "?nw={0},{1}&se={2},{3}", w, n, e, s);
        var www = new WWW(url);
        yield return www;
        var retryTimes = 0;
        while (www.error != null)
        {
            retryTimes++;
            if (retryTimes >= 10)
            {
                Debug.LogWarningFormat(www.error + " Stop retrying {0}. retryTimes: {1}", url, retryTimes);
                break;
            }
            else
            {
                Debug.LogWarningFormat(www.error + " Is retrying {0}. retryTimes: {1}", url, retryTimes);
            }
            www = new WWW(url);
            yield return www;
        }

        if (counter != null) counter.responseCount++;
        try
        {
            var mapResponse = JsonConvert.DeserializeObject<MapResponse>(www.text);

            for (int i = 0; i < mapResponse.data.assets.parcels.Count; i++)
            {
                var parcel = mapResponse.data.assets.parcels[i];
                var index = DclMap.CoordinatesToIndex(parcel.x, parcel.y);
                Parcels[index] = parcel;
            }

            foreach (var estate in mapResponse.data.assets.estates)
            {
                if (!Estates.Exists(est => est.id == estate.id))
                {
                    Estates.Add(estate);
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            Debug.LogWarning("113:\n" + string.Format(API_URL + "?nw={0},{1}&se={2},{3}", w, n, e, s));
            File.WriteAllText(@"D:\GitHub\Decentraland3DMap\UnityProject\Assets\Editor\JsonFiles\114error.txt",
                www.text);
        }
    }

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

            try
            {
                var response = JsonConvert.DeserializeObject<DistrictsResponse>(www.text);

                for (var i = 0; i < response.data.Count; i++)
                {
                    Districts.Add(response.data[i]);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogWarning("152:" + www.text);
                throw;
            }
        }
    }
    
    public class ParcelPublicationAPI
    {
        public const string API_URL = DclMap.API_URL + "/parcels/{0}/{1}/publications";

        public static IEnumerator AsyncFetchAll(MonoBehaviour holder)
        {
            counter1 = new ParcelPublicationAPI.Counter();
            var k = 0;
            for (int i = 0; i < DclMap.N; i++)//TODO：太可怕了
            {
                var coord = DclMap.IndexToCoordinates(i);
                if (Parcels[i].district_id == null)
                {
                    holder.StartCoroutine(AsyncFetch(coord.x, coord.y, counter1));
                    k++;
                    if (k >= 1)
                    {
                        yield return new WaitForSecondsRealtime(0.03f);
                        k = 0;
                    }
                }
                else
                {
                    counter1.responseCount++;
                }
            }

            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (counter1.responseCount >= DclMap.N)
                {
                    break;
                }
            }
        }

        public static IEnumerator AsyncFetch(int x, int y, ParcelPublicationAPI.Counter counter = null)
        {
            var url = string.Format(API_URL, x, y);
            var www = new WWW(url);
            yield return www;
            var retryTimes = 0;
            while (www.error != null)
            {
                retryTimes++;
                if (retryTimes >= 10)
                {
                    Debug.LogErrorFormat(www.error + " Stop retrying {0}. retryTimes: {1}", url, retryTimes);
                    break;
                }
                else
                {
                    Debug.LogWarningFormat(www.error + " Is retrying {0}. retryTimes: {1}", url, retryTimes);
                }
                yield return new WaitForSecondsRealtime(10);
                www = new WWW(url);
                yield return www;
            }

            var response = JsonConvert.DeserializeObject<ParcelPublicationsResponse>(www.text);
            
            var publicationHistory = new PublicationHistory
            {
                id = x+","+y,
                isEstate = false,
                publications = response.data
            };

            PublicationHistories.Add(publicationHistory);
            
            if (counter != null) counter.responseCount++;
        }

        public class Counter
        {
            public int responseCount;
        }
        public class PublicationHistory
        {
            public bool isEstate;
            public string id;
            public List<Publication> publications;
        }
    }
}