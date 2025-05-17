// a chunk is a 16x16x16 grid of voxels
// each voxel is stored as a number that maps to a block type in the block palette
using UnityEngine;

public class Chunk
{
    public static readonly int CHUNK_SIZE = 16;
    private readonly int[,,] voxels = new int[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
    

    //Matr Livelli di luce per ogni Voxel
    private readonly byte[,,] lightLevels = new byte[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
    private BlockPalette blockPalette;

    private static readonly Vector3Int[] Directions = new Vector3Int[]
    {
        new Vector3Int(1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 0, -1),
    };


    public Chunk()
    {
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    voxels[x, y, z] = 0;
                }
            }
        }
        

        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    lightLevels[x, y, z] = 0; //inizializzo la luce a 0
                }
            }
        }

        //Inizializzo luce top down che si propaga tramite blocchi trasparenti
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int z = 0; z < CHUNK_SIZE; z++)
            {
                byte sunlight = 15;

                for (int y = CHUNK_SIZE - 1; y >= 0; y--)
                {
                    int blockType = voxels[x, y, z];
                    bool transparent = blockPalette.Instance.IsTransparent(blockType);

                    if (!transparent)
                    {
                        sunlight = 0; // blocca la luce
                    }

                    SetLight(x, y, z, sunlight);

                    if (sunlight > 1 && transparent)
                        PropagateLight(x, y, z, sunlight);
                }
            }
        }


    }

    public int getVoxel(int x, int y, int z)
    {
        int voxelX = x % CHUNK_SIZE;
        int voxelY = y % CHUNK_SIZE;
        int voxelZ = z % CHUNK_SIZE;
        return voxels[voxelX, voxelY, voxelZ];
    }

  
    public void setVoxel(int x, int y, int z, int block)
    {
        int voxelX = x % CHUNK_SIZE;
        int voxelY = y % CHUNK_SIZE;
        int voxelZ = z % CHUNK_SIZE;
        voxels[voxelX, voxelY, voxelZ] = block;
    }



    public bool IsEmpty()
    {
        for (int x = 0; x < CHUNK_SIZE; x++)
        {
            for (int y = 0; y < CHUNK_SIZE; y++)
            {
                for (int z = 0; z < CHUNK_SIZE; z++)
                {
                    if (voxels[x, y, z] != 0)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }


    //Getter e setter luce
    public byte GetLight(int x, int y, int z)
    {
        int lx = x % CHUNK_SIZE;
        int ly = y % CHUNK_SIZE;
        int lz = z % CHUNK_SIZE;
        return lightLevels[lx, ly, lz];
    }

    public void SetLight(int x, int y, int z, byte light)
    {
        int lx = x % CHUNK_SIZE;
        int ly = y % CHUNK_SIZE;
        int lz = z % CHUNK_SIZE;
        lightLevels[lx, ly, lz] = light;
    }


    //Propago la luce all'interno del chunk tramite un algoritmo floodfiller
    public void PropagateLight(int startX, int startY, int startZ, byte initialLight)
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        queue.Enqueue(new Vector3Int(startX, startY, startZ));
        SetLight(startX, startY, startZ, initialLight);

        while (queue.Count > 0)
        {
            Vector3Int pos = queue.Dequeue();
            byte currentLight = GetLight(pos.x, pos.y, pos.z);

            foreach (Vector3Int dir in Directions)
            {
                int nx = pos.x + dir.x;
                int ny = pos.y + dir.y;
                int nz = pos.z + dir.z;

                if (!InBounds(nx, ny, nz))
                    continue;

                byte neighborLight = GetLight(nx, ny, nz);
                int blockType = voxels[nx, ny, nz];
                //bool transparent = BlockPalette.IsTransparent(blockType);

                if (transparent && neighborLight + 2 <= currentLight)
                {
                    SetLight(nx, ny, nz, (byte)(currentLight - 1));
                    queue.Enqueue(new Vector3Int(nx, ny, nz));
                }
            }
        }
    }

    private bool InBounds(int x, int y, int z)
    {
        return x >= 0 && x < CHUNK_SIZE && y >= 0 && y < CHUNK_SIZE && z >= 0 && z < CHUNK_SIZE;
    }




    private bool IsTransparent(int blockId)
    {
        BlockType t = blockPalette.GetBlockType(blockId);
        return blockType != null && blockType.transparent();
    }



}