using System.Collections.Generic;
using AoE.RTS.Visuals;
using UnityEditor;
using UnityEngine;

namespace AoE.RTS.EditorTools
{
    public static class PlaceholderVisualMeshFactory
    {
        public static void CreateAllPrefabs()
        {
            EnsureFolder(PlaceholderVisualPaths.ResourcesFolder);

            SavePrefab(BuildVillager(), PlaceholderVisualPaths.VillagerPrefabAsset);
            SavePrefab(BuildMilitia(), PlaceholderVisualPaths.MilitiaPrefabAsset);
            SavePrefab(BuildTownCenter(), PlaceholderVisualPaths.TownCenterPrefabAsset);
            SavePrefab(BuildHouse(), PlaceholderVisualPaths.HousePrefabAsset);
            SavePrefab(BuildBarracks(), PlaceholderVisualPaths.BarracksPrefabAsset);
            SavePrefab(BuildTree(), PlaceholderVisualPaths.TreePrefabAsset);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
                AssetDatabase.CreateFolder("Assets", "Resources");

            AssetDatabase.CreateFolder(parent, name);
        }

        static GameObject BuildVillager()
        {
            CombineInstance[] parts =
            {
                BoxInstance(0.55f, 1.0f, 0.35f, new Vector3(0f, 0.7f, 0f)),
                BoxInstance(0.32f, 0.32f, 0.32f, new Vector3(0f, 1.41f, 0f))
            };
            return CreateCombinedObject("VillagerVisual", parts);
        }

        static GameObject BuildMilitia()
        {
            CombineInstance[] parts =
            {
                BoxInstance(0.6f, 1.05f, 0.4f, new Vector3(0f, 0.675f, 0f)),
                BoxInstance(0.34f, 0.34f, 0.34f, new Vector3(0f, 1.44f, 0f)),
                BoxInstance(0.08f, 0.7f, 0.08f, new Vector3(0.42f, 0.8f, 0f))
            };
            return CreateCombinedObject("MilitiaVisual", parts);
        }

        static GameObject BuildTownCenter()
        {
            CombineInstance[] parts =
            {
                BoxInstance(7.5f, 3.5f, 7.5f, new Vector3(0f, 1.75f, 0f)),
                BoxInstance(2.0f, 2.5f, 2.0f, new Vector3(0f, 4.75f, 0f))
            };
            return CreateCombinedObject("TownCenterVisual", parts);
        }

        static GameObject BuildHouse()
        {
            List<CombineInstance> parts = new List<CombineInstance>
            {
                BoxInstance(3.8f, 2.5f, 3.8f, new Vector3(0f, 1.25f, 0f))
            };

            const int segments = 8;
            const float radius = 2.8f;
            const float yBase = 2.5f;
            Vector3 apex = new Vector3(0f, 4.2f, 0f);
            for (int i = 0; i < segments; i++)
            {
                float a0 = i * Mathf.PI * 2f / segments;
                float a1 = (i + 1) * Mathf.PI * 2f / segments;
                Vector3 p0 = new Vector3(Mathf.Cos(a0) * radius, yBase, Mathf.Sin(a0) * radius);
                Vector3 p1 = new Vector3(Mathf.Cos(a1) * radius, yBase, Mathf.Sin(a1) * radius);
                parts.Add(TriangleInstance(apex, p0, p1));
            }

            return CreateCombinedObject("HouseVisual", parts.ToArray());
        }

        static GameObject BuildBarracks()
        {
            CombineInstance[] parts =
            {
                BoxInstance(5.8f, 3.2f, 5.8f, new Vector3(0f, 1.6f, 0f)),
                BoxInstance(1.2f, 0.8f, 4.0f, new Vector3(0f, 3.6f, 2.2f))
            };
            return CreateCombinedObject("BarracksVisual", parts);
        }

        static GameObject BuildTree()
        {
            CombineInstance[] parts =
            {
                BoxInstance(0.35f, 1.2f, 0.35f, new Vector3(0f, 0.6f, 0f)),
                ConeInstance(1.4f, 2.8f, 10, new Vector3(0f, 1.0f, 0f))
            };
            return CreateCombinedObject("TreeVisual", parts);
        }

