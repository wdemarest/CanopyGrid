/*--------------------------------------------------------
   MantisScripter.cs

   Created by MINGFEN WANG on 13-12-26.
   Copyright (c) 2013 MINGFEN WANG. All rights reserved.
   http://www.mesh-online.net/
   --------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
/*
 * If you want to use the managed plugin, you must use this namespace.
 */
using MantisLOD;
using UnityEditor;
using UnityEngine;
using MantisLODEditor;

namespace MantisLODScriptable
{
    /*
    MantisPlugin - for tweaking whether you're using the manager plugin
    MantisParams - all the parameters for changing the triangle count in a mesh
    MantisMeshList - contains a list of optimized meshes
    MantisProgressive - creates a saveable progressive mesh, and can save it
    MantisStandard - can save a standard mesh from a mantis mesh

    MantisScripter
    the scriptable interface to Mantis for generating LoD.
        The main function for scripting is:
        var mantisScripter = new MantisScripter();
        var param = new MantisParams() { set parameters here }
        mantisScripter.OptimizeAndSaveMesh( myGameObject, param, (string fileName) => "myFilePrefix"+fileName+"myFileAffix.asset" );
        ...or...
        mantisScripter.OptimizeAndSaveProgressize( myGameObject, param, "whateverName.asset" );

    */

    public static class MantisPlugin
    {
        public static bool use_managed_plugin = false;
    }
    public class MantisParams
    {
        /*
		 * If you want to simplify meshes at runtime, please use the managed plugin.
		 * The managed plugin is provided as c# script, which can run on all platforms,
		 * but it is slower than the native plugin.
		 * To use the managed plugin, You just need to add the prefix of
		 * 'MantisLODSimpler.' before the native APIs.
		 */

        public float quality = 100.0f;
        public bool protect_boundary = false;
        public bool protect_detail = false;
        public bool protect_symmetry = false;
        public bool protect_normal = false;
        public bool protect_shape = false;
        public bool use_detail_map = false;
        public int detail_boost = 10;
    }

    public class MantisMeshList
    {
        [DllImport("MantisLOD")]
        private static extern int create_progressive_mesh(Vector3[] vertex_array, int vertex_count, int[] triangle_array, int triangle_count, Vector3[] normal_array, int normal_count, Color[] color_array, int color_count, Vector2[] uv_array, int uv_count, int protect_boundary, int protect_detail, int protect_symmetry, int protect_normal, int protect_shape, int use_detail_map, int detail_boost);
        [DllImport("MantisLOD")]
        private static extern int get_triangle_list(int index, float goal, int[] triangles, ref int triangle_count);
        [DllImport("MantisLOD")]
        private static extern int delete_progressive_mesh(int index);

        public Mantis_Mesh[] Mantis_Meshes = null;

        public int origin_face_number = 0;
        public int face_number = 0;

        private bool optimized = false;

        public bool isEmpty { get { return Mantis_Meshes == null; } }
        public int Length { get { return Mantis_Meshes.Length; } }

