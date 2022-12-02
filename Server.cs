using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    Socket serverSocket = null;

    List<Socket> Connections = new List<Socket>();

    //클라이언트로부터 받은 패킷 클래스를 담아 놓는다
    List<byte[]> Buffer = new List<byte[]>();

    ArrayList ByteBuffers = new ArrayList();

    public const int PortNum = 12345;

    private void Start()
    {
        Debug.Log("Server Start");
        this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //AddressFamily => 소켓의 주소 패밀리를 가져옴,AddressFamily.InterNetwork => IPv4
        //SocketType.Stream => TCP방식 ProtocolType.Tcp => TCP타입 소켓
        IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, PortNum); //모든 호스트로부터 요청받을 준비

        this.serverSocket.Bind(ipLocal); //클라이언트로부터 받은 소켓을 로컬의 엔드포인트에 연결 -> 바인딩

        Debug.Log("Start Listening..");
        this.serverSocket.Listen(100); //포트를 열어놓은 다음 클라이언트의 요청 대기 -> 리스닝, 클라이언트의 최대 수(100)
    }

    private void SocketClose()
    {
        //서버종료
        if(this.serverSocket != null)
        {
            this.serverSocket.Close();
        }
        this.serverSocket = null;

        //클라이언트 접속종료
        foreach(Socket client in this.Connections)
        {
            client.Close();
        }
        this.Connections.Clear();
    }

    private void OnApplicationQuit()
    {
        //시스템이 종료되면 서버와 클라이언트 모두 종료
        SocketClose();
    }

    private void Update()
    {
        List<Socket> listenList = new List<Socket>();
        listenList.Add(this.serverSocket);

        Socket.Select(listenList, null, null, 1000);
        //어느 소켓에 read,write,exception이 발생했는지 확인하는 함수,변화가 발생한 소켓의 디스크립터만 1로 변경

        //통신요청이 있다면 listenList는 0이 아님

        for(int i = 0;i<listenList.Count;i++)
        {
            //Accept
            Socket newConnection = ((Socket)listenList[i]).Accept();
            //연결이 성공적으로 이루어지면 newConnection값은 새로운 소켓 디스크립터이며, 실패하면 0보다 작은값이 리턴

            //클라이언트 소켓을 저장
            this.Connections.Add(newConnection);

            Debug.Log("New Client Connected");
        }

        //서버와 연결된 클라이언트들이 하나라도 있을 경우
        if(Connections.Count != 0)
        {
            //연결된 클라이언트 소켓 복제
            List<Socket> cloneConnections = new List<Socket>(this.Connections);
            Socket.Select(cloneConnections, null, null, 1000);
            foreach(Socket client in cloneConnections)
            {
                byte[] receivedBytes = new byte[512];
                ArrayList buffer = (ArrayList)this.ByteBuffers[cloneConnections.IndexOf(client)];

                //클라이언트로부터 전송된 데이터 담기
                int read = client.Receive(receivedBytes);
                for(int i = 0; i<read; i++)
                {
                    buffer.Add(receivedBytes[i]);
                }

                while(buffer.Count > 0) //버퍼를 다 읽어들일때까지
                {
                    //패킷의 첫번째의 정보는 전체 데이터의 크기
                    int packetDataLength = (byte)buffer[0];
                    if(packetDataLength < buffer.Count)
                    {
                        ArrayList thisPacketBytes = new ArrayList(buffer);
                        //버퍼 뒷부분 삭제
                        thisPacketBytes.RemoveRange(packetDataLength, thisPacketBytes.Count - (packetDataLength + 1));
                    }
                }
            }
        }
    }
}
