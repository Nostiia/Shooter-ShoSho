using Fusion;
using UnityEngine;

public class PlayerAvatar : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer _bodyRenderer;
    [SerializeField] private Sprite[] _avatarSprites;
    [SerializeField] private AnimatorOverrideController[] _animatiaons;
    [SerializeField] private Animator _animator;

    [Networked] private int _selectedAvatarIndex { get; set; }

    public void InitializeAvatar(int avatarIndex)
    {
        if (avatarIndex >= 0 && avatarIndex < _avatarSprites.Length)
        {
            _selectedAvatarIndex = avatarIndex;
            UpdateAvatar(_selectedAvatarIndex);
        }
    }

    public void UpdateAvatar(int avatarIndex)
    {
        if (avatarIndex >= 0 && avatarIndex < _avatarSprites.Length)
        {
            _bodyRenderer.sprite = _avatarSprites[avatarIndex];
            if (avatarIndex < _animatiaons.Length)
            {
                _animator.runtimeAnimatorController = _animatiaons[avatarIndex];
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_UpdateAvatar(int avatarIndex)
    {
        UpdateAvatar(avatarIndex);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SelectAvatar(int avatarIndex)
    {
        if (Object.HasStateAuthority)
        {
            _selectedAvatarIndex = avatarIndex;
            RPC_UpdateAvatar(avatarIndex);
        }
    }

    public int GetSelectedAvatarIndex()
    {
        return _selectedAvatarIndex;
    }
}