        public void Traverse(Action<Mantis_Mesh> fn)
        {
            if (Mantis_Meshes == null) return;
            foreach (Mantis_Mesh child in Mantis_Meshes)
            {
                fn(child);
            }
        }
        public bool isReadWrite
        {
            get
            {
                bool read_write_enabled = true;
                foreach (Mantis_Mesh child in Mantis_Meshes)
                {
                    if (!child.mesh.isReadable)
                    {
                        read_write_enabled = false;
                        break;
                    }
                }
                return read_write_enabled;
            }
        }
        public int create_progressive_mesh(Vector3[] vertex_array, int vertex_count, int[] triangle_array, int triangle_count, Vector3[] normal_array, int normal_count, Color[] color_array, int color_count, Vector2[] uv_array, int uv_count, MantisParams param)
        {
            int childIndex;
            if (!MantisPlugin.use_managed_plugin)
            {
                childIndex = create_progressive_mesh(vertex_array, vertex_count, triangle_array, triangle_count, normal_array, normal_count, color_array, color_count, uv_array, uv_count, param.protect_boundary ? 1 : 0, param.protect_detail ? 1 : 0, param.protect_symmetry ? 1 : 0, param.protect_normal ? 1 : 0, param.protect_shape ? 1 : 0, param.use_detail_map ? 1 : 0, param.detail_boost);
            }
            else
            {
                childIndex = MantisLODSimpler.create_progressive_mesh(vertex_array, vertex_count, triangle_array, triangle_count, normal_array, normal_count, color_array, color_count, uv_array, uv_count, param.protect_boundary ? 1 : 0, param.protect_detail ? 1 : 0, param.protect_symmetry ? 1 : 0, param.protect_normal ? 1 : 0, param.protect_shape ? 1 : 0, param.use_detail_map ? 1 : 0, param.detail_boost);
            }
            return childIndex;
        }
        public int get_triangle_list2(int index, float goal, int[] triangles, ref int triangle_count)
        {
            return !MantisPlugin.use_managed_plugin
                ? get_triangle_list(index, goal, triangles, ref triangle_count)
                : MantisLODSimpler.get_triangle_list(index, goal, triangles, ref triangle_count);
        }
        public bool set_quality(float quality)
        {
            bool setDirty = false;

            face_number = 0;

            Traverse((Mantis_Mesh child) =>
            {
                // get triangle list by quality value
                if (child.index != -1 && get_triangle_list2(child.index, quality, child.out_triangles, ref child.out_count) == 1)
                {
                    if (child.out_count > 0)
                    {
                        int counter = 0;
                        int mat = 0;
                        while (counter < child.out_count)
                        {
                            int len = child.out_triangles[counter];
                            counter++;
                            if (len > 0)
                            {
                                int[] new_triangles = new int[len];
                                Array.Copy(child.out_triangles, counter, new_triangles, 0, len);
                                child.mesh.SetTriangles(new_triangles, mat);
                                counter += len;
                            }
                            else
                            {
                                child.mesh.SetTriangles((int[])null, mat);
                            }
                            mat++;
                        }
                        face_number += child.mesh.triangles.Length / 3;
                        // refresh normals and bounds
                        //child.mesh.RecalculateNormals();
                        //child.mesh.RecalculateBounds();
                        setDirty = true;
                    }
                }
            });
            return setDirty;
        }
        private string get_uuid_from_mesh(Mesh mesh)
        {
            string uuid = mesh.name + "_" + mesh.vertexCount.ToString() + "_" + mesh.subMeshCount.ToString();
            for (int i = 0; i < mesh.subMeshCount; ++i)
            {
                uuid += "_" + mesh.GetIndexCount(i).ToString();
            }
            return uuid;
        }
        private void get_all_meshes(GameObject gameObject)
        {
            Component[] allFilters = (Component[])gameObject.GetComponentsInChildren(typeof(MeshFilter));
            Component[] allRenderers = (Component[])gameObject.GetComponentsInChildren(typeof(SkinnedMeshRenderer));
            int mesh_count = allFilters.Length + allRenderers.Length;
            if (mesh_count > 0)
            {
                Mantis_Meshes = new Mantis_Mesh[mesh_count];
                int counter = 0;
                foreach (Component child in allFilters)
                {
                    Mantis_Meshes[counter] = new Mantis_Mesh();
                    Mantis_Meshes[counter].mesh = ((MeshFilter)child).sharedMesh;
                    Mantis_Meshes[counter].uuid = get_uuid_from_mesh(((MeshFilter)child).sharedMesh);
                    counter++;
                }
                foreach (Component child in allRenderers)
                {
                    Mantis_Meshes[counter] = new Mantis_Mesh();
                    Mantis_Meshes[counter].mesh = ((SkinnedMeshRenderer)child).sharedMesh;
                    Mantis_Meshes[counter].uuid = get_uuid_from_mesh(((SkinnedMeshRenderer)child).sharedMesh);
                    counter++;
                }
            }
        }
        public bool init_all(GameObject gameObject)
        {
            bool facesCounted = false;
            if (isEmpty)
            {
                get_all_meshes(gameObject);
                if (!isEmpty)
                {
                    optimized = false;
                    face_number = 0;
                    foreach (Mantis_Mesh child in Mantis_Meshes)
                    {
                        int triangle_number = child.mesh.triangles.Length;
                        child.origin_triangles = new int[child.mesh.subMeshCount][];
                        // out data is large than origin data
                        child.out_triangles = new int[triangle_number + child.mesh.subMeshCount];
                        for (int i = 0; i < child.mesh.subMeshCount; i++)
                        {
                            int[] sub_triangles = child.mesh.GetTriangles(i);
                            face_number += sub_triangles.Length / 3;
                            // save origin triangle list
                            child.origin_triangles[i] = new int[sub_triangles.Length];
                            Array.Copy(sub_triangles, child.origin_triangles[i], sub_triangles.Length);
                        }
                        child.index = -1;
                    }
                    origin_face_number = face_number;
                    facesCounted = true;
                }
            }
            return facesCounted;
        }
        public void clean_all()
        {
            // restore triangle list
            if (Mantis_Meshes != null)
            {
                foreach (Mantis_Mesh child in Mantis_Meshes)
                {
                    if (child.index != -1)
                    {
                        for (int i = 0; i < child.mesh.subMeshCount; i++)
                        {
                            child.mesh.SetTriangles(child.origin_triangles[i], i);
                        }
                        //child.mesh.RecalculateNormals();
                        //child.mesh.RecalculateBounds();
                        // do not need it
                        if (!MantisPlugin.use_managed_plugin)
                        {
                            delete_progressive_mesh(child.index);
                        }
                        else
                        {
                            MantisLODSimpler.delete_progressive_mesh(child.index);
                        }
                        child.index = -1;
                    }
                }
            }
            Mantis_Meshes = null;
        }
        public void optimize(MantisParams param)
        {
            if (optimized) return;

            Traverse((Mantis_Mesh child) =>
            {
                int triangle_number = child.mesh.triangles.Length;
                Vector3[] vertices = child.mesh.vertices;
                // in data is large than origin data
                int[] triangles = new int[triangle_number + child.mesh.subMeshCount];
                // we need normal data to protect normal boundary
                Vector3[] normals = child.mesh.normals;
                // we need color data to protect color boundary
                Color[] colors = child.mesh.colors;
                // we need uv data to protect uv boundary
                Vector2[] uvs = child.mesh.uv;
                int counter = 0;
                for (int i = 0; i < child.mesh.subMeshCount; i++)
                {
                    int[] sub_triangles = child.mesh.GetTriangles(i);
                    triangles[counter] = sub_triangles.Length;
                    counter++;
                    Array.Copy(sub_triangles, 0, triangles, counter, sub_triangles.Length);
                    counter += sub_triangles.Length;
                }
                // create progressive mesh
                child.index = create_progressive_mesh(vertices, vertices.Length, triangles, counter, normals, normals.Length, colors, colors.Length, uvs, uvs.Length, param);
            });
            optimized = true;
        }
    }

