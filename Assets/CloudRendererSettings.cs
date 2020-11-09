using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CloudRenderer/CloudRendererSettings")]
public class CloudRendererSettings : ScriptableObject
{
    public ComputeShader shader;
    public Shader blendShader;
    [Range(0, 3)]
    public int kernel;
    [Range(1, 4)]
    public int downsample;
    public Texture3D cloudVolume;
    [Header("Blue noise")]
    public Texture2D[] blueNoise;
    [Header("Beer's law")]
    [Range(1f, 20f)]
    public float alpha;
    [Header("Henyey-Greenstein scattering")]
    [Range(-1f, 1f)]
    public float g0;
    [Range(-1f, 1f)]
    public float g1;
    [Range(0f, 1f)]
    public float hgLerp;
    [Header("Cloud placement")]
    public float topY;
    public float thinY;
    public float bottomY;
}
