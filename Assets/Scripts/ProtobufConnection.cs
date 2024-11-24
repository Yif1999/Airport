using System;
using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using Google.Protobuf;
using System.Net;
using System.Threading;
using ClientMsg = NeurAR.Client;
using CamMsg = NeurAR.Camera;
using HeadMsg = NeurAR.Head;
using ImageMsg = NeurAR.Image;

public enum CameraState
{
    Idle,
    WaitingForMove,
    WaitingForSend,
}

public class ProtobufConnection : MonoBehaviour
{
    public Camera mainCam;
    private CamMsg _camMsg;
    private Socket _client;
    private Thread _thread_recv;
    private Thread _thread_send;
    private CamMsg.Types.Vector3 _camPosition;
    private CamMsg.Types.Vector3 _camRotation; 
    private int _recvCount;
    private int _sendCount;
    private RenderTexture _camRenderTex;
    private Texture2D _camTex2D;
    private byte[] _encodedImage;
    private CameraState _camState;
    
    // Start is called before the first frame update
    private void Start()
    {
        _recvCount = 0;
        _sendCount = 0;
        _camPosition = new CamMsg.Types.Vector3();
        _camRotation = new CamMsg.Types.Vector3();
        _camState = CameraState.Idle;
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
            
            //创建线程，执行读取服务器消息
            _thread_recv = new Thread(Receive)
            {
                IsBackground = true
            };
            _thread_recv.Start();            
            _thread_send = new Thread(Send)
            {
                IsBackground = true
            };
            _thread_send.Start();
        }
        catch (Exception)
        {
            Debug.Log("未能连接");
        }
        
    }

    private void OnApplicationQuit()
    {
        ClientClose();
        _thread_recv.Abort();
        _thread_recv.Join();   
        _thread_send.Abort();
        _thread_send.Join();
    }

    // Update is called once per frame
    private void Update()
    {
        RenderTexture.active = _camRenderTex;
        _camTex2D.ReadPixels(new Rect(0, 0, _camRenderTex.width, _camRenderTex.height), 0, 0);
        _camTex2D.Apply();
        _encodedImage = _camTex2D.EncodeToPNG();
        
        if (_camState == CameraState.WaitingForMove)
        {
            mainCam.transform.position = new Vector3(_camPosition.X, _camPosition.Y, _camPosition.Z);
            mainCam.transform.rotation = Quaternion.Euler(_camRotation.X, _camRotation.Y, _camRotation.Z);
            _camState = CameraState.Idle;
            StartCoroutine(SetCameraState());
        }
    }

    IEnumerator SetCameraState()
    {
        yield return null;
        _camState = CameraState.WaitingForSend;
    }

    private void Receive()
    {
        while (true)
        {
            Thread.Sleep(100);
            _recvCount++;
            var bufferLength = new byte[4];
            _client.Receive(bufferLength);
            var length = BitConverter.ToInt32(bufferLength, 0);
            if (length == 0)
            {
                continue;
            }
            var buffer = new byte[length];
            var len = _client.Receive(buffer);
            _camMsg = NeurAR.Camera.Parser.ParseFrom(buffer, 0, len);
            _camPosition = _camMsg.Position;
            _camRotation = _camMsg.Rotation;
            _camState = CameraState.WaitingForMove;
            if (_recvCount < 61)
            {
                Debug.Log($"[Client] Prewarm capturing {_recvCount}/60...");
            }
        }
    }

    private void Send()
    {
        while (true)
        {
            Thread.Sleep(100);
            if (_camState == CameraState.WaitingForSend)
            {
                var imageMsg = new ImageMsg
                {
                    Timestamp = -1,
                    Data = ByteString.CopyFrom(_encodedImage),
                };
                var imageData = imageMsg.ToByteArray();

                var headMsg = new HeadMsg
                {
                    Timestamp = GetTimeStamp(),
                    Typeid = (int)ClientMsg.Types.ClientType.Unity,
                    PayloadLength = imageData.Length,
                };
                var headData = headMsg.ToByteArray();
                
                var lenBuf = BitConverter.GetBytes(headData.Length);
                Array.Reverse(lenBuf);
                _client.Send(lenBuf);
                _client.Send(headData);
                SendDataInChunks(_client, imageData);
                
                _sendCount++;
                if (_sendCount > 60)
                {
                    Debug.Log($"[Client] Autonomically Captured {_sendCount - 60} images.");
                }
                _camState = CameraState.Idle;
            }
        }
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

    private static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalMilliseconds);
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
}