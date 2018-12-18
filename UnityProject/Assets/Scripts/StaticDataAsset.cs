using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StaticData", menuName = "ScriptableObjects/StaticDataAsset", order = 1)]
public class StaticDataAsset : ScriptableObject
{
    public Data.Parcel[] Parcels;

    public Data.District[] Districts;

    public List<Data.Estate> Estates;
}