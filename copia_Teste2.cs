using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class copia_Teste2 : MonoBehaviour
{
    [SerializeField] private Transform[] elementos1;

    // Variáveis relacionadas aos dados recebidos
    private float _PositionX_2, _PositionY_2, _PositionZ_2;
    private float pointx_0_2, pointx_1_2, pointx_2_2, pointx_3_2, pointx_4_2, pointx_5_2, pointx_6_2, pointx_7_2, pointx_8_2, pointx_9_2, pointx_10_2, pointx_11_2, pointx_12_2, pointx_13_2, pointx_14_2;
    private float pointy_0_2, pointy_1_2, pointy_2_2, pointy_3_2, pointy_4_2, pointy_5_2, pointy_6_2, pointy_7_2, pointy_8_2, pointy_9_2, pointy_10_2, pointy_11_2, pointy_12_2, pointy_13_2, pointy_14_2;
    private float pointz_0_2, pointz_1_2, pointz_2_2, pointz_3_2, pointz_4_2, pointz_5_2, pointz_6_2, pointz_7_2, pointz_8_2, pointz_9_2, pointz_10_2, pointz_11_2, pointz_12_2, pointz_13_2, pointz_14_2;
    private float theta11, theta1_graus1, theta21, theta2_graus1, theta231, theta23_graus1, theta31, theta3_graus1, theta51, theta5_graus1, a11 = 43.9f, a21 = 118f, a31 = 18.2f, d41 = 171.9f, k1;
    private float[,] _B1_2 = new float[21, 3];
    private float[,] _B2_2 = new float[21, 3];
    private float[,] _B3_2 = new float[21, 3];
    private List<float> dado_enviado = new List<float>();
    private int iniciar_2;

    // Variáveis de controle
    private int m1 = 0;
    private string[] separado1;
    private List<float> comparado_11 = new List<float>();
    private List<float> comparado_13 = new List<float>();
    private List<float> comparado_15 = new List<float>();
    private int curva2 = 1;
    private int currentAngleIndex2 = 0;

    // Variáveis para a comunicação UDP
    public UdpClient udpReceiver1;
    public Thread receiveThread1;
    private ConcurrentQueue<float> dataQueue1 = new ConcurrentQueue<float>();

    // Velocidade de rotação gradual
    private float velocidadeRotacao1; // Graus por segundo

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
    void Update()
    {
        // Verifica se há dados na fila para processar
        if (!emTransicao1)
        {
            if (currentAngleIndex2 < dado_enviado.Count)
            {
                Debug.Log("Entrou na rotação " + (currentAngleIndex2) + " do segundo motor. E o valor enviado foi: " + dado_enviado[currentAngleIndex2]);

                // Determina o ângulo de destino com base no dado recebido
                float targetAngle1 = dado_enviado[currentAngleIndex2];

                // Limita o ângulo dentro do intervalo permitido
                targetAngle1 = Mathf.Clamp(targetAngle1, anguloInicial1, anguloFinal1);

                // Determina a direção da rotação
                if (targetAngle1 > currentRotationZ)
                {
                    direcao1 = 1; // Rotação horária
                    velocidadeRotacao1 = 14f;
                }
                else
                {
                    direcao1 = -1; // Rotação anti-horária
                    velocidadeRotacao1 = -14f;
                }

                // Atualiza o ângulo de rotação alvo
                targetRotationZ = targetAngle1;

                // Inicia a transição
                emTransicao1 = true;
            }
        }

        // Rotaciona gradualmente o objeto em torno do eixo Y
        RotateObjectSmooth1();
        verificar1();
    }

    // Método para verificar se a rotação atingiu o ângulo desejado
    void verificar1()
    {
        //Debug.Log("Valor Z: " + Mathf.Round((elementos1[1].localEulerAngles.z) * 1000f) / 1000f + " Valor do Theta2: " + Mathf.Round(theta2_graus1 * 1000f) / 1000f);
        float angulo_euler12 = Mathf.Abs(Mathf.Abs(elementos1[0].localEulerAngles.y) - 360);
        float angulo_euler22 = Mathf.Abs(Mathf.Abs(elementos1[1].localEulerAngles.z) - 360);
        float angulo_euler32 = Mathf.Abs(Mathf.Abs(elementos1[2].localEulerAngles.z) - 360);
        float angulo_euler52 = Mathf.Abs(Mathf.Abs(elementos1[3].localEulerAngles.z) - 360);

        if (comparado_11[currentAngleIndex2] <= 0)
        {
            comparado_11[currentAngleIndex2] += 360;
        }
        if (dado_enviado[currentAngleIndex2] < 0)
        {
            dado_enviado[currentAngleIndex2] += 360;
        }
        if (comparado_13[currentAngleIndex2] > 360 || comparado_13[currentAngleIndex2] < 0)
        {
            if (comparado_13[currentAngleIndex2] > 360)
            {
                comparado_13[currentAngleIndex2] -= 360;
            }
            else
            {
                comparado_13[currentAngleIndex2] += 360;
            }
        }
        if (comparado_15[currentAngleIndex2] < 0)
        {
            comparado_15[currentAngleIndex2] += 360;
        }

        float dif12 = Mathf.Abs(angulo_euler12 - comparado_11[currentAngleIndex2]);
        float dif22 = Mathf.Abs(angulo_euler22 - dado_enviado[currentAngleIndex2]);
        float dif32 = Mathf.Abs(angulo_euler32 - comparado_13[currentAngleIndex2]);
        float dif52 = Mathf.Abs(angulo_euler52 - comparado_15[currentAngleIndex2]);

        //Debug.Log("Theta 2 lido no motor 2 " + angulo_euler22 + ". Angulo 2 enviado " + dado_enviado[currentAngleIndex2] + ". A diferença é: " + dif22);
        //Debug.Log("Theta 5 lido no motor 2 " + angulo_euler52 + ". Angulo 5 enviado " + comparado_15[currentAngleIndex2] + ". A diferença é: " + dif52);

        if (/*dif12 < 0.03f && dif22 < 0.03f &&*/ dif32 < 0.03f && dif52 < 0.03f)
        {
            Debug.Log("O valor lido depois de entrar na condição é: " + angulo_euler22 + ". O valor enviado foi: " + dado_enviado[currentAngleIndex2] + ". A diferença é: " + dif22);
            emTransicao1 = false;
            if(dado_enviado.Count == (currentAngleIndex2 + 1))
            {
                elementos1[1].localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            if(currentAngleIndex2 < 59)
            {
                currentAngleIndex2++;
            }
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
            pointx_0_2 = float.Parse(separado1[0]);
            pointy_0_2 = float.Parse(separado1[1]);
            pointz_0_2 = float.Parse(separado1[2]);
            pointx_1_2 = float.Parse(separado1[3]);
            pointy_1_2 = float.Parse(separado1[4]);
            pointz_1_2 = float.Parse(separado1[5]);
            pointx_2_2 = float.Parse(separado1[6]);
            pointy_2_2 = float.Parse(separado1[7]);
            pointz_2_2 = float.Parse(separado1[8]);
            pointx_3_2 = float.Parse(separado1[9]);
            pointy_3_2 = float.Parse(separado1[10]);
            pointz_3_2 = float.Parse(separado1[11]);
            pointx_4_2 = float.Parse(separado1[12]);
            pointy_4_2 = float.Parse(separado1[13]);
            pointz_4_2 = float.Parse(separado1[14]);
            pointx_5_2 = float.Parse(separado1[15]);
            pointy_5_2 = float.Parse(separado1[16]);
            pointz_5_2 = float.Parse(separado1[17]);
            pointx_6_2 = float.Parse(separado1[18]);
            pointy_6_2 = float.Parse(separado1[19]);
            pointz_6_2 = float.Parse(separado1[20]);
            pointx_7_2 = float.Parse(separado1[21]);
            pointy_7_2 = float.Parse(separado1[22]);
            pointz_7_2 = float.Parse(separado1[23]);
            pointx_8_2 = float.Parse(separado1[24]);
            pointy_8_2 = float.Parse(separado1[25]);
            pointz_8_2 = float.Parse(separado1[26]);
            pointx_9_2 = float.Parse(separado1[27]);
            pointy_9_2 = float.Parse(separado1[28]);
            pointz_9_2 = float.Parse(separado1[29]);
            pointx_10_2 = float.Parse(separado1[30]);
            pointy_10_2 = float.Parse(separado1[31]);
            pointz_10_2 = float.Parse(separado1[32]);
            pointx_11_2 = float.Parse(separado1[33]);
            pointy_11_2 = float.Parse(separado1[34]);
            pointz_11_2 = float.Parse(separado1[35]);
            pointx_12_2 = float.Parse(separado1[36]);
            pointy_12_2 = float.Parse(separado1[37]);
            pointz_12_2 = float.Parse(separado1[38]);
            pointx_13_2 = float.Parse(separado1[39]);
            pointy_13_2 = float.Parse(separado1[40]);
            pointz_13_2 = float.Parse(separado1[41]);
            pointx_14_2 = float.Parse(separado1[42]);
            pointy_14_2 = float.Parse(separado1[43]);
            pointz_14_2 = float.Parse(separado1[44]);
            iniciar_2 = int.Parse(separado1[45]);

            if (iniciar_2 > 0)
            {
                curva2 = 1;
                m1 = 0;
                currentAngleIndex2 = 0;
                comparado_11.Clear();
                comparado_13.Clear();
                comparado_15.Clear();
                Calcular_Angulos();
            }
        }

        void Calcular_Angulos()
        {
            // Cálculo dos pontos de Bézier: Primeira Curva
            int i = 0;

            for (float t = 0; t <= 1; t += 0.05f)
            {
                int j = 0;

                _PositionX_2 = Mathf.Pow(1 - t, 4) * pointx_0_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointx_1_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointx_2_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointx_3_2 + Mathf.Pow(t, 4) * pointx_4_2;
                _PositionY_2 = Mathf.Pow(1 - t, 4) * pointy_0_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointy_1_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointy_2_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointy_3_2 + Mathf.Pow(t, 4) * pointy_4_2;
                _PositionZ_2 = Mathf.Pow(1 - t, 4) * pointz_0_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointz_1_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointz_2_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointz_3_2 + Mathf.Pow(t, 4) * pointz_4_2;

                _B1_2[i, j] = _PositionX_2;
                j++;
                _B1_2[i, j] = _PositionY_2;
                j++;
                _B1_2[i, j] = _PositionZ_2;
                i++;
            }

            //Cálculo dos pontos de Bézier: Segunda Curva

            i = 0;
            for (float t = 0; t <= 1; t += 0.05f)
            {
                int j = 0;

                _PositionX_2 = Mathf.Pow(1 - t, 4) * pointx_5_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointx_6_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointx_7_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointx_8_2 + Mathf.Pow(t, 4) * pointx_9_2;
                _PositionY_2 = Mathf.Pow(1 - t, 4) * pointy_5_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointy_6_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointy_7_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointy_8_2 + Mathf.Pow(t, 4) * pointy_9_2;
                _PositionZ_2 = Mathf.Pow(1 - t, 4) * pointz_5_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointz_6_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointz_7_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointz_8_2 + Mathf.Pow(t, 4) * pointz_9_2;

                _B2_2[i, j] = _PositionX_2;
                j++;
                _B2_2[i, j] = _PositionY_2;
                j++;
                _B2_2[i, j] = _PositionZ_2;
                i++;
            }

            //Cálculo dos pontos de Bézier: Terceira Curva

            i = 0;
            for (float t = 0; t <= 1; t += 0.05f)
            {
                int j = 0;

                _PositionX_2 = Mathf.Pow(1 - t, 4) * pointx_10_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointx_11_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointx_12_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointx_13_2 + Mathf.Pow(t, 4) * pointx_14_2;
                _PositionY_2 = Mathf.Pow(1 - t, 4) * pointy_10_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointy_11_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointy_12_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointy_13_2 + Mathf.Pow(t, 4) * pointy_14_2;
                _PositionZ_2 = Mathf.Pow(1 - t, 4) * pointz_10_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointz_11_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointz_12_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointz_13_2 + Mathf.Pow(t, 4) * pointz_14_2;

                _B3_2[i, j] = _PositionX_2;
                j++;
                _B3_2[i, j] = _PositionY_2;
                j++;
                _B3_2[i, j] = _PositionZ_2;
                i++;
            }

            // Cálculo do ângulo de rotação
            for (int c = 0; c < 65; c++)
            {
                if (curva2 == 1)
                {
                    if (m1 < 20)
                    {
                        //theta11 = (Mathf.Atan2(_B1_2[m1, 1], _B1_2[m1, 0]));
                        //theta1_graus1 = theta11 * (180 / Mathf.PI);
                        //comparado_11.Add(theta1_graus1);
                        //k1 = (Mathf.Pow(_B1_2[m1, 0], 2) + Mathf.Pow(_B1_2[m1, 1], 2) + Mathf.Pow(_B1_2[m1, 2], 2) + Mathf.Pow(a11, 2) - 2 * a11 * (_B1_2[m1, 0] * Mathf.Cos(theta11) + _B1_2[m1, 1] * Mathf.Sin(theta11)) - Mathf.Pow(a21, 2) - Mathf.Pow(a31, 2) - Mathf.Pow(d41, 2)) / (2 * a21);
                        //theta31 = (Mathf.Atan2(d41, a31) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a31, 2) + Mathf.Pow(d41, 2) - Mathf.Pow(k1, 2)), k1));
                        //theta3_graus1 = theta31 * (180 / Mathf.PI);
                        //comparado_13.Add(306.09f - theta3_graus1);
                        //theta231 = Mathf.Atan2(_B1_2[m1, 2] * (a31 + a21 * Mathf.Cos(theta31)) + (d41 + a21 * Mathf.Sin(theta31)) * (_B1_2[m1, 1] * Mathf.Sin(theta11) + _B1_2[m1, 0] * Mathf.Cos(theta11) - a11), -(d41 + (a21 * Mathf.Sin(theta31))) * _B1_2[m1, 2] + (a31 + a21 * Mathf.Cos(theta31)) * (_B1_2[m1, 1] * Mathf.Sin(theta11) + _B1_2[m1, 0] * Mathf.Cos(theta11) - a11));
                        //theta23_graus1 = theta231 * (180 / Mathf.PI);
                        //theta21 = theta23_graus1 - theta3_graus1;
                        //theta2_graus1 = theta21;
                        //dado_enviado.Add(144.58f - theta2_graus1);
                        //theta5_graus1 = theta2_graus1 + theta3_graus1;
                        //comparado_15.Add(79.6f + theta5_graus1);
                        //m1++;


                        theta11 = (Mathf.Atan2(_B1_2[m1, 1], _B1_2[m1, 0]));
                        theta1_graus1 = theta11 * (180 / Mathf.PI);
                        comparado_11.Add(theta1_graus1);
                        k1 = (Mathf.Pow(_B1_2[m1, 0], 2) + Mathf.Pow(_B1_2[m1, 1], 2) + Mathf.Pow(_B1_2[m1, 2], 2) + Mathf.Pow(a11, 2) - 2 * a11 * (_B1_2[m1, 0] * Mathf.Cos(theta11) + _B1_2[m1, 1] * Mathf.Sin(theta11)) - Mathf.Pow(a21, 2) - Mathf.Pow(a31, 2) - Mathf.Pow(d41, 2)) / (2 * a21);
                        theta31 = (Mathf.Atan2(d41, a31) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a31, 2) + Mathf.Pow(d41, 2) - Mathf.Pow(k1, 2)), k1));
                        theta3_graus1 = theta31 * (180 / Mathf.PI);
                        comparado_13.Add(306.09f - theta3_graus1);
                        theta231 = Mathf.Atan2(_B1_2[m1, 2] * (a31 + a21 * Mathf.Cos(theta31)) + (d41 + a21 * Mathf.Sin(theta31)) * (_B1_2[m1, 1] * Mathf.Sin(theta11) + _B1_2[m1, 0] * Mathf.Cos(theta11) - a11), -(d41 + (a21 * Mathf.Sin(theta31))) * _B1_2[m1, 2] + (a31 + a21 * Mathf.Cos(theta31)) * (_B1_2[m1, 1] * Mathf.Sin(theta11) + _B1_2[m1, 0] * Mathf.Cos(theta11) - a11));
                        theta23_graus1 = theta231 * (180 / Mathf.PI);
                        theta21 = theta231 - theta31;
                        theta2_graus1 = theta21 * (180 / Mathf.PI);
                        dado_enviado.Add(144.58f - theta2_graus1);
                        theta51 = theta21 + theta31;
                        theta5_graus1 = theta51 * (180 / Mathf.PI);
                        comparado_15.Add(79.6f + theta5_graus1);
                        m1++;
                    }
                    else
                    {
                        //Atualiza o número da curva
                        curva2++;

                        //Reinicia o ciclo partindo da posição final
                        m1 = 0;
                    }
                }

                else
                {
                    if (curva2 == 2)
                    {
                        if (m1 < 20)
                        {
                            theta11 = (Mathf.Atan2(_B2_2[m1, 1], _B2_2[m1, 0]));
                            theta1_graus1 = theta11 * (180 / Mathf.PI);
                            comparado_11.Add(theta1_graus1);
                            k1 = (Mathf.Pow(_B2_2[m1, 0], 2) + Mathf.Pow(_B2_2[m1, 1], 2) + Mathf.Pow(_B2_2[m1, 2], 2) + Mathf.Pow(a11, 2) - 2 * a11 * (_B2_2[m1, 0] * Mathf.Cos(theta11) + _B2_2[m1, 1] * Mathf.Sin(theta11)) - Mathf.Pow(a21, 2) - Mathf.Pow(a31, 2) - Mathf.Pow(d41, 2)) / (2 * a21);
                            theta31 = (Mathf.Atan2(d41, a31) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a31, 2) + Mathf.Pow(d41, 2) - Mathf.Pow(k1, 2)), k1));
                            theta3_graus1 = theta31 * (180 / Mathf.PI);
                            comparado_13.Add(306.09f - theta3_graus1);
                            theta231 = Mathf.Atan2(_B2_2[m1, 2] * (a31 + a21 * Mathf.Cos(theta31)) + (d41 + a21 * Mathf.Sin(theta31)) * (_B2_2[m1, 1] * Mathf.Sin(theta11) + _B2_2[m1, 0] * Mathf.Cos(theta11) - a11), -(d41 + (a21 * Mathf.Sin(theta31))) * _B2_2[m1, 2] + (a31 + a21 * Mathf.Cos(theta31)) * (_B2_2[m1, 1] * Mathf.Sin(theta11) + _B2_2[m1, 0] * Mathf.Cos(theta11) - a11));
                            theta23_graus1 = theta231 * (180 / Mathf.PI);
                            theta21 = theta231 - theta31;
                            theta2_graus1 = theta21 * (180 / Mathf.PI);
                            dado_enviado.Add(144.58f - theta2_graus1);
                            theta51 = theta21 + theta31;
                            theta5_graus1 = theta51 * (180 / Mathf.PI);
                            comparado_15.Add(79.6f + theta5_graus1);
                            m1++;
                        }

                        else
                        {
                            //Atualiza o número da curva
                            curva2++;

                            //Reinicia o ciclo partindo da posição final
                            m1 = 0;
                        }

                    }

                    else
                    {
                        if (curva2 == 3)
                        {
                            if (m1 < 20)
                            {
                                theta11 = Mathf.Atan2(_B3_2[m1, 1], _B3_2[m1, 0]);
                                theta1_graus1 = theta11 * (180 / Mathf.PI);
                                comparado_11.Add(theta1_graus1);
                                k1 = (Mathf.Pow(_B3_2[m1, 0], 2) + Mathf.Pow(_B3_2[m1, 1], 2) + Mathf.Pow(_B3_2[m1, 2], 2) + Mathf.Pow(a11, 2) - 2 * a11 * (_B3_2[m1, 0] * Mathf.Cos(theta11) + _B3_2[m1, 1] * Mathf.Sin(theta11)) - Mathf.Pow(a21, 2) - Mathf.Pow(a31, 2) - Mathf.Pow(d41, 2)) / (2 * a21);
                                theta31 = (Mathf.Atan2(d41, a31) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a31, 2) + Mathf.Pow(d41, 2) - Mathf.Pow(k1, 2)), k1));
                                theta3_graus1 = theta31 * (180 / Mathf.PI);
                                comparado_13.Add(306.09f - theta3_graus1);
                                theta231 = Mathf.Atan2(_B3_2[m1, 2] * (a31 + a21 * Mathf.Cos(theta31)) + (d41 + a21 * Mathf.Sin(theta31)) * (_B3_2[m1, 1] * Mathf.Sin(theta11) + _B3_2[m1, 0] * Mathf.Cos(theta11) - a11), -(d41 + (a21 * Mathf.Sin(theta31))) * _B3_2[m1, 2] + (a31 + a21 * Mathf.Cos(theta31)) * (_B3_2[m1, 1] * Mathf.Sin(theta11) + _B3_2[m1, 0] * Mathf.Cos(theta11) - a11));
                                theta23_graus1 = theta231 * (180 / Mathf.PI);
                                theta21 = theta231 - theta31;
                                theta2_graus1 = theta21 * (180 / Mathf.PI);
                                dado_enviado.Add(144.58f - theta2_graus1);
                                theta51 = theta21 + theta31;
                                theta5_graus1 = theta51 * (180 / Mathf.PI);
                                comparado_15.Add(79.6f + theta5_graus1);
                                m1++;

                                // Reinicia a transição entre ângulos
                                emTransicao1 = false;
                            }

                            else
                            {
                                //Atualiza o número da curva
                                curva2 = 0;

                                //Reinicia o ciclo partindo da posição final
                                m1 = 0;
                            }
                        }
                    }
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



