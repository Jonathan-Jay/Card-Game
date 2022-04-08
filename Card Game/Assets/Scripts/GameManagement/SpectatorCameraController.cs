using System;
using UnityEngine;

public class SpectatorCameraController : MonoBehaviour {
    public Transform following;
    public Vector3 orginOffset = Vector3.zero;
    public float sensitivity = 1f;
    public Vector2 rotXMinMax = new Vector2(310f, 450f);

    private Camera cam;

    // Start is called before the first frame update
    void Start() {
        cam = GetComponent<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    // LateUpdate is called once per frame at the end of everything
    void LateUpdate() {
        if (following == null)
            return;

        if (Cursor.lockState == CursorLockMode.Locked) {
            Vector2 rotInp = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            float rotMod = transform.rotation.eulerAngles.x <= 90f ? transform.rotation.eulerAngles.x + 360f : transform.rotation.eulerAngles.x;
            transform.rotation = Quaternion.Euler(Mathf.Clamp(rotMod - rotInp.y * sensitivity, rotXMinMax.x, rotXMinMax.y), transform.rotation.eulerAngles.y + rotInp.x * sensitivity, 0);

            if (Input.GetKeyDown(KeyCode.Escape))
                Cursor.lockState = CursorLockMode.None;
        } else if (Input.GetMouseButtonDown(0))
            Cursor.lockState = CursorLockMode.Locked;

        following.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        transform.position = following.position + orginOffset;
    }
}
