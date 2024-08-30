using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class copia_Teste : MonoBehaviour
{
    [SerializeField] private Transform[] elementos;

    // Variáveis relacionadas aos dados recebidos
    private float _PositionX, _PositionY, _PositionZ;
    private float theta1, theta1_graus, theta2, theta2_graus, theta23, theta23_graus, theta3, theta3_graus, theta5, theta5_graus, a1 = 43.9f, a2 = 118f, a3 = 18.2f, d4 = 171.9f, k;
    private float[,] _B1 = new float[21, 3];
    private float[,] _B2 = new float[21, 3];
    private float[,] _B3 = new float[21, 3];
    private float pointx_0, pointx_1, pointx_2, pointx_3, pointx_4, pointx_5, pointx_6, pointx_7, pointx_8, pointx_9, pointx_10, pointx_11, pointx_12, pointx_13, pointx_14;
    private float pointy_0, pointy_1, pointy_2, pointy_3, pointy_4, pointy_5, pointy_6, pointy_7, pointy_8, pointy_9, pointy_10, pointy_11, pointy_12, pointy_13, pointy_14;
    private float pointz_0, pointz_1, pointz_2, pointz_3, pointz_4, pointz_5, pointz_6, pointz_7, pointz_8, pointz_9, pointz_10, pointz_11, pointz_12, pointz_13, pointz_14;

    // Variáveis de controle
    private int m = 0;
    private string[] separado;
    private List<float> comparado_1 = new List<float>();
    private List<float> comparado_2 = new List<float>();
    private List<float> comparado_3 = new List<float>();
    private int curva = 1;
    private List<float> angle = new List<float>();
    private int currentAnglesIndex1 = 0;
    private int inicio;

    // Variáveis para a comunicação UDP
    public UdpClient udpReceiver;
    public Thread receiveThread;
    private ConcurrentQueue<float> dataQueue = new ConcurrentQueue<float>();

    // Velocidade de rotação gradual
    private float velocidadeRotacao; // Graus por segundo

    // Rotação atual e destino
    private float currentRotationY;
    private float targetRotationY;

    // Ângulos de destino específicos
    private float anguloInicial = -180f;
    private float anguloFinal = 180f;

    // Direção da rotação (-1 para anti-horário, 1 para horário)
    private int direcao = 1; // Por padrão, rotação horária

    // Flag para controlar transição entre rotações
    private bool emTransicao = false;

    // Start is called before the first frame update
    void Start()
    {
        udpReceiver = new UdpClient(61557);
        receiveThread = new Thread(new ThreadStart(_ReceiveData));
        receiveThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        //Verifica se o objeto está em rotação
        if (!emTransicao)
        {
            if(currentAnglesIndex1 == 59)
            {
                angle[59] = -8.297053f;
            }

            if (currentAnglesIndex1 < angle.Count)
            {
                Debug.Log("Entrou na rotação do primeiro motor " + (currentAnglesIndex1) + ". E o valor enviado foi: " + angle[currentAnglesIndex1]);

                float targetAngle = angle[currentAnglesIndex1];

                // Limita o ângulo dentro do intervalo permitido
                targetAngle = Mathf.Clamp(targetAngle, anguloInicial, anguloFinal);

                // Determina a direção da rotação
                if (targetAngle > currentRotationY)
                {
                    direcao = 1; // Rotação horária
                    velocidadeRotacao = 7f;
                }
                else
                {
                    direcao = -1; // Rotação anti-horária
                    velocidadeRotacao = -7f;
                }

                // Atualiza o ângulo de rotação alvo
                targetRotationY = targetAngle;

                // Inicia a transição
                emTransicao = true;
            }
        }
        // Rotaciona gradualmente o objeto em torno do eixo Y
        RotateObjectSmooth();

        verificar();
    }

    // Método para verificar se a rotação atingiu o ângulo desejado
    void verificar()
    {
        float angulo_euler1 = Mathf.Abs(Mathf.Abs(elementos[0].localEulerAngles.y) - 360);
        float angulo_euler2 = Mathf.Abs(Mathf.Abs(elementos[1].localEulerAngles.z) - 360);
        float angulo_euler3 = Mathf.Abs(Mathf.Abs(elementos[2].localEulerAngles.z) - 360);
        float angulo_euler5 = Mathf.Abs(Mathf.Abs(elementos[3].localEulerAngles.z) - 360);

        if (angle[currentAnglesIndex1] <= 0 || angle[currentAnglesIndex1] > 360)
        {
            if (angle[currentAnglesIndex1] <= 0)
            {
                angle[currentAnglesIndex1] += 360;
            }
            else
            {
                angle[currentAnglesIndex1] -= 360;
            }
            
        }
        if (comparado_2[currentAnglesIndex1] < 0)
        {
            comparado_2[currentAnglesIndex1] += 360;
        }
        if (comparado_1[currentAnglesIndex1] > 360 || comparado_1[currentAnglesIndex1] < 0)
        {
            if (comparado_1[currentAnglesIndex1] > 360)
            {
                comparado_1[currentAnglesIndex1] -= 360;
            }
            else
            {
                comparado_1[currentAnglesIndex1] += 360;
            }

        }
        if (comparado_3[currentAnglesIndex1] < 0)
        {
            comparado_3[currentAnglesIndex1] += 360;
        }

        float dif1 = Mathf.Abs(angulo_euler1 - angle[currentAnglesIndex1]);
        float dif2 = Mathf.Abs(angulo_euler2 - comparado_2[currentAnglesIndex1]);
        float dif3 = Mathf.Abs(angulo_euler3 - comparado_1[currentAnglesIndex1]);
        float dif5 = Mathf.Abs(angulo_euler5 - comparado_3[currentAnglesIndex1]);

        //Debug.Log("Theta 1 lido " + angulo_euler1 + ". Angulo 1 enviado " + angle[currentAnglesIndex1] + ". A diferença é: " + dif1);
        //Debug.Log("Theta 2 lido " + angulo_euler2 + ". Angulo 2 enviado " + comparado_2[currentAnglesIndex1] + ". A diferença é: " + dif2);
        //Debug.Log("Theta 3 lido " + angulo_euler3 + ". Angulo 3 enviado " + comparado_1 + ". A diferença é: " + dif3);
        //Debug.Log("Entrou na rotação " + currentAnglesIndex1 + ". Theta 5 lido " + angulo_euler5 + ". Angulo 5 enviado " + comparado_3[currentAnglesIndex1] + ". A diferença é: " + dif5);

        if (dif1 < 0.03f && dif2 < 0.03f && dif3 < 0.03f /*&& dif5 < 0.03f*/)
        {
            //Debug.Log("O valor lido é: " + angulo_euler1 + ". O valor de theta 1 é: " + theta1_graus + ". A diferença é: " + dif1);
            emTransicao = false;
            if(angle.Count == (currentAnglesIndex1 + 1))
            {
              elementos[0].localRotation = Quaternion.Euler(0f, -8.297053f, 0f);
            }
            if (currentAnglesIndex1 < 59)
            {
                currentAnglesIndex1++;
            }
        }
    }

    // Método para encerrar a thread de recebimento ao destruir o objeto
    void OnDestroy()
    {
        receiveThread.Abort();
    }

    // Função principal de recebimento e processamento de dados UDP
    void _ReceiveData()
    {
        while (true)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receiveBytes = udpReceiver.Receive(ref remoteEndPoint);
            string receivedData = Encoding.ASCII.GetString(receiveBytes);

            //Debug.Log("Received data: " + receivedData);

            string[] separado = receivedData.Split(';');

            // Parse dos pontos recebidos

            pointx_0 = float.Parse(separado[0]);
            pointy_0 = float.Parse(separado[1]);
            pointz_0 = float.Parse(separado[2]);
            pointx_1 = float.Parse(separado[3]);
            pointy_1 = float.Parse(separado[4]);
            pointz_1 = float.Parse(separado[5]);
            pointx_2 = float.Parse(separado[6]);
            pointy_2 = float.Parse(separado[7]);
            pointz_2 = float.Parse(separado[8]);
            pointx_3 = float.Parse(separado[9]);
            pointy_3 = float.Parse(separado[10]);
            pointz_3 = float.Parse(separado[11]);
            pointx_4 = float.Parse(separado[12]);
            pointy_4 = float.Parse(separado[13]);
            pointz_4 = float.Parse(separado[14]);
            pointx_5 = float.Parse(separado[15]);
            pointy_5 = float.Parse(separado[16]);
            pointz_5 = float.Parse(separado[17]);
            pointx_6 = float.Parse(separado[18]);
            pointy_6 = float.Parse(separado[19]);
            pointz_6 = float.Parse(separado[20]);
            pointx_7 = float.Parse(separado[21]);
            pointy_7 = float.Parse(separado[22]);
            pointz_7 = float.Parse(separado[23]);
            pointx_8 = float.Parse(separado[24]);
            pointy_8 = float.Parse(separado[25]);
            pointz_8 = float.Parse(separado[26]);
            pointx_9 = float.Parse(separado[27]);
            pointy_9 = float.Parse(separado[28]);
            pointz_9 = float.Parse(separado[29]);
            pointx_10 = float.Parse(separado[30]);
            pointy_10 = float.Parse(separado[31]);
            pointz_10 = float.Parse(separado[32]);
            pointx_11 = float.Parse(separado[33]);
            pointy_11 = float.Parse(separado[34]);
            pointz_11 = float.Parse(separado[35]);
            pointx_12 = float.Parse(separado[36]);
            pointy_12 = float.Parse(separado[37]);
            pointz_12 = float.Parse(separado[38]);
            pointx_13 = float.Parse(separado[39]);
            pointy_13 = float.Parse(separado[40]);
            pointz_13 = float.Parse(separado[41]);
            pointx_14 = float.Parse(separado[42]);
            pointy_14 = float.Parse(separado[43]);
            pointz_14 = float.Parse(separado[44]);
            inicio = int.Parse(separado[45]);

            // Se o último ponto das informações recebidas for maior que zero, inicia-se a movimentação
            if (inicio > 0)
            {
                curva = 1;
                m = 0;
                currentAnglesIndex1 = 0;

                // Limpando a lista de ângulos
                angle.Clear();
                comparado_1.Clear();
                comparado_2.Clear();
                comparado_3.Clear();

                //Chamando a função que calcula os ângulos
                angulo();
            }
        }
    }

    void angulo()
    {
        // Cálculo dos pontos de Bézier: Primeira Curva
        int i = 0;

        for (float t = 0; t <= 1; t += 0.05f)
        {
            int j = 0;

            _PositionX = Mathf.Pow(1 - t, 4) * pointx_0 + 4 * Mathf.Pow(1 - t, 3) * t * pointx_1 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointx_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointx_3 + Mathf.Pow(t, 4) * pointx_4;
            _PositionY = Mathf.Pow(1 - t, 4) * pointy_0 + 4 * Mathf.Pow(1 - t, 3) * t * pointy_1 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointy_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointy_3 + Mathf.Pow(t, 4) * pointy_4;
            _PositionZ = Mathf.Pow(1 - t, 4) * pointz_0 + 4 * Mathf.Pow(1 - t, 3) * t * pointz_1 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointz_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointz_3 + Mathf.Pow(t, 4) * pointz_4;

            _B1[i, j] = _PositionX;
            j++;
            _B1[i, j] = _PositionY;
            j++;
            _B1[i, j] = _PositionZ;
            i++;
        }

        //Cálculo dos pontos de Bézier: Segunda Curva

        i = 0;
        for (float t = 0; t <= 1; t += 0.05f)
        {
            int j = 0;

            _PositionX = Mathf.Pow(1 - t, 4) * pointx_5 + 4 * Mathf.Pow(1 - t, 3) * t * pointx_6 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointx_7 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointx_8 + Mathf.Pow(t, 4) * pointx_9;
            _PositionY = Mathf.Pow(1 - t, 4) * pointy_5 + 4 * Mathf.Pow(1 - t, 3) * t * pointy_6 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointy_7 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointy_8 + Mathf.Pow(t, 4) * pointy_9;
            _PositionZ = Mathf.Pow(1 - t, 4) * pointz_5 + 4 * Mathf.Pow(1 - t, 3) * t * pointz_6 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointz_7 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointz_8 + Mathf.Pow(t, 4) * pointz_9;

            _B2[i, j] = _PositionX;
            j++;
            _B2[i, j] = _PositionY;
            j++;
            _B2[i, j] = _PositionZ;
            i++;
        }

        //Cálculo dos pontos de Bézier: Terceira Curva

        i = 0;
        for (float t = 0; t <= 1; t += 0.05f)
        {
            int j = 0;

            _PositionX = Mathf.Pow(1 - t, 4) * pointx_10 + 4 * Mathf.Pow(1 - t, 3) * t * pointx_11 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointx_12 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointx_13 + Mathf.Pow(t, 4) * pointx_14;
            _PositionY = Mathf.Pow(1 - t, 4) * pointy_10 + 4 * Mathf.Pow(1 - t, 3) * t * pointy_11 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointy_12 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointy_13 + Mathf.Pow(t, 4) * pointy_14;
            _PositionZ = Mathf.Pow(1 - t, 4) * pointz_10 + 4 * Mathf.Pow(1 - t, 3) * t * pointz_11 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointz_12 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointz_13 + Mathf.Pow(t, 4) * pointz_14;

            _B3[i, j] = _PositionX;
            j++;
            _B3[i, j] = _PositionY;
            j++;
            _B3[i, j] = _PositionZ;
            i++;
        }

        for (int a = 0; a < 65; a++)
        {
            // Cálculo do ângulo de rotação
            if (curva == 1)
            {
                if (m < 20)
                {
                    theta1 = (Mathf.Atan2(_B1[m, 1], _B1[m, 0]));
                    theta1_graus = theta1 * (180 / Mathf.PI);
                    angle.Add(theta1_graus);
                    k = (Mathf.Pow(_B1[m, 0], 2) + Mathf.Pow(_B1[m, 1], 2) + Mathf.Pow(_B1[m, 2], 2) + Mathf.Pow(a1, 2) - 2 * a1 * (_B1[m, 0] * Mathf.Cos(theta1) + _B1[m, 1] * Mathf.Sin(theta1)) - Mathf.Pow(a2, 2) - Mathf.Pow(a3, 2) - Mathf.Pow(d4, 2)) / (2 * a2);
                    theta3 = (Mathf.Atan2(d4, a3) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a3, 2) + Mathf.Pow(d4, 2) - Mathf.Pow(k, 2)), k));
                    theta3_graus = theta3 * (180 / Mathf.PI);
                    comparado_1.Add(306.09f - theta3_graus);
                    theta23 = Mathf.Atan2(_B1[m, 2] * (a3 + a2 * Mathf.Cos(theta3)) + (d4 + a2 * Mathf.Sin(theta3)) * (_B1[m, 1] * Mathf.Sin(theta1) + _B1[m, 0] * Mathf.Cos(theta1) - a1), -(d4 + (a2 * Mathf.Sin(theta3))) * _B1[m, 2] + (a3 + a2 * Mathf.Cos(theta3)) * (_B1[m, 1] * Mathf.Sin(theta1) + _B1[m, 0] * Mathf.Cos(theta1) - a1));
                    theta23_graus = theta23 * (180 / Mathf.PI);
                    theta2 = theta23 - theta3;
                    theta2_graus = theta2 * (180 / Mathf.PI);
                    comparado_2.Add(144.58f - theta2_graus);
                    theta5 = theta2 + theta3;
                    theta5_graus = theta5 * (180 / Mathf.PI);
                    comparado_3.Add(79.6f + theta5_graus);
                    m++;
                }

                else
                {
                    //Atualiza o número da curva
                    curva++;

                    // Reinicia o ciclo partindo da posição final
                    m = 0;
                }
            }

            else
            {
                if (curva == 2)
                {
                    if (m < 20)
                    {
                        theta1 = (Mathf.Atan2(_B2[m, 1], _B2[m, 0]));
                        theta1_graus = theta1 * (180 / Mathf.PI);
                        angle.Add(theta1_graus);
                        k = (Mathf.Pow(_B2[m, 0], 2) + Mathf.Pow(_B2[m, 1], 2) + Mathf.Pow(_B2[m, 2], 2) + Mathf.Pow(a1, 2) - 2 * a1 * (_B2[m, 0] * Mathf.Cos(theta1) + _B2[m, 1] * Mathf.Sin(theta1)) - Mathf.Pow(a2, 2) - Mathf.Pow(a3, 2) - Mathf.Pow(d4, 2)) / (2 * a2);
                        theta3 = (Mathf.Atan2(d4, a3) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a3, 2) + Mathf.Pow(d4, 2) - Mathf.Pow(k, 2)), k));
                        theta3_graus = theta3 * (180 / Mathf.PI);
                        comparado_1.Add(306.09f - theta3_graus);
                        theta23 = Mathf.Atan2(_B2[m, 2] * (a3 + a2 * Mathf.Cos(theta3)) + (d4 + a2 * Mathf.Sin(theta3)) * (_B2[m, 1] * Mathf.Sin(theta1) + _B2[m, 0] * Mathf.Cos(theta1) - a1), -(d4 + (a2 * Mathf.Sin(theta3))) * _B2[m, 2] + (a3 + a2 * Mathf.Cos(theta3)) * (_B2[m, 1] * Mathf.Sin(theta1) + _B2[m, 0] * Mathf.Cos(theta1) - a1));
                        theta23_graus = theta23 * (180 / Mathf.PI);
                        theta2 = theta23 - theta3;
                        theta2_graus = theta2 * (180 / Mathf.PI);
                        comparado_2.Add(144.58f - theta2_graus);
                        theta5 = theta2 + theta3;
                        theta5_graus = theta5 * (180 / Mathf.PI);
                        comparado_3.Add(79.6f + theta5_graus);
                        m++;
                    }

                    else
                    {
                        //Atualiza o número da curva
                        curva++;

                        // Reinicia o ciclo partindo da posição final
                        m = 0;
                    }
                }

                else
                {
                    if (curva == 3)
                    {
                        if (m < 20)
                        {
                            theta1 = (Mathf.Atan2(_B3[m, 1], _B3[m, 0]));
                            theta1_graus = theta1 * (180 / Mathf.PI);
                            angle.Add(theta1_graus);
                            k = (Mathf.Pow(_B3[m, 0], 2) + Mathf.Pow(_B3[m, 1], 2) + Mathf.Pow(_B3[m, 2], 2) + Mathf.Pow(a1, 2) - 2 * a1 * (_B3[m, 0] * Mathf.Cos(theta1) + _B3[m, 1] * Mathf.Sin(theta1)) - Mathf.Pow(a2, 2) - Mathf.Pow(a3, 2) - Mathf.Pow(d4, 2)) / (2 * a2);
                            theta3 = (Mathf.Atan2(d4, a3) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a3, 2) + Mathf.Pow(d4, 2) - Mathf.Pow(k, 2)), k));
                            theta3_graus = theta3 * (180 / Mathf.PI);
                            comparado_1.Add(306.09f - theta3_graus);
                            theta23 = Mathf.Atan2(_B3[m, 2] * (a3 + a2 * Mathf.Cos(theta3)) + (d4 + a2 * Mathf.Sin(theta3)) * (_B3[m, 1] * Mathf.Sin(theta1) + _B3[m, 0] * Mathf.Cos(theta1) - a1), -(d4 + (a2 * Mathf.Sin(theta3))) * _B3[m, 2] + (a3 + a2 * Mathf.Cos(theta3)) * (_B3[m, 1] * Mathf.Sin(theta1) + _B3[m, 0] * Mathf.Cos(theta1) - a1));
                            theta23_graus = theta23 * (180 / Mathf.PI);
                            theta2 = theta23 - theta3;
                            theta2_graus = theta2 * (180 / Mathf.PI);
                            comparado_2.Add(144.58f - theta2_graus);
                            theta5 = theta2 + theta3;
                            theta5_graus = theta5 * (180 / Mathf.PI);
                            comparado_3.Add(79.6f + theta5_graus);
                            m++;
                            Debug.Log("O valor " + (a - 2) + " é: " + angle[a - 2]);
                        }

                        else
                        {
                            // Reinicia o ciclo partindo da posição final
                            m = 0;
                            curva = 0;
                        }
                    }
                }
            }
        }
    }


    // Função para rotacionar o objeto gradualmente em torno do eixo Y
    void RotateObjectSmooth()
    {
        // Calcula a rotação atual do objeto em torno do eixo Y
        currentRotationY = Mathf.LerpAngle(currentRotationY, targetRotationY, Time.deltaTime * velocidadeRotacao * direcao);

        // Aplica a rotação gradual
        elementos[0].localRotation = Quaternion.Euler(0f, -currentRotationY, 0f);
    }
}


