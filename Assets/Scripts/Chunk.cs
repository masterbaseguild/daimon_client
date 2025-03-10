// a chunk is a 16x16x16 grid of voxels
// each voxel is stored as a number that maps to a block type in the block palette
public class Chunk
{
    public static int CHUNK_SIZE = 16;
    int[,,] voxels = new int[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];

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

    public int getVoxel(int x, int y, int z)
    {
        return voxels[x, y, z];
    }

    public void setVoxel(int x, int y, int z, int voxel)
    {
        voxels[x, y, z] = voxel;
    }
}