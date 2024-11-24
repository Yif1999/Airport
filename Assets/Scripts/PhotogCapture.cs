using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Google.Protobuf;
using Camera = UnityEngine.Camera;
using CamMsg = NeurAR.Camera;
using HeadMsg = NeurAR.Head;
using FrameMsg = NeurAR.Frame;
using ClientMsg = NeurAR.Client;

public class PhotogCapture : MonoBehaviour
{
    private Camera mainCam;
    private CamMsg _camMsg;
    private Socket _client;
    private CameraState _camState;
    private int _sendCount;
    private byte[] _encodedImage;
    private RenderTexture _camRenderTex;
    private Texture2D _camTex2D;

    private void Start()
    {
        _sendCount = 0;
        // 获取主相机
        mainCam = GetComponent<Camera>();
        if (mainCam == null)
        {
            Debug.LogError("No camera found on this GameObject.");
        }
        _camRenderTex = mainCam.targetTexture;
        _camTex2D = new Texture2D(_camRenderTex.width, _camRenderTex.height, TextureFormat.RGBAHalf, false);
        _camTex2D.Apply();
        _encodedImage = new byte[0];
        
        try
        {
            var pAddress = IPAddress.Parse("127.0.0.1");
            var pEndPoint = new IPEndPoint(pAddress, 1897);
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _client.Connect(pEndPoint);
            if (_client.Connected)
            {
                Debug.Log("连接成功");
            }
        }
        catch (Exception)
        {
            Debug.Log("未能连接");
        }
    }

    private void Update()
    {
        // 检测按下空格键来捕获屏幕截图
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CaptureScreenshot();
        }
    }

    private void CaptureScreenshot()
    {
        RenderTexture.active = _camRenderTex;
        _camTex2D.ReadPixels(new Rect(0, 0, _camRenderTex.width, _camRenderTex.height), 0, 0);
        _camTex2D.Apply();
        _encodedImage = _camTex2D.EncodeToPNG();
        Matrix4x4 camToWorldMatrix = mainCam.cameraToWorldMatrix;
        var tfmData = new double[]
        {
            camToWorldMatrix.m00, camToWorldMatrix.m01, camToWorldMatrix.m02, camToWorldMatrix.m03,
            camToWorldMatrix.m20, camToWorldMatrix.m21, camToWorldMatrix.m22, camToWorldMatrix.m23,
            camToWorldMatrix.m10, camToWorldMatrix.m11, camToWorldMatrix.m12, camToWorldMatrix.m13,
            camToWorldMatrix.m30, camToWorldMatrix.m31, camToWorldMatrix.m32, camToWorldMatrix.m33
        };

        var imageMsg = new FrameMsg.Types.Image
        {
            Width = 512,
            Height = 512,
            Data = ByteString.CopyFrom(_encodedImage),
        };

        var tfmMsg = new FrameMsg.Types.TransformMatrix
        {
            Data = {tfmData},
        };

        var frameMsg = new FrameMsg
        {
            Image = imageMsg,
            Tfm = tfmMsg,
        };
        var frameData = frameMsg.ToByteArray();

        var headMsg = new HeadMsg
        {
            Timestamp = GetTimeStamp(),
            Typeid = 2, // No define in protobuf
            PayloadLength = frameData.Length,
        };
        var headData = headMsg.ToByteArray();
                
        var lenBuf = BitConverter.GetBytes(headData.Length);
        Array.Reverse(lenBuf);
        _client.Send(lenBuf);
        _client.Send(headData);
        SendDataInChunks(_client, frameData);
                
        _sendCount++;
        Debug.Log($"[Client] Autonomically Captured {_sendCount} images.");
        _camState = CameraState.Idle;
    }
    
    private static void SendDataInChunks(Socket sock, byte[] data, int chunkSize = 1024)
    {
        int totalSize = data.Length;
        int sentSize = 0;

        while (sentSize < totalSize)
        {
            // 计算当前块的结束索引
            int endIndex = Math.Min(sentSize + chunkSize, totalSize);
            byte[] chunk = new byte[endIndex - sentSize];
            Array.Copy(data, sentSize, chunk, 0, chunk.Length);

            // 发送当前数据块
            sock.Send(chunk);
            sentSize += chunk.Length;
        }
    }
    
    private static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalMilliseconds);
    }  
    
    private void ClientClose()
    {
        try
        {
            _client.Close();
            Debug.Log("关闭客户端连接");
        }
        catch (Exception)
        { 
            Debug.Log("连接已提前中断");
        }
    }
    
}
