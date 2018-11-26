using System.Collections.Generic;
using UnityEngine;

public class ParcelInfo
{
    public Parcel Parcel;

    public EstateInfo EstateInfo;

    public float LastFetchPublicationsTime = float.NegativeInfinity;
    public readonly List<Publication> SoldPublications = new List<Publication>();

    public int GetDistanceToRoad()
    {
        if (Parcel != null)
        {
            if (Parcel.tags != null && Parcel.tags.proximity != null && Parcel.tags.proximity.road != null)
            {
                return Parcel.tags.proximity.road.distance;
            }
        }

        return -1;
    }
}

public class DistrictInfo
{
    public District District;
}

public class EstateInfo
{
    private EstateInfo()
    {
        
    }

    public EstateInfo(Estate estate)
    {
        Estate = estate;
        Color = new Color(Random.value, Random.value, Random.value, 1);
    }

    public Estate Estate;

    public Color Color;

    public void Update(Estate estate)
    {
        Estate = estate;
    }
}