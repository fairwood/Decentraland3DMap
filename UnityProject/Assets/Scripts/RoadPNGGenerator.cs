using UnityEngine;
using System.Collections;
using System.IO;

public class RoadPNGGenerator : MonoBehaviour
{
    private bool isFinished = false;

    IEnumerator Start()
    {
        yield return StartCoroutine(MapAPI.AsyncFetchWholeMapParcelsBySteps(this));
        isFinished = true;
        Debug.Log("Finish");
    }

    IEnumerator GeneratePNG()
    {
        while (true)
        {
            if (isFinished) break;
            yield return new WaitForEndOfFrame();
        }

        var texture = new Texture2D(301, 301);
        var colors = new Color[DclMap.N];
        for (int i = 0; i < DclMap.N; i++)
        {
            if (DclMap.ParcelInfos[i].Parcel.district_id == "f77140f9-c7b4-4787-89c9-9fa0e219b079")
            {
                colors[i] = Color.red;
            }
            else if (DclMap.ParcelInfos[i].Parcel.district_id != null)
            {
                colors[i] = Color.blue;
            }
            else
            {
                colors[i] = Color.black;
            }
        }

        texture.SetPixels(colors);
        var bytes = texture.EncodeToPNG();
        File.WriteAllBytes(Path.Combine(path, "dclmap_base.png"), bytes);
    }

    private string path;

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 200, 600));

        GUILayout.Label("Path");
        path = GUILayout.TextField(path);

        if (GUILayout.Button("Generate"))
        {
            StartCoroutine(GeneratePNG());
        }

        GUILayout.EndArea();
    }
}