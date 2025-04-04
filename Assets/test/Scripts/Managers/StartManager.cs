using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartManager : MonoBehaviour
{
    [SerializeField] private BasicSpawner _spawner;
    [SerializeField] private Player _player;
    [SerializeField] private GameObject _startButton;
    [SerializeField] private GameObject _startCanvas;

    private const string _sceneName = "MainScene";
    private void Start()
    {
        _spawner = FindObjectOfType<BasicSpawner>();
        _startButton.SetActive(false);
    }

    private void Update()
    {
        if ( _spawner != null  &&  _spawner.PlayerCount() == 2 && _spawner.IsPlayerHost())
        {
            _startButton.SetActive(true);
        }
    }

    public void StartGame()
    {
        _startCanvas.SetActive(false);
        foreach (var player in FindObjectsOfType<Player>())
        {
            player.GetComponent<PlayerMovement>().RPC_CanMove(true);
        }
        FindObjectOfType<TimerManager>().StartGame();
    }

    public void BackToMain()
    {
        NetworkRunner runner = FindObjectOfType<NetworkRunner>(); 
        if (runner != null)
        {
            runner.Shutdown(); 
        }

        SceneManager.LoadScene(_sceneName);
    }
}
