using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    public Vector3Int currentPlayerChunkPosition;
    private Vector3Int currentChunkCenter = Vector3Int.zero;

    public World world;

    public float detectionTime = 1;

    internal void Initialize(GameObject player)
    {
        this.player = player;
        StartCheckingTheMap();
    }

    public void StartCheckingTheMap()
    {
        SetCurrentChunkCoordinates();
        StopAllCoroutines();
        StartCoroutine(CheckIfShouldLoadNextPosition());
    }

    IEnumerator CheckIfShouldLoadNextPosition()
    {
        yield return new WaitForSeconds(detectionTime);
        if (
            Mathf.Abs(currentChunkCenter.x - player.transform.position.x) > world.chunkSize ||
            Mathf.Abs(currentChunkCenter.y - player.transform.position.y) > world.chunkSize ||
            Mathf.Abs(currentChunkCenter.z - player.transform.position.z) > world.chunkSize
        )
        {
            world.LoadAdditionalChunksRequest(player);
        }
        else
        {
            StartCoroutine(CheckIfShouldLoadNextPosition());
        }
    }

    private void SetCurrentChunkCoordinates()
    {
        currentPlayerChunkPosition = WorldDataHelper.ChunkPositionFromBlockCoords(world, Vector3Int.RoundToInt(player.transform.position));
        currentChunkCenter.x = currentPlayerChunkPosition.x + world.chunkSize / 2;
        currentChunkCenter.y = currentPlayerChunkPosition.x + world.chunkSize / 2;
        currentChunkCenter.z = currentPlayerChunkPosition.z + world.chunkSize / 2;
    }
}
