using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

public class teste2 : MonoBehaviour
{
    [SerializeField] private Transform[] elementos1;

    // Variáveis relacionadas aos dados recebidos
    private float Px_01, Py_01, Pz_01, Px_11, Py_11, Pz_11, Px_21, Py_21, Pz_21, Px_31, Py_31, Pz_31, Px_41, Py_41, Pz_41, _PositionX1, _PositionY1, _PositionZ1;
    private float theta11, theta1_graus1, theta21, theta2_graus1, theta231, theta23_graus1, theta31, theta3_graus1, theta5_graus1, a11 = 43.9f, a21 = 118f, a31 = 18.2f, d41 = 171.9f, k1;
    private float[,] _B1 = new float[20, 3];

    // Variáveis de controle
    private float theta2_graus_old = 0;
    private bool verdadeiro1 = false;
    private int m1 = 0;
    private int l1 = 0;
    private string[] separado1;

    // Variáveis para a comunicação UDP
    public UdpClient udpReceiver1;
    public Thread receiveThread1;
    private ConcurrentQueue<float> dataQueue1 = new ConcurrentQueue<float>();

    // Velocidade de rotação gradual
    private float velocidadeRotacao1 = 30f; // Graus por segundo

    // Rotação atual e destino
    private float currentRotationZ;
    private float targetRotationZ;

    // Ângulos de destino específicos
    private float anguloInicial1 = -180f;
    private float anguloFinal1 = 180f;

    // Direção da rotação (-1 para anti-horário, 1 para horário)
    private int direcao1 = 1; // Por padrão, rotação horária

    // Flag para controlar transição entre rotações
    private bool emTransicao1 = false;

    // Start is called before the first frame update
    void Start()
    {
        udpReceiver1 = new UdpClient(61559);
        receiveThread1 = new Thread(new ThreadStart(_ReceiveData1));
        receiveThread1.Start();
    }

    // Update is called once per frame
    async void Update()
    {
        if (l1 != 0)
        {
            verificar1();
        }

        //if (emTransicao1 == true)
        //{
        //    await Task.Delay(3000);
        //}

        // Verifica se há dados na fila para processar
        if (dataQueue1.Count > 0 && !emTransicao1)
        {
            if (dataQueue1.TryDequeue(out float dados1))
            {
                //Debug.Log("Removido objeto: " + dados1 + ". Contagem de elementos na fila: " + dataQueue1.Count);

                // Determina o ângulo de destino com base no dado recebido
                float targetAngle1 = dados1;

                // Limita o ângulo dentro do intervalo permitido
                targetAngle1 = Mathf.Clamp(targetAngle1, anguloInicial1, anguloFinal1);

                // Determina a direção da rotação
                if (dados1 > theta2_graus_old)
                {
                    direcao1 = 1; // Rotação horária
                    velocidadeRotacao1 = 0.36f;
                }
                else
                {
                    direcao1 = -1; // Rotação anti-horária
                    velocidadeRotacao1 = -0.36f;
                }

                // Atualiza o ângulo de rotação alvo
                targetRotationZ = targetAngle1;

                // Inicia a transição
                emTransicao1 = true;

                // Reinicia a flag de verificação
                verdadeiro1 = false;
            }
        }

        // Rotaciona gradualmente o objeto em torno do eixo Y
        RotateObjectSmooth1();
    }

    // Método para verificar se a rotação atingiu o ângulo desejado
    void verificar1()
    {
        //Debug.Log("Valor Z: " + Mathf.Round((elementos1[1].localEulerAngles.z) * 1000f) / 1000f + " Valor do Theta2: " + Mathf.Round(theta2_graus1 * 1000f) / 1000f);
        float angulo_euler12 = Mathf.Abs((Mathf.Round(elementos1[0].localEulerAngles.y * 1000) / 1000));
        float angulo_euler22 = Mathf.Abs((Mathf.Round(elementos1[1].localEulerAngles.z * 1000) / 1000) - 360);
        float angulo_euler32 = Mathf.Abs((Mathf.Round(elementos1[2].localEulerAngles.z * 1000) / 1000) - 360);
        float angulo_euler52 = Mathf.Abs((Mathf.Round(elementos1[3].localEulerAngles.z * 1000) / 1000) - 360);

        if ((decimal)theta1_graus1 < 0)
        {
            theta1_graus1 += 360;
        }
        if ((decimal)theta2_graus1 < 0)
        {
            theta2_graus1 += 360;
        }
        if ((decimal)theta3_graus1 < 0)
        {
            theta3_graus1 += 360;
        }
        if ((decimal)theta5_graus1 < 0)
        {
            theta5_graus1 += 360;
        }

        float dif12 = Mathf.Round((float)angulo_euler12 - theta1_graus1);
        float dif22 = Mathf.Round((float)angulo_euler22 - theta2_graus1);
        float dif32 = Mathf.Round((float)angulo_euler32 - theta3_graus1);
        float dif52 = Mathf.Round((float)angulo_euler52 - theta5_graus1);

        if (Mathf.Abs(dif12) < 0.3f && Mathf.Abs(dif22) < 0.3f  && Mathf.Abs(dif32) < 0.3f /*&& Mathf.Abs(dif52) < 0.3f*/)
        {
            Debug.Log("O valor lido é: " + angulo_euler22 + ". O valor de theta 2 é: " + theta2_graus1 + ". A diferença é: " + dif22);
            verdadeiro1 = true;
        }
    }

