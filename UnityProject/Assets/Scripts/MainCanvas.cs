﻿using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.UI;

public class MainCanvas : MonoBehaviour
{
    public Canvas Canvas;

    public GameObject HoverFrame;
    public Text TxtHover;
    
    public RightPanel RightPanel;

    void Awake()
    {
        Canvas = GetComponent<Canvas>();
       
    }

    void Update()
    {
        if (DclMap.HoveredParcelIndex != null)
        {
            var index = (int) DclMap.HoveredParcelIndex;
            HoverFrame.SetActive(true);
            var rectTra = HoverFrame.GetComponent<RectTransform>();
            rectTra.position = Input.mousePosition + new Vector3(50, 0);
            var parcelInfo = DclMap.ParcelInfos[index];
            var coord = DclMap.IndexToCoordinates(index);
            var sb = new StringBuilder();
            sb.AppendFormat("{0},{1}\n", coord.x, coord.y);
            var open = false;
            if (parcelInfo != null)
            {
                if (parcelInfo.EstateInfo != null)
                {
                    if (parcelInfo.EstateInfo.Estate.publication != null &&
                        parcelInfo.EstateInfo.Estate.publication.status == "open")
                    {
                        open = true;
                        var price = parcelInfo.EstateInfo.Estate.publication.price;
                        var parcelCount = parcelInfo.EstateInfo.Estate.data.parcels.Count;
                        var unitPrice = price / parcelCount;
                        sb.AppendFormat("{0}\n", parcelInfo.EstateInfo.Estate.data.name);
                        sb.AppendFormat("({0}) @ {1}\n{2} MANA", parcelCount, unitPrice, price);
                    }
                }
                else if (parcelInfo.Parcel != null)
                {
                    string name;
                    if (parcelInfo.Parcel.district_id != null)
                    {
                        var district = DclMap.Districts.Find(d => d.id == parcelInfo.Parcel.district_id);
                        if (district != null)
                        {
                            name = district.name;
                        }
                        else
                        {
                            name = "waiting...";
                        }
                    }
                    else
                    {
                        name = parcelInfo.Parcel.data.name;
                    }
                    sb.AppendFormat("{0}\n", name);
                    if (parcelInfo.Parcel.publication != null && parcelInfo.Parcel.publication.status == "open")
                    {
                        open = true;
                        var price = parcelInfo.Parcel.publication.price;
                        sb.AppendFormat("{0} MANA", price);
                    }
                }
                var dealPrice = DclMap.GetLastDealPrice(index);
                if (dealPrice >= 0)
                {
                    sb.AppendFormat("\nLast deal @ {0} MANA", dealPrice);
                }
            }

            TxtHover.text = sb.ToString();
        }
        else
        {
            HoverFrame.SetActive(false);
        }
    }
}