//[SerializeField] private Transform[] elementos;

//// Variáveis relacionadas aos dados recebidos
//private float Px_0, Py_0, Pz_0, Px_1, Py_1, Pz_1, Px_2, Py_2, Pz_2, Px_3, Py_3, Pz_3, Px_4, Py_4, Pz_4, _PositionX, _PositionY, _PositionZ;
//private float theta1, theta1_graus, theta2, theta2_graus, theta23, theta23_graus, theta3, theta3_graus, theta5, theta5_graus, a1 = 43.9f, a2 = 118f, a3 = 18.2f, d4 = 171.9f, k;
//private float[,] _B1 = new float[21, 3];
//private float[,] _B2 = new float[21, 3];
//private float[,] _B3 = new float[21, 3];
//private float pointx_0, pointx_1, pointx_2, pointx_3, pointx_4, pointx_5, pointx_6, pointx_7, pointx_8, pointx_9, pointx_10, pointx_11, pointx_12, pointx_13, pointx_14;
//private float pointy_0, pointy_1, pointy_2, pointy_3, pointy_4, pointy_5, pointy_6, pointy_7, pointy_8, pointy_9, pointy_10, pointy_11, pointy_12, pointy_13, pointy_14;
//private float pointz_0, pointz_1, pointz_2, pointz_3, pointz_4, pointz_5, pointz_6, pointz_7, pointz_8, pointz_9, pointz_10, pointz_11, pointz_12, pointz_13, pointz_14;