    public class MantisProgressive
    {
        public int max_lod_count = 8;

        public void fill_progressive_mesh(MantisMeshList mantisMeshList, ProgressiveMesh pm)
        {
            int triangle_count = 0;
            int[][][][] temp_triangles;
            temp_triangles = new int[max_lod_count][][][];
            // max lod count
            triangle_count++;
            for (int lod = 0; lod < temp_triangles.Length; lod++)
            {
                float quality = 100.0f * (temp_triangles.Length - lod) / temp_triangles.Length;
                temp_triangles[lod] = new int[mantisMeshList.Length][][];
                // mesh count
                triangle_count++;
                int mesh_count = 0;
                pm.uuids = new string[mantisMeshList.Length];
                mantisMeshList.Traverse((Mantis_Mesh child) =>
                {
                    // get triangle list by quality value
                    if (child.index != -1 && mantisMeshList.get_triangle_list2(child.index, quality, child.out_triangles, ref child.out_count) == 1)
                    {
                        if (child.out_count > 0)
                        {
                            int counter = 0;
                            int mat = 0;
                            temp_triangles[lod][mesh_count] = new int[child.mesh.subMeshCount][];
                            // sub mesh count
                            triangle_count++;
                            while (counter < child.out_count)
                            {
                                int len = child.out_triangles[counter];
                                // triangle count
                                triangle_count++;
                                // triangle list count
                                triangle_count += len;
                                counter++;
                                int[] new_triangles = new int[len];
                                Array.Copy(child.out_triangles, counter, new_triangles, 0, len);
                                temp_triangles[lod][mesh_count][mat] = new_triangles;
                                counter += len;
                                mat++;
                            }
                        }
                        else
                        {
                            temp_triangles[lod][mesh_count] = new int[child.mesh.subMeshCount][];
                            // sub mesh count
                            triangle_count++;
                            for (int mat = 0; mat < temp_triangles[lod][mesh_count].Length; mat++)
                            {
                                temp_triangles[lod][mesh_count][mat] = new int[0];
                                // triangle count
                                triangle_count++;
                            }
                        }
                    }
                    pm.uuids[mesh_count] = child.uuid;
                    mesh_count++;
                });
            }
            // create fix size array
            pm.triangles = new int[triangle_count];

            // reset the counter
            triangle_count = 0;
            // max lod count
            pm.triangles[triangle_count] = temp_triangles.Length;
            triangle_count++;
            for (int lod = 0; lod < temp_triangles.Length; lod++)
            {
                // mesh count
                pm.triangles[triangle_count] = temp_triangles[lod].Length;
                triangle_count++;
                for (int mesh_count = 0; mesh_count < temp_triangles[lod].Length; mesh_count++)
                {
                    // sub mesh count
                    pm.triangles[triangle_count] = temp_triangles[lod][mesh_count].Length;
                    triangle_count++;
                    for (int mat = 0; mat < temp_triangles[lod][mesh_count].Length; mat++)
                    {
                        // triangle count
                        pm.triangles[triangle_count] = temp_triangles[lod][mesh_count][mat].Length;
                        triangle_count++;
                        Array.Copy(temp_triangles[lod][mesh_count][mat], 0, pm.triangles, triangle_count, temp_triangles[lod][mesh_count][mat].Length);
                        // triangle list count
                        triangle_count += temp_triangles[lod][mesh_count][mat].Length;
                    }
                }
            }
        }

