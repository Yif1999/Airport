using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraControl : MonoBehaviour
{
    public float moveSpeed = 10f; // 相机移动速度
    public float rotationSpeed = 5f; // 视角旋转速度

    private float pitch = 0f; // 上下旋转角度
    private float yaw = 0f;  // 左右旋转角度

    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        // 获取WASD输入
        float horizontal = Input.GetAxis("Horizontal"); // A和D键
        float vertical = Input.GetAxis("Vertical");   // W和S键
        
        // 获取E和Q输入用于上升和下降
        float verticalMovement = 0f;
        if (Input.GetKey(KeyCode.E))
        {
            verticalMovement = 1f;
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            verticalMovement = -1f;
        }

        // 移动相机
        Vector3 direction = transform.right * horizontal + transform.forward * vertical + transform.up * verticalMovement;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void HandleRotation()
    {
        // 检测鼠标右键是否按下
        if (Input.GetMouseButton(1))
        {
            // 获取鼠标移动的增量
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // 更新角度
            yaw += mouseX * rotationSpeed;
            pitch -= mouseY * rotationSpeed;

            // 限制俯仰角度范围
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            // 应用旋转
            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }
    }

}