//// Variáveis de controle
//private float theta1_graus_old = 0;
//private bool verdadeiro = false;
//private int m = 0;
//private int l = 0;
//private string[] separado;
//private float comparado_1, comparado_2, comparado_3;
//private int curva = 1;

//// Variáveis para a comunicação UDP
//public UdpClient udpReceiver;
//public Thread receiveThread;
//private ConcurrentQueue<float> dataQueue = new ConcurrentQueue<float>();

//// Velocidade de rotação gradual
//private float velocidadeRotacao; // Graus por segundo

//// Rotação atual e destino
//private float currentRotationY;
//private float targetRotationY;

//// Ângulos de destino específicos
//private float anguloInicial = -180f;
//private float anguloFinal = 180f;

//// Direção da rotação (-1 para anti-horário, 1 para horário)
//private int direcao = 1; // Por padrão, rotação horária

//// Flag para controlar transição entre rotações
//private bool emTransicao = false;

////private int ciclosCompletos = 0; // Variável para contar os ciclos completos
//private const int maxCiclos = 3; // Número máximo de ciclos

//// Start is called before the first frame update
//void Start()
//{
//    udpReceiver = new UdpClient(61557);
//    receiveThread = new Thread(new ThreadStart(_ReceiveData));
//    receiveThread.Start();
//}

