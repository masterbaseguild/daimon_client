using UnityEngine;
using System.Collections.Generic;

// a chunk mesh is a physical representation of a chunk in the virtual world
// it is composed of 4 meshes: the main mesh, the collider mesh, the transparent mesh and the non concrete mesh
// NOTE: for the moment we do not have any non concrete opaque block, so the non concrete mesh can only use the transparent material
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
    private readonly ChunkMesh nonConcreteMesh; // the non concrete mesh is another instance of chunkmesh and a child of the main mesh

    private static readonly Direction[] directions =
    {
        Direction.backwards,
        Direction.down,
        Direction.foreward,
        Direction.left,
        Direction.right,
        Direction.up
    };

    public ChunkMesh(int meshType) // 0 = main mesh, 1 = non opaque mesh, 2 = non concrete mesh
    {
        world = GameObject.Find("World").GetComponent<World>();
        if (meshType == 0)
        {
            nonOpaqueMesh = new ChunkMesh(1);
            nonConcreteMesh = new ChunkMesh(2);
        }
        gameObject = new GameObject();
        gameObject.transform.parent = GameObject.Find("World").transform;
        gameObject.name = "ChunkMesh";
        if (meshType != 2)
        {
        gameObject.layer = LayerMask.NameToLayer("Ground");
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMaterial = world.physicMaterial;
        meshCollider.sharedMesh = colliderMesh;
        }
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        if (meshType != 0)
        {
            meshRenderer.material = world.nonOpaqueMaterial;
        }
        else
        {
            meshRenderer.material = world.material;
        }
        meshFilter.mesh = mesh;
    }

    public void DeleteMesh(int meshType)
    {
        if (meshType == 0)
        {
            nonOpaqueMesh.DeleteMesh(1);
            nonConcreteMesh.DeleteMesh(2);
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
        if (!blockType.IsConcrete() && nonConcreteMesh != null)
        {
            nonConcreteMesh.AddBlockToMesh(x, y, z, voxel, BlockPalette);
            return;
        }
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
        if(blockType.IsConcrete())
        {
            UpdateColliderMesh();
        }
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