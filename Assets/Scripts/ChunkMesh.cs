using UnityEngine;
using System.Collections.Generic;

// a chunk mesh is a physical representation of a chunk in the virtual world
// it is composed of 3 meshes: the main mesh, the collider mesh and the transparent mesh
public class ChunkMesh
{
    GameObject gameObject; // reference to the unity gameobject for this mesh
    MeshRenderer meshRenderer; // component of the gameobject used to set the material
    MeshFilter meshFilter; // component of the gameobject used to set the visual mesh
    MeshCollider meshCollider; // component of the gameobject used to set the collision mesh

    Mesh mesh = new Mesh();
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    Mesh colliderMesh = new Mesh();
    List<Vector3> colliderVertices = new List<Vector3>();
    List<int> colliderTriangles = new List<int>();

    ChunkMesh nonOpaqueMesh; // the transparent mesh is another instance of chunkmesh and a child of the main mesh

    public ChunkMesh(bool isMainMesh)
    {
        if (isMainMesh)
        {
            nonOpaqueMesh = new ChunkMesh(false);
        }
        gameObject = new GameObject();
        gameObject.transform.parent = GameObject.Find("World").transform;
        gameObject.name = "ChunkMesh";
        gameObject.layer = LayerMask.NameToLayer("Ground");
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMaterial = World.GetPhysicMaterial();
        if (!isMainMesh)
        {
            meshRenderer.material = World.GetNonOpaqueMaterial();
        }
        else
        {
            meshRenderer.material = World.GetMaterial();
        }
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = colliderMesh;
    }

    static Direction[] directions =
    {
        Direction.backwards,
        Direction.down,
        Direction.foreward,
        Direction.left,
        Direction.right,
        Direction.up
    };

    void AddVertices(Direction direction, int x, int y, int z)
    {
        // order of vertices matters for the normals and how we render the mesh
        switch (direction)
        {
            case Direction.backwards:
                vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
                vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
                vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
                vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
                break;
            case Direction.foreward:
                vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
                vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
                vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
                break;
            case Direction.left:
                vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
                vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
                vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
                vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
                break;

            case Direction.right:
                vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
                vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
                vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
                break;
            case Direction.down:
                vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
                vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
                vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
                vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
                break;
            case Direction.up:
                vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
                vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
                vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
                break;
            default:
                break;
        }
    }

    void AddColliderVertices(Direction direction, int x, int y, int z)
    {
        // order of vertices matters for the normals and how we render the mesh
        switch (direction)
        {
            case Direction.backwards:
                colliderVertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
                colliderVertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
                colliderVertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
                colliderVertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
                break;
            case Direction.foreward:
                colliderVertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
                colliderVertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                colliderVertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
                colliderVertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
                break;
            case Direction.left:
                colliderVertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
                colliderVertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
                colliderVertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
                colliderVertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
                break;

            case Direction.right:
                colliderVertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
                colliderVertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
                colliderVertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                colliderVertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
                break;
            case Direction.down:
                colliderVertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
                colliderVertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
                colliderVertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
                colliderVertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
                break;
            case Direction.up:
                colliderVertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
                colliderVertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
                colliderVertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
                colliderVertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
                break;
            default:
                break;
        }
    }

    void AddQuadTriangles()
    {
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);

        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);
    }

    void AddColliderQuadTriangles()
    {
        colliderTriangles.Add(colliderVertices.Count - 4);
        colliderTriangles.Add(colliderVertices.Count - 3);
        colliderTriangles.Add(colliderVertices.Count - 2);

        colliderTriangles.Add(colliderVertices.Count - 4);
        colliderTriangles.Add(colliderVertices.Count - 2);
        colliderTriangles.Add(colliderVertices.Count - 1);
    }

    public void AddBlockToMesh(int x, int y, int z, int voxel, BlockPalette BlockPalette)
    {
        if(voxel == 0)
        {
            return;
        }
        var blockType = BlockPalette.GetBlockType(voxel);
        if (!blockType.IsOpaque() && nonOpaqueMesh != null)
        {
            nonOpaqueMesh.AddBlockToMesh(x, y, z, voxel, BlockPalette);
            return;
        }
        for (int i = 0; i < directions.Length; i++)
        {
            var neighbour = World.GetNeighbourVoxel(x, y, z, directions[i]);
            var neighbourBlockType = BlockPalette.GetBlockType(neighbour);
            if (neighbour == 0 || (!neighbourBlockType.IsOpaque() && blockType.IsOpaque()))
            {
                AddVertices(directions[i], x, y, z);
                AddQuadTriangles();
                uvs.AddRange(BlockPalette.GetBlockUVs(voxel));
            }
            if (neighbour == 0 || (!neighbourBlockType.IsConcrete() && blockType.IsConcrete()))
            {
                AddColliderVertices(directions[i], x, y, z);
                AddColliderQuadTriangles();
            }
        }
        UpdateMesh();
        UpdateColliderMesh();
    }

    // update the meshes every time a block in the chunk changes
    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    // update the meshes every time a block in the chunk changes
    void UpdateColliderMesh()
    {
        colliderMesh.Clear();
        colliderMesh.vertices = colliderVertices.ToArray();
        colliderMesh.triangles = colliderTriangles.ToArray();
        colliderMesh.RecalculateNormals();
        meshCollider.sharedMesh = colliderMesh;
    }
}