//// Update is called once per frame
//void Update()
//{
//    if (l != 0)
//    {
//        verificar();
//    }

//    // Verifica se há dados na fila para processar
//    if (dataQueue.Count > 0 && !emTransicao)
//    {
//        if (dataQueue.TryDequeue(out float dados))
//        {
//            //Debug.Log("Removido objeto: " + dados + ". Contagem de elementos na fila: " + dataQueue.Count);

//            // Determina o ângulo de destino com base no dado recebido
//            float targetAngle = dados;

//            // Limita o ângulo dentro do intervalo permitido
//            targetAngle = Mathf.Clamp(targetAngle, anguloInicial, anguloFinal);

//            // Determina a direção da rotação
//            if (dados > currentRotationY)
//            {
//                direcao = 1; // Rotação horária
//                velocidadeRotacao = 7f;
//            }
//            else
//            {
//                direcao = -1; // Rotação anti-horária
//                velocidadeRotacao = -7f;
//            }

//            // Atualiza o ângulo de rotação alvo
//            targetRotationY = targetAngle;

//            // Inicia a transição
//            emTransicao = true;

//            // Reinicia a flag de verificação
//            verdadeiro = false;
//        }
//    }

//    // Rotaciona gradualmente o objeto em torno do eixo Y
//    RotateObjectSmooth();
//}

