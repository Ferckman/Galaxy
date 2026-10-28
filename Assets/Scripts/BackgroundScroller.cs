using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] private float _scrollSpeed = 2f;
    [SerializeField] private float _resetPositionY = 20f;
    [SerializeField] private float _startPositionY = 20f;

    private void Update()
    {
        transform.Translate(Vector3.down * _scrollSpeed * Time.deltaTime);

        if (transform.position.y <= -_resetPositionY) transform.position = new Vector3(transform.position.x, _startPositionY, transform.position.z);
    }
}