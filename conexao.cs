using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;

public class conexao : MonoBehaviour
{
    public UdpClient udpReceiver;
    public Thread receiveThread;

    private UdpClient udpSender;
    private Thread SendThread;

    public string IP_destino = "10.13.133.254"; // IP da máquina que recebe as informações
    public int porta_destino = 61558; // porta na qual a máquina de destino das informações recebe as informações



// Start is called before the first frame update
    void Start()
        {
        // Inicialização do método implementado para receber dados

            udpReceiver = new UdpClient(61557);
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.Start();

        // Inicialização do método implementado para enviar dados
        
            udpSender = new UdpClient(porta_destino);
            SendThread = new Thread(new ThreadStart(SendLoop));
            SendThread.Start();
        }


    void ReceiveData() // Método para receber dados
    {
        while (true)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); //Identificando o IP dá maquina que enviou as informações
            byte[] receiveBytes = udpReceiver.Receive(ref remoteEndPoint); // Armazenando as informações recebidas
            string receivedData = Encoding.ASCII.GetString(receiveBytes); // Convertendo as informações do formato de byte para string
            Debug.Log("Received data: " + receivedData); // Imprimindo na tela as informações recebidas
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private void SendLoop() // Método para enviar dados continuamente
    {
        while (true)
        {
            Thread.Sleep(1000); // Example: Send a message every second (envio de mensagem a cada segundo)
            SendMessage("1200");
        }
    }

    private void SendMessage(string message) // Método para enviar dados
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message); // Codificando a mensagem de string para bytes
            udpSender.Send(data, data.Length, IP_destino, porta_destino); // Enviando os dados
            Debug.Log("enviado: " + data ); // Imprimindo a mensagem que a informação foi enviada
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error sending message: " + e.Message);
        }
    }

    void OnApplicationQuit() // Encerrando os processos de recebimento e envio de dados
    {
        receiveThread.Abort();
        udpReceiver.Close();
        udpSender.Close();
        SendThread.Abort();


    }
}