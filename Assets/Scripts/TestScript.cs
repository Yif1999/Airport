using System;
using System.Net.Sockets;
using UnityEngine;
using Tutorial;
using Google.Protobuf;
using System.Net;
using System.Threading;
using System.Text;

public class TestScript : MonoBehaviour
{

    static Socket client;
    // Start is called before the first frame update
    void Start()
    {

        try
        {
             IPAddress pAddress = IPAddress.Parse("127.0.0.1");
             IPEndPoint pEndPoint = new IPEndPoint(pAddress, 9999);
             client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
             client.Connect(pEndPoint);
             Debug.Log("���ӳɹ�");
             //�����̣߳�ִ�ж�ȡ��������Ϣ
             Thread c_thread = new Thread(Received);
             c_thread.IsBackground = true;
             c_thread.Start();
        }
        catch (System.Exception)
        {
             Debug.Log("δ������");
        }
        stream.Close();
        client.Close();

    }

    // Update is called once per frame
    void Update()
    {

        gameObject.transform.Rotate(0, 1, 0);

    }

    static void ListPeople(AddressBook addressBook)
    {
        foreach (Person person in addressBook.People)
        {
            Debug.Log("Person ID: " + person.Id);
            Debug.Log("  Name: " + person.Name);
            Debug.Log("  E-mail address: " + person.Email);

        }
    }

    public static void Received()
    {
        while (true)
        {
            try
            {
                byte[] buffer = new byte[1000];
                int len = client.Receive(buffer);
                if (len == 0) break;
                AddressBook addressBook = AddressBook.Parser.ParseFrom(buffer,0,len);
                Debug.Log("���Է�������" + addressBook.People[0].Name);
            }
           catch (System.Exception)
            {
    
                throw;
            }

        }
    }

    public static void close()
    {
        try
        {
            client.Close();
            Debug.Log("�رտͻ�������");
        }
        catch (System.Exception)
        {
             Debug.Log("δ����");
        }
    }
}
