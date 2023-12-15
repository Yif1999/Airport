using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

public class ImagePublish : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    public string cameraTopicName = "/unity/image_raw/compressed";
    public string cameraTransformName = "/unity/image_raw/transform_matrix";

    // Message
    private CompressedImageMsg compressedImage;
    private string frameID = "fpv_cam";

    private RenderTexture camRenderTex;
    private Texture2D camTex2D;
    private Camera fpvCam;
    private Float64MultiArrayMsg camTransformMatrix;

    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<CompressedImageMsg>(cameraTopicName);
        ros.RegisterPublisher<Float64MultiArrayMsg>(cameraTransformName);

		// get render texture
		fpvCam = GetComponent<Camera>();
		camRenderTex = fpvCam.targetTexture;
        camTex2D = new Texture2D(camRenderTex.width, camRenderTex.height, TextureFormat.ARGB32, false);
        camTex2D.Apply();

        // Initialize messages
        camTransformMatrix = new Float64MultiArrayMsg();
        compressedImage = new CompressedImageMsg();
        compressedImage.header.frame_id = frameID;
        compressedImage.format = "jpg";
        
        // Start Coroutine
        StartCoroutine(ROSPublish());
    }
    
    IEnumerator ROSPublish()
    {
        while (true)
        {
            // Prepare Data
            RenderTexture.active = camRenderTex;
            camTex2D.ReadPixels(new Rect(0, 0, camTex2D.width, camTex2D.height), 0, 0);
            camTex2D.Apply();
            Matrix4x4 camToWorldMatrix = fpvCam.cameraToWorldMatrix;
            yield return null;
            
            // Encode and Publish Data
            compressedImage.data = camTex2D.EncodeToJPG();
            camTransformMatrix.data = new double[]
                {
                camToWorldMatrix.m00, camToWorldMatrix.m01, camToWorldMatrix.m02, camToWorldMatrix.m03,
                camToWorldMatrix.m20, camToWorldMatrix.m21, camToWorldMatrix.m22, camToWorldMatrix.m23,
                camToWorldMatrix.m10, camToWorldMatrix.m11, camToWorldMatrix.m12, camToWorldMatrix.m13,
                camToWorldMatrix.m30, camToWorldMatrix.m31, camToWorldMatrix.m32, camToWorldMatrix.m33
                };
            ros.Publish(cameraTopicName, compressedImage);
            ros.Publish(cameraTransformName, camTransformMatrix);
            yield return null;
        }
    }
    
}