    // Método para encerrar a thread de recebimento ao destruir o objeto
    void OnDestroy1()
    {
        receiveThread1.Abort();
    }

    // Função principal de recebimento e processamento de dados UDP
    void _ReceiveData1()
    {
        while (true)
        {
            IPEndPoint remoteEndPoint1 = new IPEndPoint(IPAddress.Any, 0);
            byte[] receiveBytes1 = udpReceiver1.Receive(ref remoteEndPoint1);
            string receivedData1 = Encoding.ASCII.GetString(receiveBytes1);

            //Debug.Log("Received data in teste2: " + receivedData1);

            string[] separado1 = receivedData1.Split(';');

            // Parse dos pontos recebidos
            Px_01 = float.Parse(separado1[0]);
            Py_01 = float.Parse(separado1[2]);
            Pz_01 = float.Parse(separado1[1]);
            Px_11 = float.Parse(separado1[3]);
            Py_11 = float.Parse(separado1[5]);
            Pz_11 = float.Parse(separado1[4]);
            Px_21 = float.Parse(separado1[6]);
            Py_21 = float.Parse(separado1[8]);
            Pz_21 = float.Parse(separado1[7]);
            Px_31 = float.Parse(separado1[9]);
            Py_31 = float.Parse(separado1[11]);
            Pz_31 = float.Parse(separado1[10]);
            Px_41 = float.Parse(separado1[12]);
            Py_41 = float.Parse(separado1[14]);
            Pz_41 = float.Parse(separado1[13]);

            // Cálculo dos pontos de Bézier
            int i1 = 0;

            for (float t1 = 0; t1 <= 1; t1 += 0.05f)
            {
                int j1 = 0;

                _PositionX1 = Mathf.Pow(1 - t1, 4) * Px_01 + 4 * Mathf.Pow(1 - t1, 3) * t1 * Px_11 + 6 * Mathf.Pow(t1, 2) * Mathf.Pow(1 - t1, 2) * Px_21 + 4 * Mathf.Pow(t1, 3) * (1 - t1) * Px_31 + Mathf.Pow(t1, 4) * Px_41;
                _PositionY1 = Mathf.Pow(1 - t1, 4) * Py_01 + 4 * Mathf.Pow(1 - t1, 3) * t1 * Py_11 + 6 * Mathf.Pow(t1, 2) * Mathf.Pow(1 - t1, 2) * Py_21 + 4 * Mathf.Pow(t1, 3) * (1 - t1) * Py_31 + Mathf.Pow(t1, 4) * Py_41;
                _PositionZ1 = Mathf.Pow(1 - t1, 4) * Pz_01 + 4 * Mathf.Pow(1 - t1, 3) * t1 * Pz_11 + 6 * Mathf.Pow(t1, 2) * Mathf.Pow(1 - t1, 2) * Pz_21 + 4 * Mathf.Pow(t1, 3) * (1 - t1) * Pz_31 + Mathf.Pow(t1, 4) * Pz_41;

                _B1[i1, j1] = _PositionX1;
                j1++;
                _B1[i1, j1] = _PositionY1;
                j1++;
                _B1[i1, j1] = _PositionZ1;
                i1++;
            }

            // Cálculo do ângulo de rotação
            if (l1 == 0)
            {
                Debug.Log("O incremento m2 é: " + m1);
                l1++;
                theta11 = (Mathf.Atan2(_B1[m1, 2], _B1[m1, 0]));
                theta1_graus1 = (float)Mathf.Round(theta11 * (180 / Mathf.PI) * 1000) / 1000;
                k1 = (Mathf.Pow(_B1[m1, 0], 2) + Mathf.Pow(_B1[m1, 2], 2) + Mathf.Pow(_B1[m1, 1], 2) + Mathf.Pow(a11, 2) - 2 * a11 * (_B1[m1, 0] * Mathf.Cos(theta11) + _B1[m1, 2] * Mathf.Sin(theta11)) - Mathf.Pow(a21, 2) - Mathf.Pow(a31, 2) - Mathf.Pow(d41, 2)) / (2 * a21);
                theta31 = (Mathf.Atan2(d41, a31) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a31, 2) + Mathf.Pow(d41, 2) - Mathf.Pow(k1, 2)), k1));
                theta3_graus1 = (float)Mathf.Round(theta31 * (180 / Mathf.PI) * 1000) / 1000;
                theta231 = Mathf.Atan2(_B1[m1, 1] * (a31 + a21 * Mathf.Cos(theta31)) + (d41 + a21 * Mathf.Sin(theta31)) * (_B1[m1, 2] * Mathf.Sin(theta11) + _B1[m1, 0] * Mathf.Cos(theta11) - a11), -(d41 + (a21 * Mathf.Sin(theta31))) * _B1[m1, 1] + (a31 + a21 * Mathf.Cos(theta31)) * (_B1[m1, 2] * Mathf.Sin(theta11) + _B1[m1, 0] * Mathf.Cos(theta11) - a11));
                theta23_graus1 = (float)Mathf.Round(theta231 * (180 / Mathf.PI) * 1000) / 1000;
                theta21 = theta231 - theta31;
                theta2_graus1 = (float)Mathf.Round(theta21 * (180 / Mathf.PI) * 1000) / 1000;
                theta5_graus1 = (float)Mathf.Round((theta2_graus1 + theta3_graus1) * (180 / Mathf.PI) * 1000) / 1000;
                dataQueue1.Enqueue(theta2_graus1);
            }
            else
            {
                if (verdadeiro1 == true)
                {
                    theta2_graus_old = theta2_graus1;
                    m1++;
                    Debug.Log("O incremento m2 é: " + m1);
                    theta11 = (Mathf.Atan2(_B1[m1, 2], _B1[m1, 1]));
                    theta1_graus1 = (float)Mathf.Round(theta11 * (180 / Mathf.PI) * 1000) / 1000;
                    k1 = (Mathf.Pow(_B1[m1, 0], 2) + Mathf.Pow(_B1[m1, 2], 2) + Mathf.Pow(_B1[m1, 1], 2) + Mathf.Pow(a11, 2) - 2 * a11 * (_B1[m1, 0] * Mathf.Cos(theta11) + _B1[m1, 2] * Mathf.Sin(theta11)) - Mathf.Pow(a21, 2) - Mathf.Pow(a31, 2) - Mathf.Pow(d41, 2)) / (2 * a21);
                    theta31 = (Mathf.Atan2(d41, a31) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a31, 2) + Mathf.Pow(d41, 2) - Mathf.Pow(k1, 2)), k1));
                    theta3_graus1 = theta31 * (180 / Mathf.PI);
                    theta231 = Mathf.Atan2(_B1[m1, 1] * (a31 + a21 * Mathf.Cos(theta31)) + (d41 + a21 * Mathf.Sin(theta31)) * (_B1[m1, 2] * Mathf.Sin(theta11) + _B1[m1, 0] * Mathf.Cos(theta11) - a11), -(d41 + (a21 * Mathf.Sin(theta31))) * _B1[m1, 1] + (a31 + a21 * Mathf.Cos(theta31)) * (_B1[m1, 2] * Mathf.Sin(theta11) + _B1[m1, 0] * Mathf.Cos(theta11) - a11));
                    theta23_graus1 = theta231 * (180 / Mathf.PI);
                    theta21 = theta231 - theta31;
                    theta2_graus1 = (float)Mathf.Round(theta21 * (180 / Mathf.PI) * 1000) / 1000;
                    theta5_graus1 = (float)Mathf.Round((theta2_graus1 + theta3_graus1) * (180 / Mathf.PI) * 1000) / 1000;
                    dataQueue1.Enqueue(theta2_graus1);
                    verdadeiro1 = false;

                    // Reinicia a transição entre ângulos
                    emTransicao1 = false;
                }
            }
        }
    }

    // Função para rotacionar o objeto gradualmente em torno do eixo Y
    void RotateObjectSmooth1()
    {
        // Calcula a rotação atual do objeto em torno do eixo Y
        currentRotationZ = Mathf.LerpAngle(currentRotationZ, targetRotationZ, Time.deltaTime * velocidadeRotacao1 * direcao1);

        // Aplica a rotação gradual
        elementos1[1].localRotation = Quaternion.Euler(0f, 0f, -currentRotationZ);
    }
}