        static CombineInstance BoxInstance(float width, float height, float depth, Vector3 center)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.localScale = new Vector3(width, height, depth);
            cube.transform.localPosition = center;
            MeshFilter filter = cube.GetComponent<MeshFilter>();
            CombineInstance instance = new CombineInstance
            {
                mesh = Object.Instantiate(filter.sharedMesh),
                transform = cube.transform.localToWorldMatrix
            };
            Object.DestroyImmediate(cube);
            return instance;
        }

        static CombineInstance ConeInstance(float radius, float height, int segments, Vector3 center)
        {
            Mesh mesh = CreateConeMesh(radius, height, segments);
            CombineInstance instance = new CombineInstance
            {
                mesh = mesh,
                transform = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one)
            };
            return instance;
        }

        static CombineInstance TriangleInstance(Vector3 a, Vector3 b, Vector3 c)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = new[] { a, b, c };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return new CombineInstance
            {
                mesh = mesh,
                transform = Matrix4x4.identity
            };
        }

        static Mesh CreateConeMesh(float radius, float height, int segments)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            Vector3 apex = new Vector3(0f, height, 0f);
            Vector3 baseCenter = Vector3.zero;

            for (int i = 0; i < segments; i++)
            {
                float a0 = i * Mathf.PI * 2f / segments;
                float a1 = (i + 1) * Mathf.PI * 2f / segments;
                Vector3 p0 = new Vector3(Mathf.Cos(a0) * radius, 0f, Mathf.Sin(a0) * radius);
                Vector3 p1 = new Vector3(Mathf.Cos(a1) * radius, 0f, Mathf.Sin(a1) * radius);

                int start = vertices.Count;
                vertices.Add(apex);
                vertices.Add(p0);
                vertices.Add(p1);
                triangles.Add(start);
                triangles.Add(start + 1);
                triangles.Add(start + 2);

                start = vertices.Count;
                vertices.Add(baseCenter);
                vertices.Add(p0);
                vertices.Add(p1);
                triangles.Add(start);
                triangles.Add(start + 2);
                triangles.Add(start + 1);
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static Material sharedMaterial;

        static Material GetOrCreateSharedMaterial()
        {
            if (sharedMaterial != null)
                return sharedMaterial;

            const string materialPath = PlaceholderVisualPaths.ResourcesFolder + "/PlaceholderLit.mat";
            sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (sharedMaterial != null)
                return sharedMaterial;

            sharedMaterial = SceneMaterialFactory.CreateLitMaterial(new Color(0.75f, 0.75f, 0.75f));
            AssetDatabase.CreateAsset(sharedMaterial, materialPath);
            return sharedMaterial;
        }

        static GameObject CreateCombinedObject(string objectName, CombineInstance[] parts)
        {
            Mesh combinedMesh = new Mesh { name = objectName + "Mesh" };
            combinedMesh.CombineMeshes(parts, true, true);
            combinedMesh.RecalculateNormals();
            combinedMesh.RecalculateBounds();

            GameObject root = new GameObject(objectName);
            MeshFilter filter = root.AddComponent<MeshFilter>();
            MeshRenderer renderer = root.AddComponent<MeshRenderer>();
            filter.sharedMesh = combinedMesh;
            renderer.sharedMaterial = GetOrCreateSharedMaterial();
            return root;
        }

        static void SavePrefab(GameObject source, string prefabPath)
        {
            MeshFilter filter = source.GetComponent<MeshFilter>();
            MeshRenderer renderer = source.GetComponent<MeshRenderer>();

            string baseName = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
            string folder = System.IO.Path.GetDirectoryName(prefabPath)?.Replace('\\', '/');
            string meshPath = folder + "/" + baseName + "_Mesh.asset";

            if (AssetDatabase.LoadAssetAtPath<Mesh>(meshPath) != null)
                AssetDatabase.DeleteAsset(meshPath);

            Mesh meshAsset = Object.Instantiate(filter.sharedMesh);
            meshAsset.name = baseName + "_Mesh";
            AssetDatabase.CreateAsset(meshAsset, meshPath);

            filter.sharedMesh = meshAsset;
            renderer.sharedMaterial = GetOrCreateSharedMaterial();

            PrefabUtility.SaveAsPrefabAsset(source, prefabPath);
            Object.DestroyImmediate(source);
        }
    }
}
