using UnityEngine;

[System.Serializable]
public class CloudEditorData : ScriptableObject
{
    [HideInInspector]
    public int size;
    [HideInInspector]
    public float[] freqs;
    [HideInInspector]
    public int[] octaves;
    [HideInInspector]
    public int[] inverts;

    public CloudEditorData()
    {
        freqs = new float[2];
        octaves = new int[2];
        inverts = new int[2];
    }
}
