using UnityEngine;

// every other user
public class User : MonoBehaviour
{
    public string username;
    public int index;
    private Transform usernameText;

    private void Start()
    {
        // Set the username text to the username of the user
        usernameText = transform.Find("Username");
        usernameText.GetComponent<TextMesh>().text = username;
    }

    void Update()
    {
        // Rotate the username to match the camera rotation
        if (Camera.main != null)
        {
            // set the username rotation to be the same as the camera
            usernameText.rotation = Camera.main.transform.rotation;
        }
    }
}