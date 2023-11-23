using System;
using System.Net.Sockets;
using UnityEngine;
using Google.Protobuf;
using System.Net;
using System.Threading;
using ClientMsg = NeurAR.Client;
using CamMsg = NeurAR.Camera;

public class TestScript : MonoBehaviour
{
    public Camera mainCam;
    private CamMsg _camMsg;
    private Socket _client;
    private Thread _thread;
    private System.Numerics.Vector3 _camPosition;
    
    // Start is called before the first frame update
    private void Start()
    {
        try
        {
            var pAddress = IPAddress.Parse("127.0.0.1");
            var pEndPoint = new IPEndPoint(pAddress, 8080);
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _client.Connect(pEndPoint);
            Debug.Log("连接成功");
            //创建线程，执行读取服务器消息
            _thread = new Thread(Received)
            {
                IsBackground = true
            };
            _thread.Start();
        }
        catch (Exception)
        {
            Debug.Log("未能连接");
        }
        
    }

    private void OnApplicationQuit()
    {
        ClientClose();
        _thread.Abort();
        _thread.Join();
    }

    // Update is called once per frame
    private void Update()
    {
        gameObject.transform.Rotate(0, 1, 0);
        mainCam.transform.localEulerAngles = new Vector3(0,0,_camPosition.Z);
    }

    private void Received()
    {
        
        var clientMsg = new ClientMsg
        {
            Timestamp = GetTimeStamp(),
            Type = ClientMsg.Types.ClientType.Unity
        };
        var clientData = clientMsg.ToByteArray();
        var dataLength = BitConverter.GetBytes(clientData.Length);
        Debug.Log(clientData.Length);
        _client.Send(dataLength);
        _client.Send(clientData);
        Debug.Log("Sent type message.");
        
        while (true)
        {

            var bufferLength = new byte[4];
            _client.Receive(bufferLength);
            var length = BitConverter.ToInt32(bufferLength, 0);
            var buffer = new byte[length];
            var len = _client.Receive(buffer);
            _camMsg = NeurAR.Camera.Parser.ParseFrom(buffer, 0, len);
            Debug.Log("来自服务器：" + _camMsg.Rotation.Z);
            _camPosition = new System.Numerics.Vector3(0,0,_camMsg.Rotation.Z);

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
}