using UnityEngine;
using UnityEngine.UI;

public class ChooseAvatarManager : MonoBehaviour
{
    [SerializeField] private Button[] _avatarButtons;
    [SerializeField] private Image[] _underlines;
    [SerializeField] private Sprite[] _spritesForAvatar;
    [SerializeField] private Button _chooseButton;
    [SerializeField] private MainMenuManager _mainMenuManager;
    [SerializeField] private Image[] _avatars;
    private int _selectedAvatarIndex = -1;
    [SerializeField] private Player _player;

    private void Start()
    {
        _selectedAvatarIndex = 0;

        PlayerPrefs.SetInt("SelectedAvatar", _selectedAvatarIndex);
        PlayerPrefs.Save();

        foreach (Image avatar in _avatars)
        {
            avatar.sprite = _spritesForAvatar[_selectedAvatarIndex];
        }
        foreach (var underline in _underlines)
        {
            underline.gameObject.SetActive(false);
        }

        for (int i = 0; i < _avatarButtons.Length; i++)
        {
            int index = i;
            _avatarButtons[i].onClick.AddListener(() => SelectAvatar(index));
        }

        _chooseButton.onClick.AddListener(ChooseAvatar);
    }

    private void SelectAvatar(int index)
    {
        // Remove previous selection
        foreach (var underline in _underlines)
        {
            underline.gameObject.SetActive(false);
        }

        // Show the underline for the selected avatar
        _underlines[index].gameObject.SetActive(true);
        _selectedAvatarIndex = index;
    }

    private void ChooseAvatar()
    {
        if (_selectedAvatarIndex == -1)
        {
            Debug.LogWarning("No avatar selected!");
            return;
        }

        // Save selected avatar for use in the game
        PlayerPrefs.SetInt("SelectedAvatar", _selectedAvatarIndex);
        PlayerPrefs.Save();

        foreach (Image avatar in _avatars)
        {
            avatar.sprite = _spritesForAvatar[_selectedAvatarIndex];
        }

        foreach (var player in FindObjectsOfType<Player>())
        {
            player.SaveHostIndex(_selectedAvatarIndex);
        }

        _mainMenuManager.BackToMain();
    }
}

