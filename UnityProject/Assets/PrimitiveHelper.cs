
using UnityEngine;

public static class PrimitiveHelper
{
    private static Mesh _cube;
    public static Mesh Cube
    {
        get
        {
            if (!_cube)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var meshF = go.GetComponent<MeshFilter>();
                _cube = meshF.sharedMesh;
                Object.Destroy(go);
            }

            return _cube;
        }
    }
}