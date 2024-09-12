using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class networkmanagerui : MonoBehaviour
{
    [SerializeField] private Button serverBtn;
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private TMP_InputField ipInput;

    private void Awake()
    {
        /* serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        }); */
        hostBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
        });
        clientBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
        });
        ipInput.onValueChanged.AddListener(delegate{editText();});
    }

    private void editText()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Address=ipInput.text;
    }
}
