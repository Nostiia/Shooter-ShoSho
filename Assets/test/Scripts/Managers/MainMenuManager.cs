using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _chooseAvatarCanvas;
    [SerializeField] private GameObject _mainMenuCanvas;
    [SerializeField] private GameObject _createRoomPanel;
    [SerializeField] private GameObject _connectRoomPanel;
    private void Start()
    {
        _chooseAvatarCanvas.SetActive(false);
        _createRoomPanel.SetActive(false);
        _connectRoomPanel.SetActive(false);
        _mainMenuCanvas.SetActive(true);
    }
    public void CreateRoom()
    {
        _mainMenuCanvas.SetActive(false);
        _createRoomPanel.SetActive(true);
    }

    public void Connect()
    {
        _mainMenuCanvas.SetActive(false);
        _connectRoomPanel.SetActive(true);
    }

    public void ChooseAvatar()
    {
        _chooseAvatarCanvas.SetActive(true);
        _mainMenuCanvas.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ActivateMainMenu()
    {
        _mainMenuCanvas.SetActive(true);
    }

    public void BackToMain()
    {
        ActivateMainMenu();
        _chooseAvatarCanvas.SetActive(false);
        _createRoomPanel.SetActive(false);
        _connectRoomPanel.SetActive(false);
    }

    public void BackOfLoading()
    {
        NetworkRunner runner = FindObjectOfType<NetworkRunner>();
        if (runner != null)
        {
            runner.Shutdown();
        }

        SceneManager.LoadScene("SampleScene");
    }
}