//using UnityEngine;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Collections.Concurrent;

//public class copia_Teste2 : MonoBehaviour
//{
//    [SerializeField] private Transform[] elementos1;

//    // Variáveis relacionadas aos dados recebidos
//    private float _PositionX_2, _PositionY_2, _PositionZ_2;
//    private float pointx_0_2, pointx_1_2, pointx_2_2, pointx_3_2, pointx_4_2, pointx_5_2, pointx_6_2, pointx_7_2, pointx_8_2, pointx_9_2, pointx_10_2, pointx_11_2, pointx_12_2, pointx_13_2, pointx_14_2;
//    private float pointy_0_2, pointy_1_2, pointy_2_2, pointy_3_2, pointy_4_2, pointy_5_2, pointy_6_2, pointy_7_2, pointy_8_2, pointy_9_2, pointy_10_2, pointy_11_2, pointy_12_2, pointy_13_2, pointy_14_2;
//    private float pointz_0_2, pointz_1_2, pointz_2_2, pointz_3_2, pointz_4_2, pointz_5_2, pointz_6_2, pointz_7_2, pointz_8_2, pointz_9_2, pointz_10_2, pointz_11_2, pointz_12_2, pointz_13_2, pointz_14_2;
//    private float theta11, theta1_graus1, theta21, theta2_graus1, theta231, theta23_graus1, theta31, theta3_graus1, theta51, theta5_graus1, a11 = 43.9f, a21 = 118f, a31 = 18.2f, d41 = 171.9f, k1;
//    private float[,] _B1_2 = new float[21, 3];
//    private float[,] _B2_2 = new float[21, 3];
//    private float[,] _B3_2 = new float[21, 3];
//    private float dado_enviado;

