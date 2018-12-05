using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class RightPanel : MonoBehaviour
{

    public Text TxtCoordinates;
    public Text TxtDealHistory;

    void Start()
    {
        DclMap.Instance.OnParcelCubeClick += DidParcelCubeClick;
        
    }

    void DidParcelCubeClick(int index)
    {
        var coord = DclMap.IndexToCoordinates(index);
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
                sb.AppendLine(time.ToString());
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
