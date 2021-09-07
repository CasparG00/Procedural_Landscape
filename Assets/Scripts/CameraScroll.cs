using UnityEngine;

public class CameraScroll : MonoBehaviour
{
    private void Update()
    {
        transform.position += Vector3.down * (2 * Time.deltaTime);
    }
}
