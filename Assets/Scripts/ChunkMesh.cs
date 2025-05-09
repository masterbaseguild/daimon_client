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
    private readonly Mesh colliderMesh = new();

    private readonly BlockMesh[,,] voxels = new BlockMesh[Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE, Chunk.CHUNK_SIZE];
    private readonly MiniBlockMesh[,,] miniVoxels = new MiniBlockMesh[Chunk.CHUNK_SIZE*2, Chunk.CHUNK_SIZE*2, Chunk.CHUNK_SIZE*2];

    private readonly int meshType;
    private readonly ChunkMesh nonOpaqueMesh; // the transparent mesh is another instance of chunkmesh and a child of the main mesh
    private readonly ChunkMesh nonConcreteMesh; // the non concrete mesh is another instance of chunkmesh and a child of the main mesh
    private readonly ChunkMesh nonConcreteNonOpaqueMesh; // the non concrete mesh is another instance of chunkmesh and a child of the main mesh

    private static readonly Direction[] directions =
    {
        Direction.backwards,
        Direction.down,
        Direction.foreward,
        Direction.left,
        Direction.right,
        Direction.up
    };

    public ChunkMesh(int meshType) // 0 = main mesh, 1 = non opaque mesh, 2 = non concrete mesh, 3 = non concrete non opaque mesh
    {
        world = GameObject.Find("World").GetComponent<World>();

        gameObject = new GameObject();
        gameObject.transform.parent = GameObject.Find("World").transform;
        gameObject.name = "ChunkMesh";

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        this.meshType = meshType;
        if (meshType == 0) // main mesh
        {
            nonOpaqueMesh = new ChunkMesh(1);
            nonConcreteMesh = new ChunkMesh(2);
            nonConcreteNonOpaqueMesh = new ChunkMesh(3);
        }
        if (meshType <= 1) // concrete meshes
        {
            gameObject.layer = LayerMask.NameToLayer("Ground");
            meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMaterial = world.physicMaterial;
            meshCollider.sharedMesh = colliderMesh;
        }
        if (meshType%2 == 0) // opaque meshes
        {
            meshRenderer.material = world.material;
        }
        else // non opaque meshes
        {
            meshRenderer.material = world.nonOpaqueMaterial;
        }
    }

    public void RemoveBlockFromMesh(int x, int y, int z, int voxel, BlockPalette BlockPalette)
    {
        BlockType blockType = BlockPalette.GetBlockType(voxel);
        int intendedMeshType = (blockType.IsConcrete() ? 0 : 2) + (blockType.IsOpaque() ? 0 : 1);
        if (meshType == 0)
        {
            if (intendedMeshType == 1)
            {
                nonOpaqueMesh.RemoveBlockFromMesh(x, y, z, voxel, BlockPalette);
                return;
            }
            if (intendedMeshType == 2)
            {
                nonConcreteMesh.RemoveBlockFromMesh(x, y, z, voxel, BlockPalette);
                return;
            }
            if (intendedMeshType == 3)
            {
                nonConcreteNonOpaqueMesh.RemoveBlockFromMesh(x, y, z, voxel, BlockPalette);
                return;
            }
        }
        int chunkX = x % Chunk.CHUNK_SIZE;
        int chunkY = y % Chunk.CHUNK_SIZE;
        int chunkZ = z % Chunk.CHUNK_SIZE;
        voxels[chunkX, chunkY, chunkZ] = null;
        UpdateMesh();
        if(blockType.IsConcrete())
        {
            UpdateColliderMesh();
        }
    }

    public void RemoveMiniBlockFromMesh(int x, int y, int z, int voxel, BlockPalette BlockPalette)
    {
        BlockType blockType = BlockPalette.GetBlockType(voxel);
        int intendedMeshType = (blockType.IsConcrete() ? 0 : 2) + (blockType.IsOpaque() ? 0 : 1);
        if (meshType == 0)
        {
            if (intendedMeshType == 1)
            {
                nonOpaqueMesh.RemoveMiniBlockFromMesh(x, y, z, voxel, BlockPalette);
                return;
            }
            if (intendedMeshType == 2)
            {
                nonConcreteMesh.RemoveMiniBlockFromMesh(x, y, z, voxel, BlockPalette);
                return;
            }
            if (intendedMeshType == 3)
            {
                nonConcreteNonOpaqueMesh.RemoveMiniBlockFromMesh(x, y, z, voxel, BlockPalette);
                return;
            }
        }
        int chunkX = x % (Chunk.CHUNK_SIZE*2);
        int chunkY = y % (Chunk.CHUNK_SIZE*2);
        int chunkZ = z % (Chunk.CHUNK_SIZE*2);
        miniVoxels[chunkX, chunkY, chunkZ] = null;
        UpdateMesh();
        if(blockType.IsConcrete())
        {
            UpdateColliderMesh();
        }
    }

    public void AddBlockToMesh(int x, int y, int z, int voxel, BlockPalette BlockPalette, bool isWorldInit)
    {
        if (voxel == 0)
        {
            return;
        }
        BlockType blockType = BlockPalette.GetBlockType(voxel);
        int intendedMeshType = (blockType.IsConcrete() ? 0 : 2) + (blockType.IsOpaque() ? 0 : 1);
        if (meshType == 0)
        {
            if (intendedMeshType == 1)
            {
                nonOpaqueMesh.AddBlockToMesh(x, y, z, voxel, BlockPalette, isWorldInit);
                return;
            }
            if (intendedMeshType == 2)
            {
                nonConcreteMesh.AddBlockToMesh(x, y, z, voxel, BlockPalette, isWorldInit);
                return;
            }
            if (intendedMeshType == 3)
            {
                nonConcreteNonOpaqueMesh.AddBlockToMesh(x, y, z, voxel, BlockPalette, isWorldInit);
                return;
            }
        }
        int chunkX = x % Chunk.CHUNK_SIZE;
        int chunkY = y % Chunk.CHUNK_SIZE;
        int chunkZ = z % Chunk.CHUNK_SIZE;
        BlockMesh currentVoxel = new()
        {
            voxel = voxel
        };
        for (int i = 0; i < directions.Length; i++)
        {
            int neighbour = world.GetNeighbourVoxel(x, y, z, directions[i]);
            BlockType neighbourBlockType = BlockPalette.GetBlockType(neighbour);
            if (neighbour == 0 || (!neighbourBlockType.IsOpaque() && blockType != neighbourBlockType))
            {
                currentVoxel.AddVertices(directions[i], x, y, z);
                currentVoxel.AddQuadTriangles();
                currentVoxel.AddUvs(BlockPalette.GetBlockUVs(voxel));
            }
            if (neighbour == 0 || (!neighbourBlockType.IsConcrete() && blockType.IsConcrete()))
            {
                currentVoxel.AddColliderVertices(directions[i], x, y, z);
                currentVoxel.AddColliderQuadTriangles();
            }
        }
        voxels[chunkX, chunkY, chunkZ] = currentVoxel;
        if(!isWorldInit)
        {
            UpdateMesh();
            if(blockType.IsConcrete())
            {
                UpdateColliderMesh();
            }
        }
    }

    public void AddMiniBlockToMesh(int x, int y, int z, int voxel, BlockPalette BlockPalette, bool isWorldInit)
    {
        if (voxel == 0)
        {
            return;
        }
        BlockType blockType = BlockPalette.GetBlockType(voxel);
        int intendedMeshType = (blockType.IsConcrete() ? 0 : 2) + (blockType.IsOpaque() ? 0 : 1);
        if (meshType == 0)
        {
            if (intendedMeshType == 1)
            {
                nonOpaqueMesh.AddMiniBlockToMesh(x, y, z, voxel, BlockPalette, isWorldInit);
                return;
            }
            if (intendedMeshType == 2)
            {
                nonConcreteMesh.AddMiniBlockToMesh(x, y, z, voxel, BlockPalette, isWorldInit);
                return;
            }
            if (intendedMeshType == 3)
            {
                nonConcreteNonOpaqueMesh.AddMiniBlockToMesh(x, y, z, voxel, BlockPalette, isWorldInit);
                return;
            }
        }
        int chunkX = x % (Chunk.CHUNK_SIZE*2);
        int chunkY = y % (Chunk.CHUNK_SIZE*2);
        int chunkZ = z % (Chunk.CHUNK_SIZE*2);
        MiniBlockMesh currentVoxel = new()
        {
            voxel = voxel
        };
        for (int i = 0; i < directions.Length; i++)
        {
            currentVoxel.AddVertices(directions[i], x, y, z);
            currentVoxel.AddQuadTriangles();
            currentVoxel.AddUvs(BlockPalette.GetBlockUVs(voxel));
            currentVoxel.AddColliderVertices(directions[i], x, y, z);
            currentVoxel.AddColliderQuadTriangles();
        }
        miniVoxels[chunkX, chunkY, chunkZ] = currentVoxel;
        if(!isWorldInit)
        {
            UpdateMesh();
            if(blockType.IsConcrete())
            {
                UpdateColliderMesh();
            }
        }
    }

    // update the meshes every time a block in the chunk changes
    private void UpdateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
            {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                {
                    BlockMesh currentVoxel = voxels[x, y, z];
                    if (currentVoxel != null)
                    {
                        var newTriangles = new List<int>(currentVoxel.triangles);
                        for (int i = 0; i < newTriangles.Count; i++)
                        {
                            newTriangles[i] += vertices.Count;
                        }
                        triangles.AddRange(newTriangles);
                        vertices.AddRange(currentVoxel.vertices);
                        uvs.AddRange(currentVoxel.uvs);
                    }
                }
            }
        }
        for (int x = 0; x < Chunk.CHUNK_SIZE*2; x++)
        {
            for (int y = 0; y < Chunk.CHUNK_SIZE*2; y++)
            {
                for (int z = 0; z < Chunk.CHUNK_SIZE*2; z++)
                {
                    MiniBlockMesh currentVoxel = miniVoxels[x, y, z];
                    if (currentVoxel != null)
                    {
                        var newTriangles = new List<int>(currentVoxel.triangles);
                        for (int i = 0; i < newTriangles.Count; i++)
                        {
                            newTriangles[i] += vertices.Count;
                        }
                        triangles.AddRange(newTriangles);
                        vertices.AddRange(currentVoxel.vertices);
                        uvs.AddRange(currentVoxel.uvs);
                    }
                }
            }
        }
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
        List<Vector3> colliderVertices = new List<Vector3>();
        List<int> colliderTriangles = new List<int>();
        for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
            {
                for (int z = 0; z < Chunk.CHUNK_SIZE; z++)
                {
                    BlockMesh currentVoxel = voxels[x, y, z];
                    if (currentVoxel != null)
                    {
                        var newTriangles = new List<int>(currentVoxel.colliderTriangles);
                        for (int i = 0; i < newTriangles.Count; i++)
                        {
                            newTriangles[i] += colliderVertices.Count;
                        }
                        colliderTriangles.AddRange(newTriangles);
                        colliderVertices.AddRange(currentVoxel.colliderVertices);
                    }
                }
            }
        }
        for (int x = 0; x < Chunk.CHUNK_SIZE*2; x++)
        {
            for (int y = 0; y < Chunk.CHUNK_SIZE*2; y++)
            {
                for (int z = 0; z < Chunk.CHUNK_SIZE*2; z++)
                {
                    MiniBlockMesh currentVoxel = miniVoxels[x, y, z];
                    if (currentVoxel != null)
                    {
                        var newTriangles = new List<int>(currentVoxel.colliderTriangles);
                        for (int i = 0; i < newTriangles.Count; i++)
                        {
                            newTriangles[i] += colliderVertices.Count;
                        }
                        colliderTriangles.AddRange(newTriangles);
                        colliderVertices.AddRange(currentVoxel.colliderVertices);
                    }
                }
            }
        }
        colliderMesh.Clear();
        colliderMesh.vertices = colliderVertices.ToArray();
        colliderMesh.triangles = colliderTriangles.ToArray();
        colliderMesh.RecalculateNormals();
        meshCollider.sharedMesh = colliderMesh;
    }

    public BlockMesh GetBlockMesh(int x, int y, int z)
    {
        if(meshType == 0)
        {
            if(voxels[x, y, z] != null)
            {
                return voxels[x, y, z];
            }
            else if (nonOpaqueMesh.GetBlockMesh(x, y, z) != null)
            {
                return nonOpaqueMesh.GetBlockMesh(x, y, z);
            }
            else if (nonConcreteMesh.GetBlockMesh(x, y, z) != null)
            {
                return nonConcreteMesh.GetBlockMesh(x, y, z);
            }
            else if (nonConcreteNonOpaqueMesh.GetBlockMesh(x, y, z) != null)
            {
                return nonConcreteNonOpaqueMesh.GetBlockMesh(x, y, z);
            }
            else return voxels[x, y, z];
        }
        return voxels[x, y, z];
    }

    public MiniBlockMesh GetMiniBlockMesh(int x, int y, int z)
    {
        if(meshType == 0)
        {
            if(miniVoxels[x, y, z] != null)
            {
                return miniVoxels[x, y, z];
            }
            else if (nonOpaqueMesh.GetMiniBlockMesh(x, y, z) != null)
            {
                return nonOpaqueMesh.GetMiniBlockMesh(x, y, z);
            }
            else if (nonConcreteMesh.GetMiniBlockMesh(x, y, z) != null)
            {
                return nonConcreteMesh.GetMiniBlockMesh(x, y, z);
            }
            else if (nonConcreteNonOpaqueMesh.GetMiniBlockMesh(x, y, z) != null)
            {
                return nonConcreteNonOpaqueMesh.GetMiniBlockMesh(x, y, z);
            }
            else return miniVoxels[x, y, z];
        }
        return miniVoxels[x, y, z];
    }

    public void UpdateAllMeshes()
    {
        UpdateMesh();
        UpdateColliderMesh();
        nonOpaqueMesh.UpdateMesh();
        nonOpaqueMesh.UpdateColliderMesh();
        nonConcreteMesh.UpdateMesh();
        nonConcreteNonOpaqueMesh.UpdateMesh();
    }
}