using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

public class ImagePublish : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;
    public string cameraTopicName = "unity/image_raw/compressed";

    // Setting
    public int qualityLevel = 50;

    // Message
    private CompressedImageMsg compressedImage;
    private string frameID = "fpv_cam";

    private RenderTexture camRenderTex;
    private Texture2D camTex2D;

    void Start()
    {
        // start the ROS connection
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<CompressedImageMsg>(cameraTopicName);

		// get render texture
		Camera fpvCam = GetComponent<Camera>();
		camRenderTex = fpvCam.targetTexture;
        camTex2D = new Texture2D(camRenderTex.width, camRenderTex.height, TextureFormat.ARGB32, false);
        camTex2D.Apply();

        // Initialize messages
        compressedImage = new CompressedImageMsg();
        compressedImage.header.frame_id = frameID;
        compressedImage.format = "jpeg";
    }

    private void Update()
    {
        RenderTexture.active = camRenderTex;
        camTex2D.ReadPixels(new Rect(0,0,camTex2D.width,camTex2D.height),0,0);
        camTex2D.Apply();
        compressedImage.data = camTex2D.EncodeToJPG(qualityLevel);
        ros.Publish(cameraTopicName, compressedImage);
    }
}