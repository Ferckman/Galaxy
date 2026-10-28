using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _anim;

    private void Awake()
    {
        if (_anim == null) _anim = GetComponent<Animator>();
    }

    public void UpdateAnimation(float horizontalInput)
    {
        if (_anim == null) return;

        _anim.SetBool("Turn_Left", horizontalInput < 0f);
        _anim.SetBool("Turn_Right", horizontalInput > 0f);
    }
}