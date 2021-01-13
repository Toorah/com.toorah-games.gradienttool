using System;
using System.Collections;
using System.Collections.Generic;
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

        private void OnEnable()
        {
            m_asset = target as GradientAsset;

            if (!m_drawingTexture)
                m_drawingTexture = new Texture2D(2, 2);
            if (!m_gradientShader)
                m_gradientShader = Shader.Find("Hidden/GradientEditor");
            if (!m_gradientMaterial)
            {
                m_gradientMaterial = new Material(m_gradientShader);
                m_gradientMaterial.SetColorArray("_Colors", new Color[100]);
                m_gradientMaterial.SetFloatArray("_Positions", new float[100]);
            }
            if (!m_gradientTexture)
                m_gradientTexture = new RenderTexture(2048, 2, 32);

            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }
        private void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        private void Update()
        {
            if (m_gradientMaterial && m_points.Count > 0)
            {
                m_asset.keys = m_points.OrderBy(x => x.position).ToList();

                Color[] colors = new Color[m_points.Count];
                float[] positions = new float[m_points.Count];

                for (int i = 0; i < m_points.Count; i++)
                {
                    colors[i] = m_points[i].color;
                    positions[i] = m_points[i].position;
                }

                m_gradientMaterial.SetColorArray("_Colors", colors);
                m_gradientMaterial.SetFloatArray("_Positions", positions);
                m_gradientMaterial.SetFloat("_Count", m_points.Count);
                Repaint();

                using (var scope = new DrawingScope(m_gradientTexture))
                {
                    scope.Draw(new Rect(0, 0, 1, 1), m_drawingTexture, m_gradientMaterial);
                }
            }
        }

        float testPos;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.Space(10);

            Rect rect = GUILayoutUtility.GetRect(200, 30);
            rect.x += 20;
            rect.width -= 40;
            EditorGUI.DrawTextureTransparent(rect, m_gradientTexture, ScaleMode.StretchToFill);


            Handles.BeginGUI();
            for (int i = 0; i < m_points.Count; i++)
            {
                var p = m_points[i];
                bool isSelected = m_selectedPoint == p;
                Color outline = isSelected ? Color.yellow : Color.grey;
                Rect r = new Rect(rect.x + rect.width * p.position - 2, rect.y - 2, 4, rect.height + 4);
                Handles.DrawSolidRectangleWithOutline(r, Color.black * 0.2f, outline);

                bool isMouseOver = r.Contains(Event.current.mousePosition);

                if (Event.current.type == EventType.MouseDown && isMouseOver)
                {
                    m_selectedPoint = p;
                    m_dragging = p;
                }
                if (Event.current.type == EventType.MouseUp && m_dragging != null)
                {
                    m_dragging = null;
                }
            }
            Handles.EndGUI();

            if (m_dragging != null && Event.current.type == EventType.MouseDrag)
            {
                var mousePos = Event.current.mousePosition;
                var p = Mathf.Clamp((mousePos.x - rect.x) / rect.width, 0, 1);
                m_dragging.position = p;
                Event.current.Use();
            }

            using(new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Add"))
                {
                    m_asset.keys.Add(new GradientKey(Color.white, 0.5f));
                    EditorUtility.SetDirty(m_asset);
                }
                if(m_selectedPoint != null && m_points.Count > 1){
                    if (GUILayout.Button("Remove"))
                    {
                        m_asset.keys.Remove(m_selectedPoint);
                        m_selectedPoint = null;
                        EditorUtility.SetDirty(m_asset);
                    }
                }
            }


            GUILayout.Space(10);
            if (m_selectedPoint != null)
            {
                var p = m_selectedPoint;
                p.color = EditorGUILayout.ColorField(p.color);
                p.position = EditorGUILayout.Slider(p.position, 0, 1);
            }


            GUILayout.Space(10);
            GUILayout.Label("Debug");
            testPos = EditorGUILayout.Slider(testPos, 0, 1);
            var col = m_asset.Evaluate(testPos);
            Rect r2 = GUILayoutUtility.GetRect(10, 10);
            EditorGUILayout.ColorField(col);


            serializedObject.ApplyModifiedProperties();
        }
    }
}