//    // Variáveis de controle
//    private float theta2_graus_old = 0f;
//    private bool verdadeiro1 = false;
//    private int m1 = 0;
//    private int l1 = 0;
//    private string[] separado1;
//    private List<float> comparado_11 = new List<float>();
//    private List<float> comparado_12 = new List<float>();
//    private List<float> comparado_13 = new List<float>();
//    private int curva2 = 1;

//    // Variáveis para a comunicação UDP
//    public UdpClient udpReceiver1;
//    public Thread receiveThread1;
//    private ConcurrentQueue<float> dataQueue1 = new ConcurrentQueue<float>();

//    // Velocidade de rotação gradual
//    private float velocidadeRotacao1; // Graus por segundo

//    // Rotação atual e destino
//    private float currentRotationZ;
//    private float targetRotationZ;

//    // Ângulos de destino específicos
//    private float anguloInicial1 = -180f;
//    private float anguloFinal1 = 180f;

//    // Direção da rotação (-1 para anti-horário, 1 para horário)
//    private int direcao1 = 1; // Por padrão, rotação horária

//    // Flag para controlar transição entre rotações
//    private bool emTransicao1 = false;

//    // Start is called before the first frame update
//    void Start()
//    {
//        udpReceiver1 = new UdpClient(61559);
//        receiveThread1 = new Thread(new ThreadStart(_ReceiveData1));
//        receiveThread1.Start();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (l1 != 0)
//        {
//            verificar1();
//        }