//// Método para verificar se a rotação atingiu o ângulo desejado
//void verificar()
//{
//    //Debug.Log("Valor Y: " + Mathf.Round(elementos[0].localEulerAngles.y * 1000f) / 1000f + " Valor do Theta1: " + Mathf.Round(theta1_graus * 1000f) / 1000f);
//    float angulo_euler1 = Mathf.Abs((elementos[0].localEulerAngles.y) - 360);
//    float angulo_euler2 = Mathf.Abs((elementos[1].localEulerAngles.z) - 360);
//    float angulo_euler3 = Mathf.Abs((elementos[2].localEulerAngles.z) - 360);
//    float angulo_euler5 = Mathf.Abs((elementos[3].localEulerAngles.z) - 360);
//    if (theta1_graus < 0)
//    {
//        theta1_graus += 360;
//    }
//    if (theta1_graus == 0)
//    {
//        theta1_graus += 360;
//    }
//    if (theta2_graus < 0)
//    {
//        theta2_graus += 360;
//    }
//    if (theta3_graus < 0)
//    {
//        theta3_graus += 360;
//    }
//    if (comparado_1 > 360)
//    {
//        comparado_1 -= 360;
//        //Debug.Log("Theta 3 atualizado é: " + theta3_graus);
//    }
//    if (theta5_graus < 0)
//    {
//        theta5_graus += 360;
//    }

//    float dif1 = Mathf.Round(angulo_euler1 - theta1_graus);
//    float dif2 = Mathf.Round(angulo_euler2 - comparado_2);
//    float dif3 = Mathf.Round(angulo_euler3 - comparado_1);
//    float dif5 = Mathf.Round(angulo_euler5 - comparado_3);

//   //Debug.Log("Theta 1 lido " + angulo_euler1 + ". Angulo 1 enviado " + theta1_graus + ". A diferença é: " + dif1);
//    //Debug.Log("Theta 2 lido " + angulo_euler2 + ". Angulo 2 enviado " + comparado_2 + ". A diferença é: " + dif2);
//    //Debug.Log("Theta 3 lido " + angulo_euler3 + ". Angulo 3 enviado " + comparado_1 + ". A diferença é: " + dif3);
//    //Debug.Log("Theta 5 lido " + angulo_euler5 + ". Angulo 5 enviado " + comparado_3 + ". A diferença é: " + dif5);

//    if (Mathf.Abs(dif1) < 0.03f /*&& Mathf.Abs(dif2) < 0.03f && Mathf.Abs(dif3) < 0.03f &&*/ Mathf.Abs(dif5) < 5f)
//    {
//        //Debug.Log("O valor lido é: " + angulo_euler1 + ". O valor de theta 1 é: " + theta1_graus + ". A diferença é: " + dif1);
//        verdadeiro = true;
//    }
//}

//// Método para encerrar a thread de recebimento ao destruir o objeto
//void OnDestroy()
//{
//    receiveThread.Abort();
//}

//// Função principal de recebimento e processamento de dados UDP
//void _ReceiveData()
//{
//    while (true)
//    {
//        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
//        byte[] receiveBytes = udpReceiver.Receive(ref remoteEndPoint);
//        string receivedData = Encoding.ASCII.GetString(receiveBytes);

//        //Debug.Log("Received data: " + receivedData);

//        string[] separado = receivedData.Split(';');

//        // Parse dos pontos recebidos

