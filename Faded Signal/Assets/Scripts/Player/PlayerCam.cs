using UnityEngine;

public class playerCam : MonoBehaviour
{
    //camera variables
    float minVerticalRotation = -90f;
    float maxVerticalRotation = 90f;

    [SerializeField] float sensX;
    [SerializeField] float sensY;

    [SerializeField] Transform playerOrientation;
    float cameraXRotation;
    float cameraYRotation;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseXInput = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseYInput = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        cameraYRotation += mouseXInput;
        cameraXRotation -= mouseYInput;

        cameraXRotation = Mathf.Clamp(cameraXRotation, minVerticalRotation, maxVerticalRotation);

        //rotate camera on x and y axis
        transform.rotation = Quaternion.Euler(cameraXRotation, cameraYRotation, 0);
        //rotate player on only y axis(horizontal)
        playerOrientation.rotation = Quaternion.Euler(0,cameraYRotation, 0);
    }
}