//        // Verifica se há dados na fila para processar
//        if (dataQueue1.Count > 0 && !emTransicao1)
//        {
//            if (dataQueue1.TryDequeue(out float dados1))
//            {
//                //Debug.Log("Removido objeto: " + dados1 + ". Contagem de elementos na fila: " + dataQueue1.Count);

//                // Determina o ângulo de destino com base no dado recebido
//                float targetAngle1 = dados1;

//                // Limita o ângulo dentro do intervalo permitido
//                targetAngle1 = Mathf.Clamp(targetAngle1, anguloInicial1, anguloFinal1);

//                // Determina a direção da rotação
//                if (dados1 > theta2_graus_old)
//                {
//                    direcao1 = 1; // Rotação horária
//                    velocidadeRotacao1 = 14f;
//                }
//                else
//                {
//                    direcao1 = -1; // Rotação anti-horária
//                    velocidadeRotacao1 = -14f;
//                }

//                // Atualiza o ângulo de rotação alvo
//                targetRotationZ = targetAngle1;

//                // Inicia a transição
//                emTransicao1 = true;

//                // Reinicia a flag de verificação
//                verdadeiro1 = false;
//            }
//        }

//        // Rotaciona gradualmente o objeto em torno do eixo Y
//        RotateObjectSmooth1();
//    }

