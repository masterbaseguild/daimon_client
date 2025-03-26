using UnityEngine;
using System.Collections.Generic;

// the people class stores and manages the data of all other connected users
public class People : MonoBehaviour
{
    [SerializeField] private GameObject userPrefab;
    private readonly List<GameObject> connectedUsers = new();

    public int GetCount()
    {
        return connectedUsers.Count + 1;
    }

    public void SetPosition(int index, float x, float y, float z, float rx, float ry, float rz, float cx)
    {
        GameObject user = connectedUsers.Find(user => user.GetComponent<User>().index == index);
        if (user == null)
        {
            return;
        }
        user.transform.SetPositionAndRotation(new Vector3(x, y, z), Quaternion.Euler(rx, ry, rz));

        GameObject camera = user.transform.GetChild(0).gameObject;
        camera.transform.rotation = Quaternion.Euler(cx, ry, 0);
    }

    public GameObject GetUserGameObject(int index)
    {
        return connectedUsers.Find(user => user.GetComponent<User>().index == index);
    }

    public void AddUser(int index, string username)
    {
        GameObject user = Instantiate(userPrefab);
        user.transform.parent = GameObject.Find("People").transform;
        user.name = "User" + index;
        user.GetComponent<User>().index = index;
        user.GetComponent<User>().username = username;
        connectedUsers.Add(user);
    }

    public void RemoveUser(int index)
    {
        GameObject user = connectedUsers.Find(user => user.GetComponent<User>().index == index);
        if (user != null)
        {
            _ = connectedUsers.Remove(user);
            Destroy(user);
        }
    }

    public List<GameObject> GetUsers()
    {
        return connectedUsers;
    }
}