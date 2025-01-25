// Copyright (c) AstralShift. All rights reserved.

using System.Collections.Generic;
using UnityEditor;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace AstralShift.QTI.Helpers
{
    public static class GizmosHelpers
    {
#if UNITY_EDITOR

        private static Mesh _cylinderPrimitive;

        public static void DrawWireCapsule(Vector3 _pos, Vector3 _pos2, float _radius, Color _color = default,
            float thickness = 1)
        {
            if (_color != default) Handles.color = _color;

            var forward = _pos2 - _pos;
            var _rot = Quaternion.LookRotation(forward);
            var pointOffset = _radius / 2f;
            var length = forward.magnitude;
            var center2 = new Vector3(0f, 0, length);

            Matrix4x4 angleMatrix = Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale);

            using (new Handles.DrawingScope(angleMatrix))
            {
                Handles.DrawWireDisc(Vector3.zero, Vector3.forward, _radius, thickness);
                Handles.DrawWireArc(Vector3.zero, Vector3.up, Vector3.left * pointOffset, -180f, _radius, thickness);
                Handles.DrawWireArc(Vector3.zero, Vector3.left, Vector3.down * pointOffset, -180f, _radius, thickness);
                Handles.DrawWireDisc(center2, Vector3.forward, _radius, thickness);
                Handles.DrawWireArc(center2, Vector3.up, Vector3.right * pointOffset, -180f, _radius, thickness);
                Handles.DrawWireArc(center2, Vector3.left, Vector3.up * pointOffset, -180f, _radius, thickness);

                DrawLine(_radius, 0f, length, thickness);
                DrawLine(-_radius, 0f, length, thickness);
                DrawLine(0f, _radius, length, thickness);
                DrawLine(0f, -_radius, length, thickness);
            }
        }

        public static void DrawWireCapsule(Vector3 center, Vector3 direction, float height, float radius,
            Color color = default, float thickness = 1)
        {
            if (color != default) Handles.color = color;

            // Normalize the direction and calculate half-length based on totalLength
            Vector3 forward = direction.normalized;
            float halfLength = (height - radius * 2f) / 2f; // Account for caps at both ends

            // Calculate the positions of the two endpoints
            Vector3 bottomPos = center - forward * halfLength;

            // Calculate the rotation based on the direction
            Quaternion rotation = Quaternion.LookRotation(forward);

            // Set up the transformation matrix
            Matrix4x4 angleMatrix = Matrix4x4.TRS(bottomPos, rotation, Handles.matrix.lossyScale);

            // Draw the capsule using the drawing scope
            using (new Handles.DrawingScope(angleMatrix))
            {
                // Define the positions in local space
                Vector3 bottomCapCenter = Vector3.zero;
                Vector3 topCapCenter = new Vector3(0f, 0f, halfLength * 2f);

                // Draw the bottom cap
                Handles.DrawWireDisc(bottomCapCenter, Vector3.forward, radius, thickness);

                // Draw the connecting arcs for the bottom cap
                Handles.DrawWireArc(bottomCapCenter, Vector3.up, Vector3.left * radius, -180f, radius, thickness);
                Handles.DrawWireArc(bottomCapCenter, Vector3.left, Vector3.down * radius, -180f, radius, thickness);

                // Draw the top cap
                Handles.DrawWireDisc(topCapCenter, Vector3.forward, radius, thickness);

                // Draw the connecting arcs for the top cap
                Handles.DrawWireArc(topCapCenter, Vector3.up, Vector3.right * radius, -180f, radius, thickness);
                Handles.DrawWireArc(topCapCenter, Vector3.left, Vector3.up * radius, -180f, radius, thickness);

                // Draw the lines connecting the top and bottom caps
                Handles.DrawLine(new Vector3(radius, 0f, 0f), new Vector3(radius, 0f, halfLength * 2f), thickness);
                Handles.DrawLine(new Vector3(-radius, 0f, 0f), new Vector3(-radius, 0f, halfLength * 2f), thickness);
                Handles.DrawLine(new Vector3(0f, radius, 0f), new Vector3(0f, radius, halfLength * 2f), thickness);
                Handles.DrawLine(new Vector3(0f, -radius, 0f), new Vector3(0f, -radius, halfLength * 2f), thickness);
            }
        }

        public static void DrawCapsule2D(Vector2 center, float height, float radius, CapsuleDirection2D direction)
        {
            if (direction == CapsuleDirection2D.Vertical)
            {
                height = Mathf.Max(height, 2 * radius);
                Vector2 bottomCapCenter = center - Vector2.up * (height / 2 - radius);
                Vector2 topCapCenter = center + Vector2.up * (height / 2 - radius);
                Vector3[] centerRectangle = new Vector3[4];
                centerRectangle[0] = new Vector3(topCapCenter.x - radius, topCapCenter.y, 0);
                centerRectangle[1] = new Vector3(bottomCapCenter.x - radius, bottomCapCenter.y, 0);
                centerRectangle[2] = new Vector3(bottomCapCenter.x + radius, bottomCapCenter.y, 0);
                centerRectangle[3] = new Vector3(topCapCenter.x + radius, topCapCenter.y, 0);
                Handles.DrawSolidArc(bottomCapCenter, Vector3.forward, Vector3.left, 180, radius);
                Handles.DrawSolidRectangleWithOutline(centerRectangle, Color.white, Color.clear);
                Handles.DrawSolidArc(topCapCenter, Vector3.forward, Vector3.right, 180, radius);
            }
            else
            {
                height = Mathf.Max(height, 2 * radius);
                Vector2 leftCapCenter = center - Vector2.right * (height / 2 - radius);
                Vector2 rightCapCenter = center + Vector2.right * (height / 2 - radius);
                Vector3[] centerRectangle = new Vector3[4];
                centerRectangle[0] = new Vector3(leftCapCenter.x, leftCapCenter.y - radius, 0);
                centerRectangle[1] = new Vector3(rightCapCenter.x, rightCapCenter.y - radius, 0);
                centerRectangle[2] = new Vector3(rightCapCenter.x, rightCapCenter.y + radius, 0);
                centerRectangle[3] = new Vector3(leftCapCenter.x, leftCapCenter.y + radius, 0);
                Handles.DrawSolidArc(rightCapCenter, Vector3.forward, Vector3.down, 180, radius);
                Handles.DrawSolidRectangleWithOutline(centerRectangle, Color.white, Color.clear);
                Handles.DrawSolidArc(leftCapCenter, Vector3.forward, Vector3.up, 180, radius);
            }
        }

        public static Mesh GenerateCapsuleMesh(float radius, float height, int segments, int rings)
        {
            // Clamp the height to ensure the hemispheres stay joined
            height = Mathf.Max(height, 2 * radius);

            Mesh mesh = new Mesh();

            // Lists for vertices, normals, and triangles
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();

            float cylinderHeight = height - 2 * radius;

            // Top hemisphere (normals pointing outward)
            for (int i = 0; i <= rings; i++)
            {
                float lat = Mathf.PI * 0.5f * (i / (float)rings);
                float y = Mathf.Sin(lat) * radius;
                float xz = Mathf.Cos(lat) * radius;

                for (int j = 0; j <= segments; j++)
                {
                    float lon = Mathf.PI * 2.0f * (j / (float)segments);
                    float x = Mathf.Cos(lon) * xz;
                    float z = Mathf.Sin(lon) * xz;

                    Vector3 vertex = new Vector3(x, y + cylinderHeight * 0.5f, z);
                    vertices.Add(vertex);

                    // Normal should point outward
                    normals.Add(vertex.normalized);
                }
            }

            // Cylinder body (normals pointing outward horizontally)
            for (int i = 0; i <= 1; i++)
            {
                float y = cylinderHeight * (i - 0.5f);

                for (int j = 0; j <= segments; j++)
                {
                    float lon = Mathf.PI * 2.0f * (j / (float)segments);
                    float x = Mathf.Cos(lon) * radius;
                    float z = Mathf.Sin(lon) * radius;

                    Vector3 vertex = new Vector3(x, y, z);
                    vertices.Add(vertex);

                    // Cylinder normals point outward horizontally
                    normals.Add(new Vector3(x, 0, z).normalized);
                }
            }

            // Bottom hemisphere (normals pointing outward)
            for (int i = rings; i >= 0; i--)
            {
                float lat = Mathf.PI * 0.5f * (i / (float)rings);
                float y = Mathf.Sin(lat) * radius;
                float xz = Mathf.Cos(lat) * radius;

                for (int j = 0; j <= segments; j++)
                {
                    float lon = Mathf.PI * 2.0f * (j / (float)segments);
                    float x = Mathf.Cos(lon) * xz;
                    float z = Mathf.Sin(lon) * xz;

                    Vector3 vertex = new Vector3(x, -y - cylinderHeight * 0.5f, z);
                    vertices.Add(vertex);

                    normals.Add(new Vector3(x, -y, z).normalized);
                }
            }

            // Triangles for top hemisphere
            int vertexCountPerRow = segments + 1;

            for (int i = 0; i < rings; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    int a = i * vertexCountPerRow + j;
                    int b = a + vertexCountPerRow;
                    int c = a + 1;
                    int d = b + 1;

                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(c);

                    triangles.Add(c);
                    triangles.Add(b);
                    triangles.Add(d);
                }
            }

            // Triangles for cylinder body
            int cylinderOffset = (rings + 1) * vertexCountPerRow;

            for (int i = 0; i < 1; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    int a = cylinderOffset + i * vertexCountPerRow + j;
                    int b = a + vertexCountPerRow;
                    int c = a + 1;
                    int d = b + 1;

                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(c);

                    triangles.Add(c);
                    triangles.Add(b);
                    triangles.Add(d);
                }
            }

            // Triangles for bottom hemisphere
            int bottomOffset = cylinderOffset + 2 * vertexCountPerRow;

            for (int i = 0; i < rings; i++)
            {
                for (int j = 0; j < segments; j++)
                {
                    int a = bottomOffset + i * vertexCountPerRow + j;
                    int b = a + vertexCountPerRow;
                    int c = a + 1;
                    int d = b + 1;

                    triangles.Add(a);
                    triangles.Add(b);
                    triangles.Add(c);

                    triangles.Add(c);
                    triangles.Add(b);
                    triangles.Add(d);
                }
            }

            // Assign vertices, normals, and triangles to the mesh
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateBounds();

            return mesh;
        }

        private static void DrawLine(float arg1, float arg2, float forward, float thickness = 1)
        {
            Handles.DrawLine(new Vector3(arg1, arg2, 0f), new Vector3(arg1, arg2, forward), thickness);
        }

        public static void DrawArrow(Vector3 position, Vector3 direction, float magnitude, Vector3 normal, Color color,
            float thickness, bool isDotted = false)
        {
            Color defaultColor = Handles.color;
            Handles.color = color;

            direction.Normalize();
            Vector3 firstPoint = position;
            Vector3 secondPoint = position + direction * magnitude;

            if (isDotted)
            {
                Handles.DrawDottedLine(firstPoint, secondPoint, 3);
            }
            else
            {
                Handles.DrawLine(firstPoint, secondPoint, thickness);
            }


            Vector3 rotation = 35 * normal;
            Vector3 leftPart = Quaternion.Euler(rotation) * -direction.normalized;
            Vector3 rightPart = Quaternion.Euler(-rotation) * -direction.normalized;
            leftPart.Normalize();
            rightPart.Normalize();

            Handles.DrawLine(secondPoint, secondPoint + leftPart * 0.2f, thickness);
            Handles.DrawLine(secondPoint, secondPoint + rightPart * 0.2f, thickness);

            Handles.color = defaultColor;
        }

        public static void DrawArrow(Vector3 position, Vector3 direction, Vector3 normal, Color color, float thickness,
            bool isDotted = false)
        {
            Color defaultColor = Handles.color;
            Handles.color = color;

            Vector3 firstPoint = position;
            Vector3 secondPoint = position + direction;

            if (isDotted)
            {
                Handles.DrawDottedLine(firstPoint, secondPoint, 3);
            }
            else
            {
                Handles.DrawLine(firstPoint, secondPoint, thickness);
            }


            Vector3 rotation = 35 * normal;
            Vector3 leftPart = Quaternion.Euler(rotation) * -direction.normalized;
            Vector3 rightPart = Quaternion.Euler(-rotation) * -direction.normalized;
            leftPart.Normalize();
            rightPart.Normalize();

            Handles.DrawLine(secondPoint, secondPoint + leftPart * 0.2f, thickness);
            Handles.DrawLine(secondPoint, secondPoint + rightPart * 0.2f, thickness);

            Handles.color = defaultColor;
        }

        public static void DrawTextBox(string text, int fontSize, FontStyle fontStyle, Vector3 position,
            Color textColor, Color bgColor)
        {
            // Convert the world position to screen space (pixel coordinates)
            Vector3 screenPosition = HandleUtility.WorldToGUIPoint(position);

            // Define the label style
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = fontSize;
            labelStyle.fontStyle = fontStyle;
            labelStyle.normal.textColor = textColor;

            // Calculate the size of the label in screen space
            Vector2 labelSize = labelStyle.CalcSize(new GUIContent(text));

            // Define the background color
            Color backgroundColor = bgColor;

            // Draw the background box in screen space
            Handles.BeginGUI();
            {
                // Create a rect for the background box
                Rect backgroundRect = new Rect(screenPosition.x - labelSize.x / 2, screenPosition.y - labelSize.y / 2,
                    labelSize.x, labelSize.y);

                // Draw the background box
                EditorGUI.DrawRect(backgroundRect, backgroundColor);

                // Draw the label on top of the background box
                EditorGUI.LabelField(backgroundRect, text, labelStyle);
            }
            Handles.EndGUI();
        }

        public static void DrawTextBoxGUI(string text, int fontSize, FontStyle fontStyle, Vector3 position,
            Color textColor, Color bgColor)
        {
            // Convert the world position to screen space (pixel coordinates)
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(position);

            // Define the label style
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = fontSize;
            labelStyle.fontStyle = fontStyle;
            labelStyle.normal.textColor = textColor;

            // Calculate the size of the label in screen space
            Vector2 labelSize = labelStyle.CalcSize(new GUIContent(text));

            // Define the background color
            Color backgroundColor = bgColor;

            if (screenPosition.z > 0)
            {
                Vector2 guiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
                // Create a rect for the background box
                Rect backgroundRect = new Rect(guiPosition.x - labelSize.x / 2, guiPosition.y - labelSize.y / 2,
                    labelSize.x, labelSize.y);

                // Draw the background box
                EditorGUI.DrawRect(backgroundRect, backgroundColor);

                // Draw the label on top of the background box
                EditorGUI.LabelField(backgroundRect, text, labelStyle);
            }
        }

        public static float GetResolutionMultiplier()
        {
            float widthScale = (float)Screen.width / 1920;
            float heightScale = (float)Screen.height / 1080;

            return Mathf.Min(widthScale, heightScale);
        }

        public static float GetResolutionMultiplier(Vector2 referenceResolution)
        {
            float widthScale = (float)Screen.width / referenceResolution.x;
            float heightScale = (float)Screen.height / referenceResolution.y;

            return Mathf.Min(widthScale, heightScale);
        }

#endif
    }
}