//        pointx_0 = float.Parse(separado[0]);
//        pointy_0 = float.Parse(separado[1]);
//        pointz_0 = float.Parse(separado[2]);
//        pointx_1 = float.Parse(separado[3]);
//        pointy_1 = float.Parse(separado[4]);
//        pointz_1 = float.Parse(separado[5]);
//        pointx_2 = float.Parse(separado[6]);
//        pointy_2 = float.Parse(separado[7]);
//        pointz_2 = float.Parse(separado[8]);
//        pointx_3 = float.Parse(separado[9]);
//        pointy_3 = float.Parse(separado[10]);
//        pointz_3 = float.Parse(separado[11]);
//        pointx_4 = float.Parse(separado[12]);
//        pointy_4 = float.Parse(separado[13]);
//        pointz_4 = float.Parse(separado[14]);
//        pointx_5 = float.Parse(separado[15]);
//        pointy_5 = float.Parse(separado[16]);
//        pointz_5 = float.Parse(separado[17]);
//        pointx_6 = float.Parse(separado[18]);
//        pointy_6 = float.Parse(separado[19]);
//        pointz_6 = float.Parse(separado[20]);
//        pointx_7 = float.Parse(separado[21]);
//        pointy_7 = float.Parse(separado[22]);
//        pointz_7 = float.Parse(separado[23]);
//        pointx_8 = float.Parse(separado[24]);
//        pointy_8 = float.Parse(separado[25]);
//        pointz_8 = float.Parse(separado[26]);
//        pointx_9 = float.Parse(separado[27]);
//        pointy_9 = float.Parse(separado[28]);
//        pointz_9 = float.Parse(separado[29]);
//        pointx_10 = float.Parse(separado[30]);
//        pointy_10 = float.Parse(separado[31]);
//        pointz_10 = float.Parse(separado[32]);
//        pointx_11 = float.Parse(separado[33]);
//        pointy_11 = float.Parse(separado[34]);
//        pointz_11 = float.Parse(separado[35]);
//        pointx_12 = float.Parse(separado[36]);
//        pointy_12 = float.Parse(separado[37]);
//        pointz_12 = float.Parse(separado[38]);
//        pointx_13 = float.Parse(separado[39]);
//        pointy_13 = float.Parse(separado[40]);
//        pointz_13 = float.Parse(separado[41]);
//        pointx_14 = float.Parse(separado[42]);
//        pointy_14 = float.Parse(separado[43]);
//        pointz_14 = float.Parse(separado[44]);

//        // Cálculo dos pontos de Bézier: Primeira Curva
//        int i = 0;

//        for (float t = 0; t <= 1; t += 0.05f)
//        {
//            int j = 0;

//            _PositionX = Mathf.Pow(1 - t, 4) * pointx_0 + 4 * Mathf.Pow(1 - t, 3) * t * pointx_1 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointx_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointx_3 + Mathf.Pow(t, 4) * pointx_4;
//            _PositionY = Mathf.Pow(1 - t, 4) * pointy_0 + 4 * Mathf.Pow(1 - t, 3) * t * pointy_1 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointy_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointy_3 + Mathf.Pow(t, 4) * pointy_4;
//            _PositionZ = Mathf.Pow(1 - t, 4) * pointz_0 + 4 * Mathf.Pow(1 - t, 3) * t * pointz_1 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointz_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointz_3 + Mathf.Pow(t, 4) * pointz_4;

//            _B1[i, j] = _PositionX;
//            j++;
//            _B1[i, j] = _PositionY;
//            j++;
//            _B1[i, j] = _PositionZ;
//            i++;
//        }

//        //Cálculo dos pontos de Bézier: Segunda Curva

//        i = 0;
//        for (float t = 0; t <= 1; t += 0.05f)
//        {
//            int j = 0;

//            _PositionX = Mathf.Pow(1 - t, 4) * pointx_5 + 4 * Mathf.Pow(1 - t, 3) * t * pointx_6 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointx_7 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointx_8 + Mathf.Pow(t, 4) * pointx_9;
//            _PositionY = Mathf.Pow(1 - t, 4) * pointy_5 + 4 * Mathf.Pow(1 - t, 3) * t * pointy_6 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointy_7 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointy_8 + Mathf.Pow(t, 4) * pointy_9;
//            _PositionZ = Mathf.Pow(1 - t, 4) * pointz_5 + 4 * Mathf.Pow(1 - t, 3) * t * pointz_6 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointz_7 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointz_8 + Mathf.Pow(t, 4) * pointz_9;

//            _B2[i, j] = _PositionX;
//            j++;
//            _B2[i, j] = _PositionY;
//            j++;
//            _B2[i, j] = _PositionZ;
//            i++;
//        }

//        //Cálculo dos pontos de Bézier: Terceira Curva

//        i = 0;
//        for (float t = 0; t <= 1; t += 0.05f)
//        {
//            int j = 0;

//            _PositionX = Mathf.Pow(1 - t, 4) * pointx_10 + 4 * Mathf.Pow(1 - t, 3) * t * pointx_11 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointx_12 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointx_13 + Mathf.Pow(t, 4) * pointx_14;
//            _PositionY = Mathf.Pow(1 - t, 4) * pointy_10 + 4 * Mathf.Pow(1 - t, 3) * t * pointy_11 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointy_12 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointy_13 + Mathf.Pow(t, 4) * pointy_14;
//            _PositionZ = Mathf.Pow(1 - t, 4) * pointz_10 + 4 * Mathf.Pow(1 - t, 3) * t * pointz_11 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointz_12 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointz_13 + Mathf.Pow(t, 4) * pointz_14;

