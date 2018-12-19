using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class Parcel
    {
        public long timestamp;
    }
    [Serializable]
    public class Estate
    {
        public long timestamp;

    }
    [Serializable]
    public class District
    {
        public long timestamp;

    }

    [Serializable]
    public class PublicationHistory
    {
        public long timestamp;
        public bool isEstate;
        public string id;
        public List<Publication> publications;
    }
}