using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Move Setting"), Space(10)]
    [SerializeField] private Transform CameraTransform = null;

    [SerializeField] private float MoveScale = 1f;
    [SerializeField] private float RotateScale = 1f;

    [Header("Generate Setting"), Space(10)]
    [SerializeField] private Dragger WallGenerator = null;

    private void Update()
    {
        MoveCamera();

        if (Input.GetKeyDown(KeyCode.Alpha1))
            WallGenerator.StartInstallWall();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            WallGenerator.StartRemoveWall();
    }

    private void MoveCamera()
    {
        var horizontal = Input.GetAxis("Horizontal") * MoveScale;
        var vertical = Input.GetAxis("Vertical") * MoveScale;

        var vector = transform.forward * vertical + transform.right * horizontal;
        vector.y = 0;
        transform.position += vector;

        if (Input.GetMouseButton(1))
        {
            var rotateY = Input.GetAxis("Mouse X") * RotateScale;
            var rotateX = Input.GetAxis("Mouse Y") * RotateScale;

            CameraTransform.Rotate(-rotateX, 0, 0);
            transform.Rotate(0, rotateY, 0);
        }
    }
}
