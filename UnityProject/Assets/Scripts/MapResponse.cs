using System.Collections.Generic;

public class MapResponse
{
    public bool ok;
    public MapData data;
}
public class MapData
{
    public MapAssets assets;
    public int total;
}

public class MapAssets
{
    public List<Parcel> parcels;
    public List<Estate> estates;
}

public class Publication
{
    public string owner;

    public double price;

    public string status;

    public string asset_type;

    public string asset_id;
    /*
    "tx_hash":"0x3b6784bbf7b8a98d91213483b092e58f83cac5c62fe5a510c5c8f4b1bab31bff",
    "tx_status":"confirmed",
    "owner":"0xf902d068920234957d2908b8b0156e61c0bea2c2",
    "price":17585,
    "expires_at":1543532400000,
    "created_at":"2018-11-15T16:32:20.446",
    "updated_at":"2018-11-15T16:32:20.446",
    "status":"open",
    "buyer":null,
    "contract_id":"0xe5ad15facb00b77fb78c0771589399fdd180bf1eaaa323182fb45360a2145de4",
    "block_number":6643718,
    "block_time_created_at":1541360305000,
    "block_time_updated_at":null,
    "asset_type":"parcel",
    "asset_id":"55,-11",
    "marketplace_address":"0x8e5660b4ab70168b5a6feea0e0315cb49c8cd539"*/
}


public struct Coordinates
{
    public int x, y;

    public Coordinates(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override string ToString()
    {
        return string.Format("({0},{1})", x, y);
    }
}