//    // Método para verificar se a rotação atingiu o ângulo desejado
//    void verificar1()
//    {
//        //Debug.Log("Valor Z: " + Mathf.Round((elementos1[1].localEulerAngles.z) * 1000f) / 1000f + " Valor do Theta2: " + Mathf.Round(theta2_graus1 * 1000f) / 1000f);
//        float angulo_euler12 = Mathf.Abs(Mathf.Round(elementos1[0].localEulerAngles.y) - 360);
//        float angulo_euler22 = Mathf.Abs(Mathf.Round(elementos1[1].localEulerAngles.z) - 360);
//        float angulo_euler32 = Mathf.Abs(Mathf.Round(elementos1[2].localEulerAngles.z) - 360);
//        float angulo_euler52 = Mathf.Abs(Mathf.Round(elementos1[3].localEulerAngles.z) - 360);

//        if (theta1_graus1 <= 0)
//        {
//            theta1_graus1 += 360;
//        }
//        if (theta2_graus1 < 0)
//        {
//            theta2_graus1 += 360;
//        }
//        if (theta3_graus1 < 0)
//        {
//            theta3_graus1 += 360;
//        }
//        if (comparado_11 > 360)
//        {
//            comparado_11 -= 360;
//        }
//        if (theta5_graus1 < 0)
//        {
//            theta5_graus1 += 360;
//        }

//        float dif12 = Mathf.Round((float)angulo_euler12 - theta1_graus1);
//        float dif22 = Mathf.Round((float)angulo_euler22 - dado_enviado);
//        float dif32 = Mathf.Round((float)angulo_euler32 - comparado_11);
//        float dif52 = Mathf.Round((float)angulo_euler52 - comparado_13);

//        if (Mathf.Abs(dif12) < 0.03f && Mathf.Abs(dif22) < 0.03f && Mathf.Abs(dif32) < 0.03f && Mathf.Abs(dif52) < 0.03f)
//        {
//            Debug.Log("O valor lido é: " + angulo_euler22 + ". O valor enviado foi: " + dado_enviado + ". O valor de theta 2 foi: " + theta2_graus1 + ". A diferença é: " + dif22);
//            verdadeiro1 = true;
//        }
//    }

//    // Método para encerrar a thread de recebimento ao destruir o objeto
//    void OnDestroy1()
//    {
//        receiveThread1.Abort();
//    }

//    // Função principal de recebimento e processamento de dados UDP
//    void _ReceiveData1()
//    {
//        while (true)
//        {
//            IPEndPoint remoteEndPoint1 = new IPEndPoint(IPAddress.Any, 0);
//            byte[] receiveBytes1 = udpReceiver1.Receive(ref remoteEndPoint1);
//            string receivedData1 = Encoding.ASCII.GetString(receiveBytes1);

//            Debug.Log("Received data in teste2: " + receivedData1);

//            string[] separado1 = receivedData1.Split(';');