        public ProgressiveMesh Create(MantisMeshList mantisMeshList)
        {
            ProgressiveMesh pm = (ProgressiveMesh)ScriptableObject.CreateInstance(typeof(ProgressiveMesh));
            fill_progressive_mesh(mantisMeshList, pm);
            return pm;
        }

        public void Save(ProgressiveMesh pm, string filePath)
        {
            if (filePath != "")
            {
                AssetDatabase.CreateAsset(pm, filePath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }

    public class MantisStandard
    {
        public Mesh Save(Mesh saveMesh, string filePath)
        {
            Mesh mesh = (Mesh)UnityEngine.Object.Instantiate(saveMesh);
            MeshUtility.Optimize(mesh);
            AssetDatabase.CreateAsset(mesh, filePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return mesh;
        }
    }

    public class MantisScripter : MantisMeshList
    {
        public MantisScripter(GameObject source, GameObject target)
        {
            MeshFilter targetM = target.GetComponent<MeshFilter>();
            MeshFilter sourceM = source.GetComponent<MeshFilter>();
            targetM.sharedMesh = (Mesh)GameObject.Instantiate(sourceM.sharedMesh);
            targetM.mesh = targetM.sharedMesh;

            init_all(target);
        }
        public void Reduce(MantisParams param)
        {
            optimize(param);
            set_quality(param.quality);
        }
        public Mesh GetMesh(int index)
        {
            return Mantis_Meshes?[index]?.mesh;
        }
        public void ReleaseAndRevertMesh()
        {
            clean_all();
        }
    }
}