//            _B3[i, j] = _PositionX;
//            j++;
//            _B3[i, j] = _PositionY;
//            j++;
//            _B3[i, j] = _PositionZ;
//            i++;
//        }

//        // Cálculo do ângulo de rotação
//        if (curva == 1)
//        {
//            if (l == 0)
//            {
//                Debug.Log("O incremento m1 é: " + m + ". E o angulo é: " + theta1_graus);
//                l++;
//                theta1 = (Mathf.Atan2(_B1[m, 1], _B1[m, 0]));
//                theta1_graus = theta1 * (180 / Mathf.PI);
//                k = (Mathf.Pow(_B1[m, 0], 2) + Mathf.Pow(_B1[m, 1], 2) + Mathf.Pow(_B1[m, 2], 2) + Mathf.Pow(a1, 2) - 2 * a1 * (_B1[m, 0] * Mathf.Cos(theta1) + _B1[m, 1] * Mathf.Sin(theta1)) - Mathf.Pow(a2, 2) - Mathf.Pow(a3, 2) - Mathf.Pow(d4, 2)) / (2 * a2);
//                theta3 = (Mathf.Atan2(d4, a3) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a3, 2) + Mathf.Pow(d4, 2) - Mathf.Pow(k, 2)), k));
//                theta3_graus = Mathf.Round(theta3 * (180 / Mathf.PI) * 1000) / 1000;
//                comparado_1 = 306.09f - theta3_graus;
//                theta23 = Mathf.Atan2(_B1[m, 2] * (a3 + a2 * Mathf.Cos(theta3)) + (d4 + a2 * Mathf.Sin(theta3)) * (_B1[m, 1] * Mathf.Sin(theta1) + _B1[m, 0] * Mathf.Cos(theta1) - a1), -(d4 + (a2 * Mathf.Sin(theta3))) * _B1[m, 2] + (a3 + a2 * Mathf.Cos(theta3)) * (_B1[m, 1] * Mathf.Sin(theta1) + _B1[m, 0] * Mathf.Cos(theta1) - a1));
//                theta23_graus = theta23 * (180 / Mathf.PI);
//                theta2 = theta23 - theta3;
//                theta2_graus = Mathf.Round(theta2 * (180 / Mathf.PI) * 1000) / 1000;
//                comparado_2 = 144.58f - theta2_graus;
//                theta5 = theta2 + theta3;
//                theta5_graus = theta5 * (180 / Mathf.PI);
//                comparado_3 = 79.6f + theta5_graus;
//                m++;
//                dataQueue.Enqueue(theta1_graus);
//            }

//            else
//            {
//                if (verdadeiro == true)
//                {
//                    theta1_graus_old = theta1_graus;
//                    if (m < 21)
//                    {
//                        Debug.Log("O incremento m1 é: " + m + ". E o angulo é: " + theta1_graus);
//                        theta1 = (Mathf.Atan2(_B1[m, 1], _B1[m, 0]));
//                        theta1_graus = Mathf.Round(theta1 * (180 / Mathf.PI) * 1000) / 1000;
//                        k = (Mathf.Pow(_B1[m, 0], 2) + Mathf.Pow(_B1[m, 1], 2) + Mathf.Pow(_B1[m, 2], 2) + Mathf.Pow(a1, 2) - 2 * a1 * (_B1[m, 0] * Mathf.Cos(theta1) + _B1[m, 1] * Mathf.Sin(theta1)) - Mathf.Pow(a2, 2) - Mathf.Pow(a3, 2) - Mathf.Pow(d4, 2)) / (2 * a2);
//                        theta3 = (Mathf.Atan2(d4, a3) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a3, 2) + Mathf.Pow(d4, 2) - Mathf.Pow(k, 2)), k));
//                        theta3_graus = theta3 * (180 / Mathf.PI);
//                        comparado_1 = 306.09f - theta3_graus;
//                        theta23 = Mathf.Atan2(_B1[m, 2] * (a3 + a2 * Mathf.Cos(theta3)) + (d4 + a2 * Mathf.Sin(theta3)) * (_B1[m, 1] * Mathf.Sin(theta1) + _B1[m, 0] * Mathf.Cos(theta1) - a1), -(d4 + (a2 * Mathf.Sin(theta3))) * _B1[m, 2] + (a3 + a2 * Mathf.Cos(theta3)) * (_B1[m, 1] * Mathf.Sin(theta1) + _B1[m, 0] * Mathf.Cos(theta1) - a1));
//                        theta23_graus = theta23 * (180 / Mathf.PI);
//                        theta2 = theta23 - theta3;
//                        theta2_graus = Mathf.Round(theta2 * (180 / Mathf.PI) * 1000) / 1000;
//                        comparado_2 = 144.58f - theta2_graus;
//                        theta5 = theta2 + theta3;
//                        theta5_graus = theta5 * (180 / Mathf.PI);
//                        comparado_3 = 79.6f + theta5_graus;
//                        dataQueue.Enqueue(theta1_graus);
//                        m++;
//                        verdadeiro = false;

//                        // Reinicia a transição entre ângulos
//                        emTransicao = false;
//                    }

//                    else
//                    {
//                        //Atualiza o número da curva
//                        curva+= 1;

//                        // Reinicia o ciclo partindo da posição final
//                        m = 0;
//                        targetRotationY = theta1_graus;
//                        emTransicao = false; // Permite nova movimentação
//                    }
//                }
//            }
//        }

