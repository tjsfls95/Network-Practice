using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    Socket serverSocket = null;

    List<Socket> Connections = new List<Socket>();

    //Ŭ���̾�Ʈ�κ��� ���� ��Ŷ Ŭ������ ��� ���´�
    List<byte[]> Buffer = new List<byte[]>();

    ArrayList ByteBuffers = new ArrayList();

    public const int PortNum = 12345;

    private void Start()
    {
        Debug.Log("Server Start");
        this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //AddressFamily => ������ �ּ� �йи��� ������,AddressFamily.InterNetwork => IPv4
        //SocketType.Stream => TCP��� ProtocolType.Tcp => TCPŸ�� ����
        IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, PortNum); //��� ȣ��Ʈ�κ��� ��û���� �غ�

        this.serverSocket.Bind(ipLocal); //Ŭ���̾�Ʈ�κ��� ���� ������ ������ ��������Ʈ�� ���� -> ���ε�

        Debug.Log("Start Listening..");
        this.serverSocket.Listen(100); //��Ʈ�� ������� ���� Ŭ���̾�Ʈ�� ��û ��� -> ������, Ŭ���̾�Ʈ�� �ִ� ��(100)
    }

    private void SocketClose()
    {
        //��������
        if(this.serverSocket != null)
        {
            this.serverSocket.Close();
        }
        this.serverSocket = null;

        //Ŭ���̾�Ʈ ��������
        foreach(Socket client in this.Connections)
        {
            client.Close();
        }
        this.Connections.Clear();
    }

    private void OnApplicationQuit()
    {
        //�ý����� ����Ǹ� ������ Ŭ���̾�Ʈ ��� ����
        SocketClose();
    }

    private void Update()
    {
        List<Socket> listenList = new List<Socket>();
        listenList.Add(this.serverSocket);

        Socket.Select(listenList, null, null, 1000);
        //��� ���Ͽ� read,write,exception�� �߻��ߴ��� Ȯ���ϴ� �Լ�,��ȭ�� �߻��� ������ ��ũ���͸� 1�� ����

        //��ſ�û�� �ִٸ� listenList�� 0�� �ƴ�

        for(int i = 0;i<listenList.Count;i++)
        {
            //Accept
            Socket newConnection = ((Socket)listenList[i]).Accept();
            //������ ���������� �̷������ newConnection���� ���ο� ���� ��ũ�����̸�, �����ϸ� 0���� �������� ����

            //Ŭ���̾�Ʈ ������ ����
            this.Connections.Add(newConnection);

            Debug.Log("New Client Connected");
        }

        //������ ����� Ŭ���̾�Ʈ���� �ϳ��� ���� ���
        if(Connections.Count != 0)
        {
            //����� Ŭ���̾�Ʈ ���� ����
            List<Socket> cloneConnections = new List<Socket>(this.Connections);
            Socket.Select(cloneConnections, null, null, 1000);
            foreach(Socket client in cloneConnections)
            {
                byte[] receivedBytes = new byte[512];
                ArrayList buffer = (ArrayList)this.ByteBuffers[cloneConnections.IndexOf(client)];

                //Ŭ���̾�Ʈ�κ��� ���۵� ������ ���
                int read = client.Receive(receivedBytes);
                for(int i = 0; i<read; i++)
                {
                    buffer.Add(receivedBytes[i]);
                }

                while(buffer.Count > 0) //���۸� �� �о���϶�����
                {
                    //��Ŷ�� ù��°�� ������ ��ü �������� ũ��
                    int packetDataLength = (byte)buffer[0];
                    if(packetDataLength < buffer.Count)
                    {
                        ArrayList thisPacketBytes = new ArrayList(buffer);
                        //���� �޺κ� ����
                        thisPacketBytes.RemoveRange(packetDataLength, thisPacketBytes.Count - (packetDataLength + 1));
                    }
                }
            }
        }
    }
}
