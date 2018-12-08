using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class RightPanel : MonoBehaviour
{
    public ToggleGroup TgrDataToVisualize;

    public GameObject GrpDealPriceHint;

    public Toggle TglOnlyRoadside;

    public Text TxtFormula;

    public Text TxtCoordinates;
    public Text TxtDealHistory;

    public Coordinates SelectedParcel;

    void Start()
    {
        DclMap.Instance.OnParcelCubeClick += DidParcelCubeClick;
        GrpDealPriceHint.SetActive(false);
        DidParcelCubeClick(DclMap.CoordinatesToIndex(0, 0));
    }

    public void OnTglVisualizeAskingPriceChange(bool isOn)
    {
        if (isOn)
        {
            DclMap.DataToVisualize = DclMap.EDataToVisualize.AskingPrice;
            TxtFormula.text = "∝1/(price^3)";
            GrpDealPriceHint.SetActive(false);
        }
    }
    public void OnTglVisualizeDealPriceChange(bool isOn)
    {
        if (isOn)
        {
            DclMap.DataToVisualize = DclMap.EDataToVisualize.LastDealPrice;
            TxtFormula.text = "∝price";
            GrpDealPriceHint.SetActive(true);
        }
    }

    public void OnFilterOnlyRoadsideChange(bool isOn)
    {
        DclMap.FilterOnlyRoadside = isOn;
    }

    public void OnSeeDetailClick()
    {
        Application.OpenURL(string.Format("https://market.decentraland.org/parcels/{0}/{1}/detail", SelectedParcel.x, SelectedParcel.y));
    }

    public void OpenGitHub()
    {
        Application.OpenURL("https://github.com/fairwood/Decentraland3DMap");
    }

    void DidParcelCubeClick(int index)
    {
        var coord = DclMap.IndexToCoordinates(index);
        SelectedParcel = coord;
        TxtCoordinates.text = string.Format("{0},{1}", coord.x, coord.y);

        var sb = new StringBuilder();
        var parcelInfo = DclMap.ParcelInfos[index];
        //Sold
        for (var i = 0; i < parcelInfo.SoldPublications.Count; i++)
        {
            var publication = parcelInfo.SoldPublications[i];
            long timestamp;
            var succ = long.TryParse(publication.block_time_updated_at, out timestamp);
            if (succ)
            {
                var time = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddMilliseconds(timestamp);
                sb.AppendLine(time.ToString("g"));
            }
            else
            {
                sb.AppendLine("ERROR " + timestamp);
            }
            sb.AppendFormat("{0:0} MANA\n", publication.price);
            sb.Append("—\n");
        }
        //Auction
        {
            sb.AppendLine("Auction");
            if (parcelInfo.Parcel == null)
            {
                sb.AppendLine("waiting...");
            }
            else
            {
                if (parcelInfo.Parcel.auction_price == null)
                {
                    sb.AppendLine("Not Owned");
                }
                else
                {
                    sb.AppendFormat("{0:0} MANA\n", parcelInfo.Parcel.auction_price);
                }
            }
        }
        sb.Append("— END —\n");

        TxtDealHistory.text = sb.ToString();
    }
}
