using System.Collections.Generic;
using System.IO;
using System.Linq;
using Toorah.Drawing;
using Toorah.Gradients;
using UnityEditor;
using UnityEngine;

namespace Toorah.GradientEditor
{
    [CustomEditor(typeof(GradientAsset))]
    public class GradientAssetEditor : Editor
    {
        GradientAsset m_asset;

        [SerializeField] Texture2D m_drawingTexture;
        [SerializeField] RenderTexture m_gradientTexture;
        [SerializeField] Shader m_gradientShader;
        [SerializeField] Material m_gradientMaterial;

        [SerializeField] List<GradientKey> m_points => m_asset.keys;
        [SerializeField] GradientKey m_selectedPoint;
        [SerializeField] GradientKey m_dragging;
        [SerializeField] GradientAsset.Mode Mode { get => m_asset.mode; set => m_asset.mode = value; }

        bool m_copyGradient;
        Gradient m_testGradient = new Gradient();

        Color[] m_colors = new Color[100];
        float[] m_positions = new float[100];

        bool m_showDebug;

        private void OnEnable()
        {
            m_asset = target as GradientAsset;

            if (!m_drawingTexture)
                m_drawingTexture = new Texture2D(2, 2);
            if (!m_gradientShader)
                m_gradientShader = Shader.Find("Hidden/GradientEditor");
            if (!m_gradientMaterial)
            {
                m_colors = new Color[100];
                m_positions = new float[100];

                m_gradientMaterial = new Material(m_gradientShader);
                m_gradientMaterial.SetColorArray("_Colors", m_colors);
                m_gradientMaterial.SetFloatArray("_Positions", m_positions);
            }
            if (!m_gradientTexture)
                m_gradientTexture = new RenderTexture(2048, 4, 32);

            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }
        private void OnDisable()
        {
            EditorApplication.update -= Update;
            EditorApplication.update -= PlayDebug;
        }

        private void Update()
        {
            if (m_gradientMaterial && m_points.Count > 0)
            {
                m_asset.keys = m_points.OrderBy(x => x.position).ToList();

                for (int i = 0; i < m_points.Count; i++)
                {
                    m_colors[i] = m_points[i].color;
                    m_positions[i] = m_points[i].position;
                }
                m_gradientMaterial.SetInt("_Mode", (int)Mode);
                m_gradientMaterial.SetColorArray("_Colors", m_colors);
                m_gradientMaterial.SetFloatArray("_Positions", m_positions);
                m_gradientMaterial.SetFloat("_Count", m_points.Count);
                Repaint();

                using (var scope = new DrawingScope(m_gradientTexture))
                {
                    scope.Draw(new Rect(0, 0, 1, 1), m_drawingTexture, m_gradientMaterial);
                }
            }
        }

        float m_testPos;
        bool m_play;
        double m_time;
        GUIStyle m_boxStyle;
        void GenerateStyles()
        {
            if(m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(EditorStyles.helpBox);
                m_boxStyle.padding = new RectOffset(5, 5, 5, 10);
            }
        }

        void DrawColorKeys(Rect rect)
        {
            Handles.BeginGUI();
            for (int i = 0; i < m_points.Count; i++)
            {
                DrawColorKey(rect, m_points[i]);
            }
            Handles.EndGUI();
        }
        void DrawColorKey(Rect rect, GradientKey key)
        {
            bool isSelected = m_selectedPoint == key;

            Color outline = isSelected ? Color.yellow : Color.grey;
            Rect r = new Rect(rect.x + rect.width * key.position - 2, rect.y - 2, 4, rect.height + 4);
            Handles.DrawSolidRectangleWithOutline(r, Color.black * 0.2f, outline);

            bool isMouseOver = r.Contains(Event.current.mousePosition);

            if (Event.current.type == EventType.MouseDown && isMouseOver)
            {
                m_selectedPoint = key;
                m_dragging = key;
            }
            if (Event.current.type == EventType.MouseUp && m_dragging != null)
            {
                m_dragging = null;
            }
        }

        void DrawGradientTexture(out Rect rect)
        {
            rect = GUILayoutUtility.GetRect(200, 30);
            rect.x += 20;
            rect.width -= 40;
            EditorGUI.DrawTextureTransparent(rect, m_gradientTexture, ScaleMode.StretchToFill);
        }

        void HandleDrag(Rect rect)
        {
            if (m_dragging != null && Event.current.type == EventType.MouseDrag)
            {
                var mousePos = Event.current.mousePosition;
                var p = Mathf.Clamp((mousePos.x - rect.x) / rect.width, 0, 1);
                m_dragging.position = p;
                Event.current.Use();
            }
        }