//            // Parse dos pontos recebidos
//            pointx_0_2 = float.Parse(separado1[0]);
//            pointy_0_2 = float.Parse(separado1[1]);
//            pointz_0_2 = float.Parse(separado1[2]);
//            pointx_1_2 = float.Parse(separado1[3]);
//            pointy_1_2 = float.Parse(separado1[4]);
//            pointz_1_2 = float.Parse(separado1[5]);
//            pointx_2_2 = float.Parse(separado1[6]);
//            pointy_2_2 = float.Parse(separado1[7]);
//            pointz_2_2 = float.Parse(separado1[8]);
//            pointx_3_2 = float.Parse(separado1[9]);
//            pointy_3_2 = float.Parse(separado1[10]);
//            pointz_3_2 = float.Parse(separado1[11]);
//            pointx_4_2 = float.Parse(separado1[12]);
//            pointy_4_2 = float.Parse(separado1[13]);
//            pointz_4_2 = float.Parse(separado1[14]);
//            pointx_5_2 = float.Parse(separado1[15]);
//            pointy_5_2 = float.Parse(separado1[16]);
//            pointz_5_2 = float.Parse(separado1[17]);
//            pointx_6_2 = float.Parse(separado1[18]);
//            pointy_6_2 = float.Parse(separado1[19]);
//            pointz_6_2 = float.Parse(separado1[20]);
//            pointx_7_2 = float.Parse(separado1[21]);
//            pointy_7_2 = float.Parse(separado1[22]);
//            pointz_7_2 = float.Parse(separado1[23]);
//            pointx_8_2 = float.Parse(separado1[24]);
//            pointy_8_2 = float.Parse(separado1[25]);
//            pointz_8_2 = float.Parse(separado1[26]);
//            pointx_9_2 = float.Parse(separado1[27]);
//            pointy_9_2 = float.Parse(separado1[28]);
//            pointz_9_2 = float.Parse(separado1[29]);
//            pointx_10_2 = float.Parse(separado1[30]);
//            pointy_10_2 = float.Parse(separado1[31]);
//            pointz_10_2 = float.Parse(separado1[32]);
//            pointx_11_2 = float.Parse(separado1[33]);
//            pointy_11_2 = float.Parse(separado1[34]);
//            pointz_11_2 = float.Parse(separado1[35]);
//            pointx_12_2 = float.Parse(separado1[36]);
//            pointy_12_2 = float.Parse(separado1[37]);
//            pointz_12_2 = float.Parse(separado1[38]);
//            pointx_13_2 = float.Parse(separado1[39]);
//            pointy_13_2 = float.Parse(separado1[40]);
//            pointz_13_2 = float.Parse(separado1[41]);
//            pointx_14_2 = float.Parse(separado1[42]);
//            pointy_14_2 = float.Parse(separado1[43]);
//            pointz_14_2 = float.Parse(separado1[44]);

//            // Cálculo dos pontos de Bézier: Primeira Curva
//            int i = 0;

//            for (float t = 0; t <= 1; t += 0.05f)
//            {
//                int j = 0;

//                _PositionX_2 = Mathf.Pow(1 - t, 4) * pointx_0_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointx_1_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointx_2_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointx_3_2 + Mathf.Pow(t, 4) * pointx_4_2;
//                _PositionY_2 = Mathf.Pow(1 - t, 4) * pointy_0_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointy_1_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointy_2_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointy_3_2 + Mathf.Pow(t, 4) * pointy_4_2;
//                _PositionZ_2 = Mathf.Pow(1 - t, 4) * pointz_0_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointz_1_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointz_2_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointz_3_2 + Mathf.Pow(t, 4) * pointz_4_2;

//                _B1_2[i, j] = _PositionX_2;
//                j++;
//                _B1_2[i, j] = _PositionY_2;
//                j++;
//                _B1_2[i, j] = _PositionZ_2;
//                i++;
//            }

//            //Cálculo dos pontos de Bézier: Segunda Curva

//            i = 0;
//            for (float t = 0; t <= 1; t += 0.05f)
//            {
//                int j = 0;

//                _PositionX_2 = Mathf.Pow(1 - t, 4) * pointx_5_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointx_6_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointx_7_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointx_8_2 + Mathf.Pow(t, 4) * pointx_9_2;
//                _PositionY_2 = Mathf.Pow(1 - t, 4) * pointy_5_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointy_6_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointy_7_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointy_8_2 + Mathf.Pow(t, 4) * pointy_9_2;
//                _PositionZ_2 = Mathf.Pow(1 - t, 4) * pointz_5_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointz_6_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointz_7_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointz_8_2 + Mathf.Pow(t, 4) * pointz_9_2;

//                _B2_2[i, j] = _PositionX_2;
//                j++;
//                _B2_2[i, j] = _PositionY_2;
//                j++;
//                _B2_2[i, j] = _PositionZ_2;
//                i++;
//            }

//            //Cálculo dos pontos de Bézier: Terceira Curva

//            i = 0;
//            for (float t = 0; t <= 1; t += 0.05f)
//            {
//                int j = 0;

//                _PositionX_2 = Mathf.Pow(1 - t, 4) * pointx_10_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointx_11_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointx_12_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointx_13_2 + Mathf.Pow(t, 4) * pointx_14_2;
//                _PositionY_2 = Mathf.Pow(1 - t, 4) * pointy_10_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointy_11_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointy_12_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointy_13_2 + Mathf.Pow(t, 4) * pointy_14_2;
//                _PositionZ_2 = Mathf.Pow(1 - t, 4) * pointz_10_2 + 4 * Mathf.Pow(1 - t, 3) * t * pointz_11_2 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * pointz_12_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * pointz_13_2 + Mathf.Pow(t, 4) * pointz_14_2;

//                _B3_2[i, j] = _PositionX_2;
//                j++;
//                _B3_2[i, j] = _PositionY_2;
//                j++;
//                _B3_2[i, j] = _PositionZ_2;
//                i++;
//            }

