using UnityEngine;
using System.Collections.Generic;

// a chunk mesh is a physical representation of a chunk in the virtual world
// it is composed of 3 meshes: the main mesh, the collider mesh and the transparent mesh
public class ChunkMesh
{
    private readonly World world;
    private readonly GameObject gameObject; // reference to the unity gameobject for this mesh
    private readonly MeshRenderer meshRenderer; // component of the gameobject used to set the material
    private readonly MeshFilter meshFilter; // component of the gameobject used to set the visual mesh
    private readonly MeshCollider meshCollider; // component of the gameobject used to set the collision mesh

    private readonly Mesh mesh = new();
    private readonly List<Vector3> vertices = new();
    private readonly List<int> triangles = new();
    private readonly List<Vector2> uvs = new();

    private readonly Mesh colliderMesh = new();
    private readonly List<Vector3> colliderVertices = new();
    private readonly List<int> colliderTriangles = new();

    private readonly ChunkMesh nonOpaqueMesh; // the transparent mesh is another instance of chunkmesh and a child of the main mesh

    private static readonly Direction[] directions =
    {
        Direction.backwards,
        Direction.down,
        Direction.foreward,
        Direction.left,
        Direction.right,
        Direction.up
    };

    public ChunkMesh(bool isMainMesh)
    {
        world = GameObject.Find("World").GetComponent<World>();
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
        meshCollider.sharedMaterial = world.physicMaterial;
        meshRenderer.material = !isMainMesh ? world.nonOpaqueMaterial : world.material;
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = colliderMesh;
    }

    public void DeleteMesh(bool isMainMesh)
    {
        if (isMainMesh)
        {
            nonOpaqueMesh.DeleteMesh(false);
        }
        Object.Destroy(gameObject);
        Object.Destroy(mesh);
        Object.Destroy(colliderMesh);
    }

    private void AddVertices(Direction direction, int x, int y, int z)
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

    private void AddColliderVertices(Direction direction, int x, int y, int z)
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

    private void AddQuadTriangles()
    {
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);

        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);
    }

    private void AddColliderQuadTriangles()
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
        if (voxel == 0)
        {
            return;
        }
        BlockType blockType = BlockPalette.GetBlockType(voxel);
        if (!blockType.IsOpaque() && nonOpaqueMesh != null)
        {
            nonOpaqueMesh.AddBlockToMesh(x, y, z, voxel, BlockPalette);
            return;
        }
        for (int i = 0; i < directions.Length; i++)
        {
            int neighbour = world.GetNeighbourVoxel(x, y, z, directions[i]);
            BlockType neighbourBlockType = BlockPalette.GetBlockType(neighbour);
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
    private void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    // update the meshes every time a block in the chunk changes
    private void UpdateColliderMesh()
    {
        colliderMesh.Clear();
        colliderMesh.vertices = colliderVertices.ToArray();
        colliderMesh.triangles = colliderTriangles.ToArray();
        colliderMesh.RecalculateNormals();
        meshCollider.sharedMesh = colliderMesh;
    }
}