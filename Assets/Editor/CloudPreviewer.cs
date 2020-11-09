using UnityEngine;
using UnityEditor;
using EG = UnityEditor.EditorGUI;
using EGL = UnityEditor.EditorGUILayout;

public class CloudPreviewer : EditorWindow
{
    public static void ShowTexture(Texture3D texture)
    {
        CloudPreviewer w = EditorWindow.GetWindow<CloudPreviewer>("Cloud Preview", true, typeof(CloudEditor));
        w.texture = texture;
    }

    public Texture3D texture = null;
    bool[] _c = new bool[] {true, true, true, false};
    float zoom = 1f;
    float slice = 0f;
    Material previewMaterial = null;

    void Awake()
    {
        previewMaterial = new Material(Shader.Find("Hidden/EditorPreview"));
    }

    void OnGUI()
    {
        EGL.LabelField("Generated texture", EditorStyles.boldLabel);

        EGL.LabelField("Show channels:");
        EGL.BeginHorizontal();
        _c[3] = GUILayout.Toggle(_c[3], "A");
        EG.BeginDisabledGroup(_c[3]);
        _c[0] = GUILayout.Toggle(_c[0], "R");
        _c[1] = GUILayout.Toggle(_c[1], "G");
        _c[2] = GUILayout.Toggle(_c[2], "B");
        EG.EndDisabledGroup();
        EGL.EndHorizontal();

        EGL.LabelField("Zoom");
        zoom = EGL.Slider(zoom, 0.2f, 3f);

        EGL.LabelField("Slice");
        slice = EGL.Slider(slice, 0f, 1f);

        Rect r_tex = EGL.BeginVertical(GUILayout.ExpandHeight(true));
        r_tex.height = r_tex.width = Mathf.Min(r_tex.height, r_tex.width);
        if(texture)
        {
            previewMaterial.SetFloat("_Zoom", zoom);
            previewMaterial.SetFloat("_Slice", slice);
            previewMaterial.SetVector("_Channels", ChannelVector());
            EG.DrawPreviewTexture(r_tex, texture, previewMaterial);
        }
        EGL.EndVertical();
    }

    Vector4 ChannelVector()
    {
        return new Vector4(_c[0]?1f:0f, _c[1]?1f:0f, _c[2]?1f:0f, _c[3]?1f:0f);
    }
}