//            // Cálculo do ângulo de rotação
//            if (curva2 == 1)
//            {
//                if (l1 == 0)
//                {
//                    Debug.Log("O incremento m2 é: " + m1);
//                    l1++;
//                    theta11 = (Mathf.Atan2(_B1_2[m1, 1], _B1_2[m1, 0]));
//                    theta1_graus1 = Mathf.Round(theta11 * (180 / Mathf.PI) * 1000) / 1000;
//                    k1 = (Mathf.Pow(_B1_2[m1, 0], 2) + Mathf.Pow(_B1_2[m1, 1], 2) + Mathf.Pow(_B1_2[m1, 2], 2) + Mathf.Pow(a11, 2) - 2 * a11 * (_B1_2[m1, 0] * Mathf.Cos(theta11) + _B1_2[m1, 1] * Mathf.Sin(theta11)) - Mathf.Pow(a21, 2) - Mathf.Pow(a31, 2) - Mathf.Pow(d41, 2)) / (2 * a21);
//                    theta31 = (Mathf.Atan2(d41, a31) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a31, 2) + Mathf.Pow(d41, 2) - Mathf.Pow(k1, 2)), k1));
//                    theta3_graus1 = Mathf.Round(theta31 * (180 / Mathf.PI) * 1000) / 1000;
//                    comparado_11 = 306.09f - theta3_graus1;
//                    theta231 = Mathf.Atan2(_B1_2[m1, 2] * (a31 + a21 * Mathf.Cos(theta31)) + (d41 + a21 * Mathf.Sin(theta31)) * (_B1_2[m1, 1] * Mathf.Sin(theta11) + _B1_2[m1, 0] * Mathf.Cos(theta11) - a11), -(d41 + (a21 * Mathf.Sin(theta31))) * _B1_2[m1, 2] + (a31 + a21 * Mathf.Cos(theta31)) * (_B1_2[m1, 1] * Mathf.Sin(theta11) + _B1_2[m1, 0] * Mathf.Cos(theta11) - a11));
//                    theta23_graus1 = Mathf.Round(theta231 * (180 / Mathf.PI) * 1000) / 1000;
//                    theta21 = theta231 - theta31;
//                    theta2_graus1 = Mathf.Round(theta21 * (180 / Mathf.PI) * 1000) / 1000;
//                    theta51 = theta21 + theta31;
//                    theta5_graus1 = theta51 * (180 / Mathf.PI);
//                    comparado_13 = 79.6f + theta5_graus1;
//                    dado_enviado = 144.58f - theta2_graus1;
//                    m1++;
//                    dataQueue1.Enqueue(dado_enviado);
//                }

//                else
//                {
//                    if (verdadeiro1 == true)
//                    {
//                        theta2_graus_old = theta2_graus1;
//                        if (m1 < 21)
//                        {
//                            Debug.Log("O incremento m2 é: " + m1);
//                            theta11 = (Mathf.Atan2(_B1_2[m1, 1], _B1_2[m1, 0]));
//                            theta1_graus1 = Mathf.Round(theta11 * (180 / Mathf.PI) * 1000) / 1000;
//                            k1 = (Mathf.Pow(_B1_2[m1, 0], 2) + Mathf.Pow(_B1_2[m1, 1], 2) + Mathf.Pow(_B1_2[m1, 2], 2) + Mathf.Pow(a11, 2) - 2 * a11 * (_B1_2[m1, 0] * Mathf.Cos(theta11) + _B1_2[m1, 1] * Mathf.Sin(theta11)) - Mathf.Pow(a21, 2) - Mathf.Pow(a31, 2) - Mathf.Pow(d41, 2)) / (2 * a21);
//                            theta31 = (Mathf.Atan2(d41, a31) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a31, 2) + Mathf.Pow(d41, 2) - Mathf.Pow(k1, 2)), k1));
//                            theta3_graus1 = Mathf.Round(theta31 * (180 / Mathf.PI) * 1000) / 1000;
//                            comparado_11 = 306.09f - theta3_graus1;
//                            theta231 = Mathf.Atan2(_B1_2[m1, 2] * (a31 + a21 * Mathf.Cos(theta31)) + (d41 + a21 * Mathf.Sin(theta31)) * (_B1_2[m1, 1] * Mathf.Sin(theta11) + _B1_2[m1, 0] * Mathf.Cos(theta11) - a11), -(d41 + (a21 * Mathf.Sin(theta31))) * _B1_2[m1, 2] + (a31 + a21 * Mathf.Cos(theta31)) * (_B1_2[m1, 1] * Mathf.Sin(theta11) + _B1_2[m1, 0] * Mathf.Cos(theta11) - a11));
//                            theta23_graus1 = theta231 * (180 / Mathf.PI);
//                            theta21 = theta231 - theta31;
//                            theta2_graus1 = Mathf.Round(theta21 * (180 / Mathf.PI) * 1000) / 1000;
//                            theta51 = theta21 + theta31;
//                            theta5_graus1 = theta51 * (180 / Mathf.PI);
//                            comparado_13 = 79.6f + theta5_graus1;
//                            dado_enviado = 144.58f - theta2_graus1;
//                            m1++;
//                            dataQueue1.Enqueue(dado_enviado);
//                            verdadeiro1 = false;

//                            // Reinicia a transição entre ângulos
//                            emTransicao1 = false;
//                        }

//                        else
//                        {
//                            //Atualiza o número da curva
//                            curva2++;

//                            //Reinicia o ciclo partindo da posição final
//                            m1 = 0;
//                            targetRotationZ = dado_enviado;
//                            emTransicao1 = false;
//                        }
//                    }
//                }
//            }

