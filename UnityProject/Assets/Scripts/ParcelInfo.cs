using System.Collections.Generic;
using UnityEngine;

public class ParcelInfo
{
    public int Index;

    public int x, y;
    
    public Parcel Parcel;

    public EstateInfo EstateInfo;

    public float LastFetchPublicationsTime = float.NegativeInfinity;
    public readonly List<Publication> SoldPublications = new List<Publication>();

    private ParcelInfo()
    {
        
    }
    public ParcelInfo(int index)
    {
        Index = index;
        var coord = DclMap.IndexToCoordinates(index);
        x = coord.x;
        y = coord.y;
    }

//    public int GetDistanceToRoad()
//    {
//        if (Parcel != null)
//        {
//            if (Parcel.tags != null && Parcel.tags.proximity != null && Parcel.tags.proximity.road != null)
//            {
//                return Parcel.tags.proximity.road.distance;
//            }
//        }
//
//        return -1;
//    }
    public void Update(Parcel parcel)
    {
        Parcel = parcel;
    }
    public void Update(EstateInfo estateInfo)
    {
        EstateInfo = estateInfo;
    }

    public bool IsRoadside()
    {
        var list = new List<Coordinates>();
        if (x > -150) list.Add(new Coordinates(x - 1, y));
        if (y < 150) list.Add(new Coordinates(x, y + 1));
        if (x < 150) list.Add(new Coordinates(x + 1, y));
        if (y > -150) list.Add(new Coordinates(x, y - 1));
        foreach (var coord in list)
        {
            var ind = DclMap.CoordinatesToIndex(coord);
            if (DclMap.IsRoad[ind]) return true;
        }

        return false;
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