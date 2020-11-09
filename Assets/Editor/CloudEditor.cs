using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using EG = UnityEditor.EditorGUI;
using EGL = UnityEditor.EditorGUILayout;

public class CloudEditor : EditorWindow
{
    [MenuItem("Window/Cloud Editor")]
    public static void Create()
    {
        EditorWindow.GetWindow<CloudEditor>("Cloud Editor");
    }

    [SerializeField]
    CloudEditorData data;
    ComputeShader cs;
    static readonly string dataPath = "Assets/Editor/CloudEditor.asset";
    static readonly string csPath = "Assets/Editor/CloudGenerator.compute";

    Texture3D texture = null;
    TextureFormat format = TextureFormat.R16;
    FilterMode filter = FilterMode.Bilinear;
    TextureWrapMode wrap = TextureWrapMode.Repeat;

    string status = "";
    string assetName = "";

    void Awake()
    {
        data = AssetDatabase.LoadAssetAtPath<CloudEditorData>(dataPath);
        if(data == null)
        {
            data = ScriptableObject.CreateInstance<CloudEditorData>();
            AssetDatabase.CreateAsset(data, dataPath);
        }
        cs = AssetDatabase.LoadAssetAtPath<ComputeShader>(csPath);
    }

    void OnDestroy()
    {
        AssetDatabase.ForceReserializeAssets(new string[]{dataPath});
    }

    void OnGUI()
    {
        EGL.LabelField("3D Texture properties", EditorStyles.boldLabel);

        int new_size = EGL.IntField("Resolution", data.size);
        new_size = new_size <= 512 ? new_size : 512;
        new_size = new_size >= 8 ? new_size : 8;
        data.size = new_size;

        EGL.LabelField("Worley noise");
        data.freqs[0] = EGL.FloatField("Frequency", data.freqs[0]);
        data.octaves[0] = EGL.IntField("Octaves", data.octaves[0]);
        bool inv = EGL.Toggle("Invert", data.inverts[0] > 0);
        data.inverts[0] = inv ? 1 : 0;

        if(GUILayout.Button("Generate"))
        {
            CreateTexture();
        }

        EGL.HelpBox(status, MessageType.None, true);

        if(GUILayout.Button("Preview"))
        {
            CloudPreviewer.ShowTexture(texture);
        }

        EGL.BeginHorizontal();
        assetName = EGL.TextField("Asset name:", assetName);
        if(GUILayout.Button("Save"))
        {
            AssetDatabase.CreateAsset(texture, "Assets/"+assetName+".asset");
        }
        EGL.EndHorizontal();
    }

    void CreateTexture()
    {
        System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();
        texture = new Texture3D(data.size, data.size, data.size, format, false);
        texture.filterMode = filter;
        texture.wrapMode = wrap;

        int totalPixels = data.size * data.size * data.size;
        //float[] pixels = new float[totalPixels];
        Color[] pixels = new Color[totalPixels];
        ComputeBuffer cb = new ComputeBuffer(totalPixels, sizeof(float) * 4);
        cs.SetBuffer(0, "Result", cb);
        cs.SetInt("Size", data.size);
        cs.SetInts("Octaves", data.octaves);
        cs.SetInts("Inverts", data.inverts);
        cs.SetFloats("Freqs", data.freqs);
        cs.Dispatch(0, data.size/8, data.size/8, data.size/8);
        cb.GetData(pixels);
        cb.Release();

        //texture.SetPixelData<float>(pixels, 0);
        texture.SetPixels(pixels);
        texture.Apply();
        pixels = null;
        status = "Completed in: " + timer.Elapsed.ToString();
        timer.Stop();
        CloudPreviewer.ShowTexture(texture);
    }

    /*void FillPixels()
    {
        Parallel.For(0, data.size, z => {
            float s = 1f/data.size;
            float[] col = new float[4];
            int zOffset = z * data.size * data.size;
            for(int y = 0; y < data.size; y++)
            {
                int yOffset = y * data.size;
                for(int x = 0; x < data.size; x++)
                {
                    for(int i = 0; i < 4; i++)
                    {
                        col[i] = NoiseLib.WorleyFbm(s*x, s*y, s*z, data.chan[i].freq, data.chan[i].octaves);
                        if(data.chan[i].invert) col[i] = 1f - col[i];
                    }
                    pixels[x + yOffset + zOffset] = new Color(col[0], col[1], col[2], col[3]);
                }
                Interlocked.Add(ref donePixels, data.size);
            }
        });
    }*/
}
