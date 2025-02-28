using UnityEngine;
using System.Collections.Generic;

public class People : MonoBehaviour
{
    static List<GameObject> connectedUsers = new List<GameObject>();
    static GameObject userPrefab;
    public GameObject userPrefabObject;

    void Start()
    {
        userPrefab = userPrefabObject;
    }

    public static int GetCount()
    {
        return connectedUsers.Count + 1;
    }

    public static void setPosition(int index, float x, float y, float z, float rx, float ry, float rz, float cx)
    {
        GameObject user = connectedUsers.Find(user => user.GetComponent<User>().index == index);
        if(user == null)
        {
            return;
        }
        user.transform.position = new Vector3(x, y, z);
        user.transform.rotation = Quaternion.Euler(rx, ry, rz);
        GameObject camera = user.transform.GetChild(0).gameObject;
        camera.transform.rotation = Quaternion.Euler(cx, ry, 0);
    }

    public static GameObject GetUserGameObject(int index)
    {
        return connectedUsers.Find(user => user.GetComponent<User>().index == index);
    }

    public static void AddUser(int index, string username)
    {
        GameObject user = Instantiate(userPrefab);
        user.transform.parent = GameObject.Find("People").transform;
        user.name = "User" + index;
        user.GetComponent<User>().index = index;
        user.GetComponent<User>().username = username;
        connectedUsers.Add(user);
    }

    public static void RemoveUser(int index)
    {
        GameObject user = connectedUsers.Find(user => user.GetComponent<User>().index == index);
        if(user != null)
        {
            connectedUsers.Remove(user);
            Destroy(user);
        }
    }

    public static List<GameObject> GetUsers()
    {
        return connectedUsers;
    }
}