//        else
//        {
//            if (curva == 2)
//            {
//                if (verdadeiro == true)
//                {
//                    theta1_graus_old = theta1_graus;
//                    if (m < 21)
//                    {
//                        Debug.Log("O incremento m1 é: " + m + ". E o angulo é: " + theta1_graus);
//                        theta1 = (Mathf.Atan2(_B2[m, 1], _B2[m, 0]));
//                        theta1_graus = Mathf.Round(theta1 * (180 / Mathf.PI) * 1000) / 1000;
//                        k = (Mathf.Pow(_B2[m, 0], 2) + Mathf.Pow(_B2[m, 1], 2) + Mathf.Pow(_B2[m, 2], 2) + Mathf.Pow(a1, 2) - 2 * a1 * (_B2[m, 0] * Mathf.Cos(theta1) + _B2[m, 1] * Mathf.Sin(theta1)) - Mathf.Pow(a2, 2) - Mathf.Pow(a3, 2) - Mathf.Pow(d4, 2)) / (2 * a2);
//                        theta3 = (Mathf.Atan2(d4, a3) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a3, 2) + Mathf.Pow(d4, 2) - Mathf.Pow(k, 2)), k));
//                        theta3_graus = theta3 * (180 / Mathf.PI);
//                        comparado_1 = 306.09f - theta3_graus;
//                        theta23 = Mathf.Atan2(_B2[m, 2] * (a3 + a2 * Mathf.Cos(theta3)) + (d4 + a2 * Mathf.Sin(theta3)) * (_B2[m, 1] * Mathf.Sin(theta1) + _B2[m, 0] * Mathf.Cos(theta1) - a1), -(d4 + (a2 * Mathf.Sin(theta3))) * _B2[m, 2] + (a3 + a2 * Mathf.Cos(theta3)) * (_B2[m, 1] * Mathf.Sin(theta1) + _B2[m, 0] * Mathf.Cos(theta1) - a1));
//                        theta23_graus = theta23 * (180 / Mathf.PI);
//                        theta2 = theta23 - theta3;
//                        theta2_graus = Mathf.Round(theta2 * (180 / Mathf.PI) * 1000) / 1000;
//                        comparado_2 = 144.58f - theta2_graus;
//                        theta5 = theta2 + theta3;
//                        theta5_graus = theta5 * (180 / Mathf.PI);
//                        comparado_3 = 79.6f + theta5_graus;
//                        dataQueue.Enqueue(theta1_graus);
//                        m++;
//                        verdadeiro = false;

//                        // Reinicia a transição entre ângulos
//                        emTransicao = false;
//                    }

//                    else
//                    {
//                        //Atualiza o número da curva
//                        curva++;

//                        // Reinicia o ciclo partindo da posição final
//                        m = 0;
//                        targetRotationY = theta1_graus;
//                        emTransicao = false; // Permite nova movimentação
//                    }
//                }
//            }

//            else
//            {
//                if (verdadeiro == true)
//                {
//                    theta1_graus_old = theta1_graus;
//                    if (m < 21)
//                    {
//                        Debug.Log("O incremento m1 é: " + m);
//                        theta1 = (Mathf.Atan2(_B3[m, 1], _B3[m, 0]));
//                        theta1_graus = Mathf.Round(theta1 * (180 / Mathf.PI) * 1000) / 1000;
//                        k = (Mathf.Pow(_B3[m, 0], 2) + Mathf.Pow(_B3[m, 1], 2) + Mathf.Pow(_B3[m, 2], 2) + Mathf.Pow(a1, 2) - 2 * a1 * (_B3[m, 0] * Mathf.Cos(theta1) + _B3[m, 1] * Mathf.Sin(theta1)) - Mathf.Pow(a2, 2) - Mathf.Pow(a3, 2) - Mathf.Pow(d4, 2)) / (2 * a2);
//                        theta3 = (Mathf.Atan2(d4, a3) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a3, 2) + Mathf.Pow(d4, 2) - Mathf.Pow(k, 2)), k));
//                        theta3_graus = theta3 * (180 / Mathf.PI);
//                        comparado_1 = 306.09f - theta3_graus;
//                        theta23 = Mathf.Atan2(_B3[m, 2] * (a3 + a2 * Mathf.Cos(theta3)) + (d4 + a2 * Mathf.Sin(theta3)) * (_B3[m, 1] * Mathf.Sin(theta1) + _B3[m, 0] * Mathf.Cos(theta1) - a1), -(d4 + (a2 * Mathf.Sin(theta3))) * _B1[m, 2] + (a3 + a2 * Mathf.Cos(theta3)) * (_B3[m, 1] * Mathf.Sin(theta1) + _B3[m, 0] * Mathf.Cos(theta1) - a1));
//                        theta23_graus = theta23 * (180 / Mathf.PI);
//                        theta2 = theta23 - theta3;
//                        theta2_graus = Mathf.Round(theta2 * (180 / Mathf.PI) * 1000) / 1000;
//                        comparado_2 = 144.58f - theta2_graus;
//                        theta5 = theta2 + theta3;
//                        theta5_graus = theta5 * (180 / Mathf.PI);
//                        comparado_3 = 79.6f + theta5_graus;
//                        dataQueue.Enqueue(theta1_graus);
//                        m++;
//                        verdadeiro = false;

//                        // Reinicia a transição entre ângulos
//                        emTransicao = false;
//                    }

//                    else
//                    {
//                        // Reinicia o ciclo partindo da posição final
//                        m = 0;
//                        targetRotationY = 0;
//                        curva = 1;
//                        emTransicao = false; // Permite nova movimentação
//                        break;
//                    }
//                }
//            }
//        }
//    }
//}

//// Função para rotacionar o objeto gradualmente em torno do eixo Y
//void RotateObjectSmooth()
//{
//    // Calcula a rotação atual do objeto em torno do eixo Y
//    currentRotationY = Mathf.LerpAngle(currentRotationY, targetRotationY, Time.deltaTime * velocidadeRotacao * direcao);

//    // Aplica a rotação gradual
//    elementos[0].localRotation = Quaternion.Euler(0f, -currentRotationY, 0f);
//}