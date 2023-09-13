using System;
using System.Net.Sockets;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(0, 1, 0);

        // stream.Write(data, 0, data.Length);  // 发送数据
        //TcpClient client = new TcpClient("127.0.0.1", 8080);
        //NetworkStream stream = client.GetStream();

        byte[] data = new byte[1024];
        //int bytes = stream.Read(data, 0, data.Length);  // 接收数据
        //string response = System.Text.Encoding.UTF8.GetString(data, 0, bytes);
        //Debug.Log(response);

        //stream.Close();
        //client.Close();
    }
}
