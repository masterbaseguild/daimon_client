// a chunk is a 16x16x16 grid of voxels
// each voxel is stored as a number that maps to a block type in the block palette
public class Chunk
{
    public static readonly int CHUNK_SIZE = 16;
    private readonly int[,,] voxels = new int[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];

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
    }

    public int GetVoxel(int x, int y, int z)
    {
        int voxelX = x % CHUNK_SIZE;
        int voxelY = y % CHUNK_SIZE;
        int voxelZ = z % CHUNK_SIZE;
        return voxels[voxelX, voxelY, voxelZ];
    }

    public void SetVoxel(int x, int y, int z, int block)
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
}