using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    #region 硬盘数据区

    private static string dataPath;

    #endregion

    #region 内存数据区

    public static Parcel[] Parcels = new Parcel[DclMap.N];
    public static List<Estate> Estates = new List<Estate>();
    public static List<District> Districts = new List<District>();
    public static List<PublicationHistory> PublicationHistories = new List<PublicationHistory>();

    #endregion

    void Awake()
    {
        Instance = this;
        dataPath = Application.dataPath + "/HDData/";
    }

    void Start()
    {
        //启动时，将硬盘数据读取到内存，如果硬盘有缺失，则去固定数据找

        if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath); //创建HDData

        #region 转录Parcels
        
        Parcel[] parcelsFromHDData = null;
        Parcel[] parcelsFromStaticData = null;
        try
        {
            if (File.Exists(dataPath + "parcels.txt"))
            {
                var parcelsJson = File.ReadAllText(dataPath + "parcels.txt");

                parcelsFromHDData = JsonConvert.DeserializeObject<Parcel[]>(parcelsJson);

                if (parcelsFromHDData.Length != DclMap.N)
                {
                    Debug.LogError("parcelsFromHDData.Length = " + parcelsFromHDData.Length + " != DclMap.N = 90601!");
                    parcelsFromHDData = null;
                }

                Debug.LogWarning("LHD:" + parcelsFromHDData.Length);
            }
            else
            {
                Debug.Log("Cannot find parcels.txt in " + dataPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        try
        {
            var parcelsTxtAss = Resources.Load<TextAsset>("parcels");
            if (parcelsTxtAss)
            {
                parcelsFromStaticData = JsonConvert.DeserializeObject<Parcel[]>(parcelsTxtAss.text);
                Debug.LogWarning("LSta:" + parcelsFromStaticData.Length);
            }
            else
            {
                Debug.LogError("Cannot find parcels.txt in Resources");
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }


        for (int i = 0; i < DclMap.N; i++)
        {
            if (parcelsFromHDData != null && parcelsFromHDData[i] != null)
            {
                Parcels[i] = parcelsFromHDData[i];
            }
            else if (parcelsFromStaticData != null)
            {
                Parcels[i] = parcelsFromStaticData[i];
            }
        }

        #endregion

        #region 转录Estates
        
        Estate[] estatesFromHDData = null;
        Estate[] estatesFromStaticData = null;
        try
        {
            if (File.Exists(dataPath + "estates.txt"))
            {
                var estatesJson = File.ReadAllText(dataPath + "estates.txt");

                estatesFromHDData = JsonConvert.DeserializeObject<Estate[]>(estatesJson);
                
                Debug.LogWarning("estatesFromHDData.l:" + estatesFromHDData.Length);
            }
            else
            {
                Debug.Log("Cannot find estates.txt in " + dataPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        try
        {
            var estatesTxtAss = Resources.Load<TextAsset>("estates");
            if (estatesTxtAss)
            {
                estatesFromStaticData = JsonConvert.DeserializeObject<Estate[]>(estatesTxtAss.text);
                Debug.LogWarning("estatesFromStaticData.l:" + estatesFromStaticData.Length);
            }
            else
            {
                Debug.LogError("Cannot find estates.txt in Resources");
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }


        for (int i = 0; i < DclMap.N; i++)
        {
            if (estatesFromHDData != null && estatesFromHDData[i] != null)
            {
                Estates[i] = estatesFromHDData[i];
            }
            else if (estatesFromStaticData != null)
            {
                Estates[i] = estatesFromStaticData[i];
            }
        }

        #endregion

        #region 转录Districts

        District[] districtsFromHDData = null;
        District[] districtsFromStaticData = null;
        try
        {
            if (File.Exists(dataPath + "districts.txt"))
            {
                var districtsJson = File.ReadAllText(dataPath + "districts.txt");

                districtsFromHDData = JsonConvert.DeserializeObject<District[]>(districtsJson);

                Debug.LogWarning("districtsFromHDData.l:" + districtsFromHDData.Length);
            }
            else
            {
                Debug.Log("Cannot find districts.txt in " + dataPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        try
        {
            var districtsTxtAss = Resources.Load<TextAsset>("districts");
            if (districtsTxtAss)
            {
                districtsFromStaticData = JsonConvert.DeserializeObject<District[]>(districtsTxtAss.text);
                Debug.LogWarning("districtsFromStaticData.l:" + districtsFromStaticData.Length);
            }
            else
            {
                Debug.LogError("Cannot find districts.txt in Resources");
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }


        for (int i = 0; i < DclMap.N; i++)
        {
            if (districtsFromHDData != null && districtsFromHDData[i] != null)
            {
                Districts[i] = districtsFromHDData[i];
            }
            else if (districtsFromStaticData != null)
            {
                Districts[i] = districtsFromStaticData[i];
            }
        }

        #endregion


        #region 转录PublicationHistories

        //TODO:

        #endregion

    }



}
public class PublicationHistory
{
    public bool isEstate;
    public string id;
    public List<Publication> publications;
}