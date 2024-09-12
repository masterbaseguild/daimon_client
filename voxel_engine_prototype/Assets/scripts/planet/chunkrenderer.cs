using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class chunkrenderer : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    Mesh mesh;
    bool showGizmo = false;

    public chunkdata chunkData {get; private set;}

    public bool playerEdited
    {
        get
        {
            return chunkdata.playerEdited;
        }
        set
        {
            chunkdata.playerEdited = value;
        }
    }

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        mesh = meshFilter.mesh;

    }

    public void initChunk(chunkdata chunkData)
    {
        this.chunkData = chunkData;
    }

    private void renderMesh(meshdata meshData)
    {
        mesh.Clear();
        mesh.subMeshCount = 2;
        mesh.vertices = meshData.vertices.Concat(meshData.waterMesh.vertices).ToArray();

        mesh.SetTriangles(meshData.triangles.ToArray(), 0);
        mesh.SetTriangles(meshData.waterMesh.triangles.Select(val => val + meshData.vertices.Count).ToArray(), 1);

        mesh.uv = meshData.uv.Concat(meshData.waterMesh.uv).ToArray();
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = null;
        Mesh collisionMesh = new Mesh();
        collisionMesh.vertices = meshData.collVertices.ToArray();
        collisionMesh.triangles = meshData.collTriangles.ToArray();
        collisionMesh.RecalculateNormals();

        meshCollider.sharedMesh = collisionMesh;
    }

    public void updateChunk()
    {
        renderMesh(chunk.getChunkMeshData(chunkData));
    }

    public void updateChunk(meshdata data)
    {
        renderMesh(data);
    }

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        if (showGizmo)
        {
            if (Application.isPlaying && chunkData != null)
            {
                if (Selection.activeObject == gameObject)
                    Gizmos.color = new Color(0, 1, 0, 0.4f);
                else
                    Gizmos.color = new Color(1, 0, 1, 0.4f);

                Gizmos.DrawCube(transform.position + new Vector3(chunkdata.chunkDefaultDim / 2f, chunkdata.chunkDefaultDim / 2f, chunkdata.chunkDefaultDim / 2f), new Vector3(chunkdata.chunkDefaultDim, chunkdata.chunkDefaultDim, chunkdata.chunkDefaultDim));
            }
        }
    }

#endif

}