//            else
//            {
//                if (curva2 == 2)
//                {
//                    if (verdadeiro1 == true)
//                    {
//                        theta2_graus_old = theta2_graus1;
//                        if (m1 < 21)
//                        {
//                            Debug.Log("O incremento m2 é: " + m1);
//                            theta11 = (Mathf.Atan2(_B2_2[m1, 1], _B2_2[m1, 0]));
//                            theta1_graus1 = Mathf.Round(theta11 * (180 / Mathf.PI) * 1000) / 1000;
//                            k1 = (Mathf.Pow(_B2_2[m1, 0], 2) + Mathf.Pow(_B2_2[m1, 1], 2) + Mathf.Pow(_B2_2[m1, 2], 2) + Mathf.Pow(a11, 2) - 2 * a11 * (_B2_2[m1, 0] * Mathf.Cos(theta11) + _B2_2[m1, 1] * Mathf.Sin(theta11)) - Mathf.Pow(a21, 2) - Mathf.Pow(a31, 2) - Mathf.Pow(d41, 2)) / (2 * a21);
//                            theta31 = (Mathf.Atan2(d41, a31) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a31, 2) + Mathf.Pow(d41, 2) - Mathf.Pow(k1, 2)), k1));
//                            theta3_graus1 = Mathf.Round(theta31 * (180 / Mathf.PI) * 1000) / 1000;
//                            comparado_11 = 306.09f - theta3_graus1;
//                            theta231 = Mathf.Atan2(_B2_2[m1, 2] * (a31 + a21 * Mathf.Cos(theta31)) + (d41 + a21 * Mathf.Sin(theta31)) * (_B2_2[m1, 1] * Mathf.Sin(theta11) + _B2_2[m1, 0] * Mathf.Cos(theta11) - a11), -(d41 + (a21 * Mathf.Sin(theta31))) * _B2_2[m1, 2] + (a31 + a21 * Mathf.Cos(theta31)) * (_B2_2[m1, 1] * Mathf.Sin(theta11) + _B2_2[m1, 0] * Mathf.Cos(theta11) - a11));
//                            theta23_graus1 = theta231 * (180 / Mathf.PI);
//                            theta21 = theta231 - theta31;
//                            theta2_graus1 = Mathf.Round(theta21 * (180 / Mathf.PI) * 1000) / 1000;
//                            theta51 = theta21 + theta31;
//                            theta5_graus1 = theta51 * (180 / Mathf.PI);
//                            comparado_13 = 79.6f + theta5_graus1;
//                            dado_enviado = 144.58f - theta2_graus1;
//                            m1++;
//                            dataQueue1.Enqueue(dado_enviado);
//                            verdadeiro1 = false;

//                            // Reinicia a transição entre ângulos
//                            emTransicao1 = false;
//                        }

//                        else
//                        {
//                            //Atualiza o número da curva
//                            curva2++;

//                            //Reinicia o ciclo partindo da posição final
//                            m1 = 0;
//                            targetRotationZ = dado_enviado;
//                            emTransicao1 = false;
//                        }
//                    }
//                }

//                else
//                {
//                    if (verdadeiro1 == true)
//                    {
//                        theta2_graus_old = theta2_graus1;
//                        if (m1 < 21)
//                        {
//                            Debug.Log("O incremento m2 é: " + m1);
//                            theta11 = (Mathf.Atan2(_B3_2[m1, 1], _B3_2[m1, 0]));
//                            theta1_graus1 = Mathf.Round(theta11 * (180 / Mathf.PI) * 1000) / 1000;
//                            k1 = (Mathf.Pow(_B3_2[m1, 0], 2) + Mathf.Pow(_B3_2[m1, 1], 2) + Mathf.Pow(_B3_2[m1, 2], 2) + Mathf.Pow(a11, 2) - 2 * a11 * (_B3_2[m1, 0] * Mathf.Cos(theta11) + _B3_2[m1, 1] * Mathf.Sin(theta11)) - Mathf.Pow(a21, 2) - Mathf.Pow(a31, 2) - Mathf.Pow(d41, 2)) / (2 * a21);
//                            theta31 = (Mathf.Atan2(d41, a31) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a31, 2) + Mathf.Pow(d41, 2) - Mathf.Pow(k1, 2)), k1));
//                            theta3_graus1 = Mathf.Round(theta31 * (180 / Mathf.PI) * 1000) / 1000;
//                            comparado_11 = 306.09f - theta3_graus1;
//                            theta231 = Mathf.Atan2(_B3_2[m1, 2] * (a31 + a21 * Mathf.Cos(theta31)) + (d41 + a21 * Mathf.Sin(theta31)) * (_B3_2[m1, 1] * Mathf.Sin(theta11) + _B3_2[m1, 0] * Mathf.Cos(theta11) - a11), -(d41 + (a21 * Mathf.Sin(theta31))) * _B3_2[m1, 2] + (a31 + a21 * Mathf.Cos(theta31)) * (_B3_2[m1, 1] * Mathf.Sin(theta11) + _B3_2[m1, 0] * Mathf.Cos(theta11) - a11));
//                            theta23_graus1 = theta231 * (180 / Mathf.PI);
//                            theta21 = theta231 - theta31;
//                            theta2_graus1 = Mathf.Round(theta21 * (180 / Mathf.PI) * 1000) / 1000;
//                            theta51 = theta21 + theta31;
//                            theta5_graus1 = theta51 * (180 / Mathf.PI);
//                            comparado_13 = 79.6f + theta5_graus1;
//                            dado_enviado = 144.58f - theta2_graus1;
//                            m1++;
//                            dataQueue1.Enqueue(dado_enviado);
//                            verdadeiro1 = false;

//                            // Reinicia a transição entre ângulos
//                            emTransicao1 = false;
//                        }

//                        else
//                        {
//                            //Atualiza o número da curva
//                            curva2++;

//                            //Reinicia o ciclo partindo da posição final
//                            m1 = 0;
//                            targetRotationZ = dado_enviado;
//                            emTransicao1 = false;
//                        }
//                    }
//                }
//            }
//        }
//    }

//    // Função para rotacionar o objeto gradualmente em torno do eixo Y
//    void RotateObjectSmooth1()
//    {
//        // Calcula a rotação atual do objeto em torno do eixo Y
//        currentRotationZ = Mathf.LerpAngle(currentRotationZ, targetRotationZ, Time.deltaTime * velocidadeRotacao1 * direcao1);

//        // Aplica a rotação gradual
//        elementos1[1].localRotation = Quaternion.Euler(0f, 0f, -currentRotationZ);
//    }
//}