        void AddRemoveButtons()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (m_points.Count < 100)
                {
                    if (GUILayout.Button(new GUIContent("+", "Add Key")))
                    {
                        m_asset.keys.Add(new GradientKey(Color.white, 0.5f));
                        EditorUtility.SetDirty(m_asset);
                    }
                }
                if (m_selectedPoint != null && m_points.Count > 1)
                {
                    if (GUILayout.Button(new GUIContent("-", $"Remove Key [{m_points.IndexOf(m_selectedPoint)}]")))
                    {
                        m_asset.keys.Remove(m_selectedPoint);
                        m_selectedPoint = null;
                        EditorUtility.SetDirty(m_asset);
                    }
                }
                GUILayout.Space(20);
            }
        }
        void DrawSelectedPointBox()
        {
            if(m_points == null || m_points.Count == 0)
            {
                EditorGUILayout.HelpBox("There are no color keys added", MessageType.Warning, true);
            }
            else if (m_points.IndexOf(m_selectedPoint) != -1)
            {
                using (new GUILayout.VerticalScope(m_boxStyle))
                {
                    DrawPointGUI(m_selectedPoint);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Select a color key from above to edit its values.", MessageType.Info, true);
            }
        }
        void DrawPointGUI(GradientKey key)
        {
            GUILayout.Label($"Point [{m_points.IndexOf(key)}]");
            key.color = EditorGUILayout.ColorField(key.color);
            key.position = EditorGUILayout.Slider(key.position, 0, 1);
        }
        void ExportButton()
        {
            if (GUILayout.Button("Export"))
            {
                var path = EditorUtility.SaveFilePanelInProject("Gradient to Texture", "Gradient", "png", "");
                if (path != "")
                {
                    Texture2D tex = new Texture2D(2048, 4);
                    RenderTexture.active = m_gradientTexture;
                    tex.ReadPixels(new Rect(0, 0, m_gradientTexture.width, m_gradientTexture.height), 0, 0);
                    tex.Apply();

                    byte[] bytes = tex.EncodeToPNG();
                    File.WriteAllBytes(path, bytes);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
        }
        void DrawDebug()
        {
            using (new GUILayout.VerticalScope(m_boxStyle))
            {
                EditorGUI.indentLevel++;
                m_showDebug = EditorGUILayout.Foldout(m_showDebug, "Debug", true);
                if (m_showDebug)
                {
                    m_testPos = EditorGUILayout.Slider(m_testPos, 0, 1);
                    var col = m_asset.Evaluate(m_testPos);
                    EditorGUILayout.ColorField(col);
                    using(new EditorGUI.DisabledGroupScope(m_play))
                    {
                        if (GUILayout.Button("Play"))
                        {
                            m_play = true;
                            m_testPos = 0;
                            m_time = EditorApplication.timeSinceStartup;
                            EditorApplication.update += PlayDebug;
                        }
                    }

                    m_copyGradient = EditorGUILayout.Foldout(m_copyGradient, "Copy Gradient", true);
                    if (m_copyGradient)
                    {
                        m_testGradient = EditorGUILayout.GradientField(m_testGradient);
                        if(GUILayout.Button("Copy Cradient"))
                        {
                            serializedObject.Update();
                            m_points.Clear();
                            var colors = m_testGradient.colorKeys;
                            for(int i = 0; i < colors.Length; i++)
                            {
                                colors[i].color.a = m_testGradient.Evaluate(colors[i].time).a;
                                var p = new GradientKey(colors[i].color, colors[i].time);
                                m_points.Add(p);
                            }
                            EditorUtility.SetDirty(m_asset);
                            serializedObject.ApplyModifiedProperties();
                            m_copyGradient = false;
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        private void PlayDebug()
        {
            m_testPos = (float)(EditorApplication.timeSinceStartup - m_time)/5f;
            Repaint();
            if(m_testPos >= 1)
            {
                m_play = false;
                m_testPos = 1;
                EditorApplication.update -= PlayDebug;
            }
        }

        public override void OnInspectorGUI()
        {
            GenerateStyles();


            serializedObject.Update();

            GUILayout.Space(10);

            DrawGradientTexture(out var rect);
            DrawColorKeys(rect);

            HandleDrag(rect);

            AddRemoveButtons();

            Mode = (GradientAsset.Mode)EditorGUILayout.EnumPopup("Mode", Mode);

            GUILayout.Space(10);
            DrawSelectedPointBox();
            GUILayout.Space(10);
            ExportButton();

            GUILayout.Space(10);

            DrawDebug();


            serializedObject.ApplyModifiedProperties();
        }
    }
}
