using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class copia_Teste4 : MonoBehaviour
{
    [SerializeField] private Transform[] elementos5;

    // Variáveis relacionadas aos dados recebidos
    private float _PositionX5, _PositionY5, _PositionZ5;
    private float pointx_0_5, pointx_1_5, pointx_2_5, pointx_3_5, pointx_4_5, pointx_5_5, pointx_6_5, pointx_7_5, pointx_8_5, pointx_9_5, pointx_10_5, pointx_11_5, pointx_12_5, pointx_13_5, pointx_14_5;
    private float pointy_0_5, pointy_1_5, pointy_2_5, pointy_3_5, pointy_4_5, pointy_5_5, pointy_6_5, pointy_7_5, pointy_8_5, pointy_9_5, pointy_10_5, pointy_11_5, pointy_12_5, pointy_13_5, pointy_14_5;
    private float pointz_0_5, pointz_1_5, pointz_2_5, pointz_3_5, pointz_4_5, pointz_5_5, pointz_6_5, pointz_7_5, pointz_8_5, pointz_9_5, pointz_10_5, pointz_11_5, pointz_12_5, pointz_13_5, pointz_14_5;
    private float theta15, theta1_graus5, theta25, theta2_graus5, theta235, theta23_graus5, theta35, theta3_graus5, theta5_graus5, a15 = 43.9f, a25 = 118f, a35 = 18.2f, d45 = 171.9f, k5;
    private float[,] _B1_5 = new float[21, 3];
    private float[,] _B2_5 = new float[21, 3];
    private float[,] _B3_5 = new float[21, 3];

    // Variáveis de controle
    private float theta5_graus_old = 0;
    private int m5 = 0;
    private string[] separado5;
    private float[] dado_enviado2 = new float[60];
    private List<float> comparado_15 = new List<float>();
    private List<float> comparado_35 = new List<float>();
    private List<float> comparado_25 = new List<float>();
    private int curva5 = 1;
    private int iniciar;
    private List<float> angles = new List<float>();
    private int currentAnglesIndex = 0;
    private int iterador = 0;
    float targetAngle5;
    private bool encerrado = false;

    // Variáveis para a comunicação UDP
    public UdpClient udpReceiver5;
    public Thread receiveThread5;
    private ConcurrentQueue<float> dataQueue5 = new ConcurrentQueue<float>();

    // Velocidade de rotação gradual
    private float velocidadeRotacao5; // Graus por segundo

    // Rotação atual e destino
    private float currentRotationZ5;
    private float targetRotationZ5;

    // Ângulos de destino específicos
    private float anguloInicial5 = -180f;
    private float anguloFinal5 = 180f;

    // Direção da rotação (-1 para anti-horário, 1 para horário)
    private int direcao5 = 1; // Por padrão, rotação horária

    // Flag para controlar transição entre rotações
    private bool emTransicao5 = false;

    // Start is called before the first frame update
    void Start()
    {
        udpReceiver5 = new UdpClient(61565);
        receiveThread5 = new Thread(new ThreadStart(_ReceiveData5));
        receiveThread5.Start();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Lista = " + angles.Count);
        //Debug.Log("O valor do indexador de angulo é: " + currentAnglesIndex);
        if (!emTransicao5)
        {
            //Debug.Log("Indexador atual " + currentAnglesIndex + " e tamanho da lista " + angles.Count + "o angulo é: " + angles[currentAnglesIndex]);
            if (currentAnglesIndex < angles.Count)
            {
                Debug.Log("Entrou na rotação " + (currentAnglesIndex) + " do quarto motor. E o valor enviado foi: " + angles[currentAnglesIndex]);

                targetAngle5 = angles[currentAnglesIndex];
                //Debug.Log("Valor destino: " + targetAngle5);

                // Limita o ângulo dentro do intervalo permitido
                targetAngle5 = Mathf.Clamp(targetAngle5, anguloInicial5, anguloFinal5);

                // Determina a direção da rotação
                if (targetAngle5 > currentRotationZ5)
                {
                    direcao5 = 1; // Rotação horária
                    velocidadeRotacao5 = 10f;
                }
                else
                {
                    direcao5 = -1; // Rotação anti-horária
                    velocidadeRotacao5 = -10f;
                }

                // Atualiza o ângulo de rotação alvo
                targetRotationZ5 = targetAngle5;

                // Inicia a transição
                emTransicao5 = true;
            }
        }

        // Rotaciona gradualmente o objeto em torno do eixo Y
        RotateObjectSmooth5();

        verificar5();
    }

    // Método para verificar se a rotação atingiu o ângulo desejado
    void verificar5()
    {
        //Debug.Log("Valor Z: " + Mathf.Round(elementos5[3].localEulerAngles.z * 1000f) / 1000f + " Valor do Theta5: " + Mathf.Round(theta5_graus5 * 1000f) / 1000f);
        float angulo_euler15 = Mathf.Abs(Mathf.Abs(elementos5[0].localEulerAngles.y) - 360);
        float angulo_euler25 = Mathf.Abs(Mathf.Abs(elementos5[1].localEulerAngles.z) - 360);
        float angulo_euler35 = Mathf.Abs(Mathf.Abs(elementos5[2].localEulerAngles.z) - 360);
        float angulo_euler55 = Mathf.Abs(Mathf.Abs(elementos5[3].localEulerAngles.z) - 360);

        if (comparado_15[currentAnglesIndex] <= 0)
        {
            comparado_15[currentAnglesIndex] += 360;
        }
        if (comparado_25[currentAnglesIndex] < 0)
        {
            comparado_25[currentAnglesIndex] += 360;
        }
        if (comparado_35[currentAnglesIndex] < 0 || comparado_35[currentAnglesIndex] > 360)
        {
            if (comparado_35[currentAnglesIndex] < 0)
            {
                comparado_35[currentAnglesIndex] += 360;
            }
            else
            {
                comparado_35[currentAnglesIndex] -= 360;
            }
        }
        if (angles[currentAnglesIndex] < 0)
        {
            angles[currentAnglesIndex] += 360;
        }

        float dif15 = Mathf.Abs(angulo_euler15 - comparado_15[currentAnglesIndex]);
        float dif25 = Mathf.Abs(angulo_euler25 - comparado_25[currentAnglesIndex]);
        float dif35 = Mathf.Abs(angulo_euler35 - comparado_35[currentAnglesIndex]);
        float dif55 = Mathf.Abs(angulo_euler55 - angles[currentAnglesIndex]);

       //Debug.Log("Theta 1 lido " + angulo_euler15 + ". Angulo 1 enviado " + theta1_graus5 + ". A diferença é: " + dif15);
       //Debug.Log("Theta 2 lido " + angulo_euler25 + ". Angulo 2 enviado " + comparado_25 + ". A diferença é: " + dif25);
       //Debug.Log("Theta 3 lido " + angulo_euler35 + ". Angulo 3 enviado " + comparado_35 + ". A diferença é: " + dif35);
       Debug.Log("Theta 5 lido no motor 5 " + angulo_euler55 + ". Angulo 5 enviado " + angles[currentAnglesIndex] + ". A diferença é: " + dif55);

        if (/*dif15 < 0.03f && dif25 < 0.03f &&*/ dif35 < 0.03f && dif55 < 0.03f)
        {
            //Debug.Log("Theta 5 lido " + angulo_euler55 + ". Angulo 5 enviado " + angles[currentAnglesIndex] + ". A diferença é: " + dif55);
            emTransicao5 = false;
            Debug.Log("Quantidade de angulos: " + angles.Count + ". Indexador atual: " + (currentAnglesIndex + 1));
            if (angles.Count == (currentAnglesIndex+1))
            {
                elementos5[3].localRotation = Quaternion.Euler(0f, 0f, -90f);
            }
            if (currentAnglesIndex < 59)
            {
                currentAnglesIndex++;
            }
        }
    }

    // Método para encerrar a thread de recebimento ao destruir o objeto
    void OnDestroy()
    {
        receiveThread5.Abort();
    }

    // Função principal de recebimento e processamento de dados UDP
    void _ReceiveData5()
    {
        while (true)
        {
            IPEndPoint remoteEndPoint5 = new IPEndPoint(IPAddress.Any, 0);
            byte[] receiveBytes5 = udpReceiver5.Receive(ref remoteEndPoint5);
            string receivedData5 = Encoding.ASCII.GetString(receiveBytes5);

            //Debug.Log("Received data in teste4: " + receivedData5);

            string[] separado5 = receivedData5.Split(';');

            // Parse dos pontos recebidos
            pointx_0_5 = float.Parse(separado5[0]);
            pointy_0_5 = float.Parse(separado5[1]);
            pointz_0_5 = float.Parse(separado5[2]);
            pointx_1_5 = float.Parse(separado5[3]);
            pointy_1_5 = float.Parse(separado5[4]);
            pointz_1_5 = float.Parse(separado5[5]);
            pointx_2_5 = float.Parse(separado5[6]);
            pointy_2_5 = float.Parse(separado5[7]);
            pointz_2_5 = float.Parse(separado5[8]);
            pointx_3_5 = float.Parse(separado5[9]);
            pointy_3_5 = float.Parse(separado5[10]);
            pointz_3_5 = float.Parse(separado5[11]);
            pointx_4_5 = float.Parse(separado5[12]);
            pointy_4_5 = float.Parse(separado5[13]);
            pointz_4_5 = float.Parse(separado5[14]);
            pointx_5_5 = float.Parse(separado5[15]);
            pointy_5_5 = float.Parse(separado5[16]);
            pointz_5_5 = float.Parse(separado5[17]);
            pointx_6_5 = float.Parse(separado5[18]);
            pointy_6_5 = float.Parse(separado5[19]);
            pointz_6_5 = float.Parse(separado5[20]);
            pointx_7_5 = float.Parse(separado5[21]);
            pointy_7_5 = float.Parse(separado5[22]);
            pointz_7_5 = float.Parse(separado5[23]);
            pointx_8_5 = float.Parse(separado5[24]);
            pointy_8_5 = float.Parse(separado5[25]);
            pointz_8_5 = float.Parse(separado5[26]);
            pointx_9_5 = float.Parse(separado5[27]);
            pointy_9_5 = float.Parse(separado5[28]);
            pointz_9_5 = float.Parse(separado5[29]);
            pointx_10_5 = float.Parse(separado5[30]);
            pointy_10_5 = float.Parse(separado5[31]);
            pointz_10_5 = float.Parse(separado5[32]);
            pointx_11_5 = float.Parse(separado5[33]);
            pointy_11_5 = float.Parse(separado5[34]);
            pointz_11_5 = float.Parse(separado5[35]);
            pointx_12_5 = float.Parse(separado5[36]);
            pointy_12_5 = float.Parse(separado5[37]);
            pointz_12_5 = float.Parse(separado5[38]);
            pointx_13_5 = float.Parse(separado5[39]);
            pointy_13_5 = float.Parse(separado5[40]);
            pointz_13_5 = float.Parse(separado5[41]);
            pointx_14_5 = float.Parse(separado5[42]);
            pointy_14_5 = float.Parse(separado5[43]);
            pointz_14_5 = float.Parse(separado5[44]);
            iniciar = int.Parse(separado5[45]);

            if (iniciar > 0)
            {
                curva5 = 1;
                m5 = 0;
                currentAnglesIndex = 0;
                angles.Clear();
                comparado_15.Clear();
                comparado_25.Clear();
                comparado_35.Clear();
                Angulos();
            }
        }
    }

    // Função que executa o cálculo dos pontos de Bézier e dos ângulos de movimentação
    void Angulos()
    {
        // Primeira Curva
        int i = 0;

        for (float t5 = 0; t5 <= 1; t5 += 0.05f)
        {
            int j = 0;

            _PositionX5 = Mathf.Pow(1 - t5, 4) * pointx_0_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointx_1_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointx_2_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointx_3_5 + Mathf.Pow(t5, 4) * pointx_4_5;
            _PositionY5 = Mathf.Pow(1 - t5, 4) * pointy_0_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointy_1_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointy_2_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointy_3_5 + Mathf.Pow(t5, 4) * pointy_4_5;
            _PositionZ5 = Mathf.Pow(1 - t5, 4) * pointz_0_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointz_1_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointz_2_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointz_3_5 + Mathf.Pow(t5, 4) * pointz_4_5;

            _B1_5[i, j] = _PositionX5;
            j++;
            _B1_5[i, j] = _PositionY5;
            j++;
            _B1_5[i, j] = _PositionZ5;
            i++;
        }

        //Segunda Curva
        i = 0;

        for (float t5 = 0; t5 <= 1; t5 += 0.05f)
        {
            int j = 0;

            _PositionX5 = Mathf.Pow(1 - t5, 4) * pointx_5_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointx_6_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointx_7_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointx_8_5 + Mathf.Pow(t5, 4) * pointx_9_5;
            _PositionY5 = Mathf.Pow(1 - t5, 4) * pointy_5_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointy_6_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointy_7_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointy_8_5 + Mathf.Pow(t5, 4) * pointy_9_5;
            _PositionZ5 = Mathf.Pow(1 - t5, 4) * pointz_5_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointz_6_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointz_7_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointz_8_5 + Mathf.Pow(t5, 4) * pointz_9_5;

            _B2_5[i, j] = _PositionX5;
            j++;
            _B2_5[i, j] = _PositionY5;
            j++;
            _B2_5[i, j] = _PositionZ5;
            i++;
        }

        //Terceira Curva
        i = 0;

        for (float t5 = 0; t5 <= 1; t5 += 0.05f)
        {
            int j = 0;

            _PositionX5 = Mathf.Pow(1 - t5, 4) * pointx_10_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointx_11_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointx_12_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointx_13_5 + Mathf.Pow(t5, 4) * pointx_14_5;
            _PositionY5 = Mathf.Pow(1 - t5, 4) * pointy_10_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointy_11_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointy_12_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointy_13_5 + Mathf.Pow(t5, 4) * pointy_14_5;
            _PositionZ5 = Mathf.Pow(1 - t5, 4) * pointz_10_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointz_11_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointz_12_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointz_13_5 + Mathf.Pow(t5, 4) * pointz_14_5;

            _B3_5[i, j] = _PositionX5;
            j++;
            _B3_5[i, j] = _PositionY5;
            j++;
            _B3_5[i, j] = _PositionZ5;
            i++;
        }

        // Limpando a lista de ângulos
        angles.Clear();

        // Cálculo do ângulo de rotação
        for (int a = 0; a < 65; a++)
        {
            if (curva5 == 1)
            {
                if (m5 < 20)
                {
                    //Debug.Log("O incremento m5 é: " + m5 + ". Na curva 1");
                    theta15 = (Mathf.Atan2(_B1_5[m5, 1], _B1_5[m5, 0]));
                    theta1_graus5 = theta15 * (180 / Mathf.PI);
                    comparado_15.Add(theta1_graus5);
                    k5 = (Mathf.Pow(_B1_5[m5, 0], 2) + Mathf.Pow(_B1_5[m5, 1], 2) + Mathf.Pow(_B1_5[m5, 2], 2) + Mathf.Pow(a15, 2) - 2 * a15 * (_B1_5[m5, 0] * Mathf.Cos(theta15) + _B1_5[m5, 1] * Mathf.Sin(theta15)) - Mathf.Pow(a25, 2) - Mathf.Pow(a35, 2) - Mathf.Pow(d45, 2)) / (2 * a25);
                    theta35 = (Mathf.Atan2(d45, a35) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a35, 2) + Mathf.Pow(d45, 2) - Mathf.Pow(k5, 2)), k5));
                    theta3_graus5 = theta35 * (180 / Mathf.PI);
                    comparado_35.Add(306.09f - theta3_graus5);
                    theta235 = Mathf.Atan2(_B1_5[m5, 2] * (a35 + a25 * Mathf.Cos(theta35)) + (d45 + a25 * Mathf.Sin(theta35)) * (_B1_5[m5, 1] * Mathf.Sin(theta15) + _B1_5[m5, 0] * Mathf.Cos(theta15) - a15), -(d45 + (a25 * Mathf.Sin(theta35))) * _B1_5[m5, 2] + (a35 + a25 * Mathf.Cos(theta35)) * (_B1_5[m5, 1] * Mathf.Sin(theta15) + _B1_5[m5, 0] * Mathf.Cos(theta15) - a15));
                    theta23_graus5 = theta235 * (180 / Mathf.PI);
                    theta25 = theta235 - theta35;
                    theta2_graus5 = theta25 * (180 / Mathf.PI);
                    comparado_25.Add(144.58f - theta2_graus5);
                    theta5_graus5 = theta2_graus5 + theta3_graus5;
                    dado_enviado2[a] = 79.6f + theta5_graus5;
                    m5++;
                    angles.Add(dado_enviado2[a]);
                }

                else
                {
                    m5 = 0;
                    curva5++;
                }
            }

            else
            {
                if (curva5 == 2)
                {
                    if (m5 < 20)
                    {
                        theta15 = (Mathf.Atan2(_B2_5[m5, 1], _B2_5[m5, 0]));
                        theta1_graus5 = theta15 * (180 / Mathf.PI);
                        comparado_15.Add(theta1_graus5);
                        k5 = (Mathf.Pow(_B2_5[m5, 0], 2) + Mathf.Pow(_B2_5[m5, 1], 2) + Mathf.Pow(_B2_5[m5, 2], 2) + Mathf.Pow(a15, 2) - 2 * a15 * (_B2_5[m5, 0] * Mathf.Cos(theta15) + _B2_5[m5, 1] * Mathf.Sin(theta15)) - Mathf.Pow(a25, 2) - Mathf.Pow(a35, 2) - Mathf.Pow(d45, 2)) / (2 * a25);
                        theta35 = (Mathf.Atan2(d45, a35) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a35, 2) + Mathf.Pow(d45, 2) - Mathf.Pow(k5, 2)), k5));
                        theta3_graus5 = theta35 * (180 / Mathf.PI);
                        comparado_35.Add(306.09f - theta3_graus5);
                        theta235 = Mathf.Atan2(_B2_5[m5, 2] * (a35 + a25 * Mathf.Cos(theta35)) + (d45 + a25 * Mathf.Sin(theta35)) * (_B2_5[m5, 1] * Mathf.Sin(theta15) + _B2_5[m5, 0] * Mathf.Cos(theta15) - a15), -(d45 + (a25 * Mathf.Sin(theta35))) * _B2_5[m5, 2] + (a35 + a25 * Mathf.Cos(theta35)) * (_B2_5[m5, 1] * Mathf.Sin(theta15) + _B2_5[m5, 0] * Mathf.Cos(theta15) - a15));
                        theta23_graus5 = theta235 * (180 / Mathf.PI);
                        theta25 = theta235 - theta35;
                        theta2_graus5 = theta25 * (180 / Mathf.PI);
                        comparado_25.Add(144.58f - theta2_graus5);
                        theta5_graus5 = theta2_graus5 + theta3_graus5;
                        dado_enviado2[a-1] = 79.6f + theta5_graus5;
                        m5++;
                        angles.Add(dado_enviado2[a-1]);
                    }

                    else
                    {
                        m5 = 0;

                        //Atualiza o número da curva
                        curva5++;
                    }
                }

                else
                {
                    if (curva5 == 3)
                    {
                        if (m5 < 20)
                        {
                            //Debug.Log("O incremento m5 é: " + m5 + ". Na curva 3");
                            theta15 = (Mathf.Atan2(_B3_5[m5, 1], _B3_5[m5, 0]));
                            theta1_graus5 = theta15 * (180 / Mathf.PI);
                            comparado_15.Add(theta1_graus5);
                            k5 = (Mathf.Pow(_B3_5[m5, 0], 2) + Mathf.Pow(_B3_5[m5, 1], 2) + Mathf.Pow(_B3_5[m5, 2], 2) + Mathf.Pow(a15, 2) - 2 * a15 * (_B3_5[m5, 0] * Mathf.Cos(theta15) + _B3_5[m5, 1] * Mathf.Sin(theta15)) - Mathf.Pow(a25, 2) - Mathf.Pow(a35, 2) - Mathf.Pow(d45, 2)) / (2 * a25);
                            theta35 = (Mathf.Atan2(d45, a35) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a35, 2) + Mathf.Pow(d45, 2) - Mathf.Pow(k5, 2)), k5));
                            theta3_graus5 = theta35 * (180 / Mathf.PI);
                            comparado_35.Add(306.09f - theta3_graus5);
                            theta235 = Mathf.Atan2(_B3_5[m5, 2] * (a35 + a25 * Mathf.Cos(theta35)) + (d45 + a25 * Mathf.Sin(theta35)) * (_B3_5[m5, 1] * Mathf.Sin(theta15) + _B3_5[m5, 0] * Mathf.Cos(theta15) - a15), -(d45 + (a25 * Mathf.Sin(theta35))) * _B3_5[m5, 2] + (a35 + a25 * Mathf.Cos(theta35)) * (_B3_5[m5, 1] * Mathf.Sin(theta15) + _B3_5[m5, 0] * Mathf.Cos(theta15) - a15));
                            theta23_graus5 = theta235 * (180 / Mathf.PI);
                            theta25 = theta235 - theta35;
                            theta2_graus5 = theta25 * (180 / Mathf.PI);
                            comparado_25.Add(144.58f - theta2_graus5);
                            theta5_graus5 = theta2_graus5 + theta3_graus5;
                            dado_enviado2[a-2] = 79.6f + theta5_graus5;
                            m5++;
                            angles.Add(dado_enviado2[a-2]);
                        }
                        else
                        {
                            m5 = 0;

                            //Atualiza o número da curva
                            curva5 = 0;
                        }
                    }
                }
            }
        }
    }


    // Função para rotacionar o objeto gradualmente em torno do eixo Y
    void RotateObjectSmooth5()
    {
        // Calcula a rotação atual do objeto em torno do eixo Y
        currentRotationZ5 = Mathf.LerpAngle(currentRotationZ5, targetRotationZ5, Time.deltaTime * velocidadeRotacao5 * direcao5);
        Debug.Log("Rotação atual: " + currentRotationZ5);

        // Aplica a rotação gradual
        elementos5[3].localRotation = Quaternion.Euler(0f, 0f, -currentRotationZ5);
    }
}



//[SerializeField] private Transform[] elementos5;

//// Variáveis relacionadas aos dados recebidos
//private float _PositionX5, _PositionY5, _PositionZ5;
//private float pointx_0_5, pointx_1_5, pointx_2_5, pointx_3_5, pointx_4_5, pointx_5_5, pointx_6_5, pointx_7_5, pointx_8_5, pointx_9_5, pointx_10_5, pointx_11_5, pointx_12_5, pointx_13_5, pointx_14_5;
//private float pointy_0_5, pointy_1_5, pointy_2_5, pointy_3_5, pointy_4_5, pointy_5_5, pointy_6_5, pointy_7_5, pointy_8_5, pointy_9_5, pointy_10_5, pointy_11_5, pointy_12_5, pointy_13_5, pointy_14_5;
//private float pointz_0_5, pointz_1_5, pointz_2_5, pointz_3_5, pointz_4_5, pointz_5_5, pointz_6_5, pointz_7_5, pointz_8_5, pointz_9_5, pointz_10_5, pointz_11_5, pointz_12_5, pointz_13_5, pointz_14_5;
//private float theta15, theta1_graus5, theta25, theta2_graus5, theta235, theta23_graus5, theta35, theta3_graus5, theta5_graus5, a15 = 43.9f, a25 = 118f, a35 = 18.2f, d45 = 171.9f, k5;
//private float[,] _B1_5 = new float[21, 3];
//private float[,] _B2_5 = new float[21, 3];
//private float[,] _B3_5 = new float[21, 3];

//// Variáveis de controle
//private float theta5_graus_old = 0;
//private bool verdadeiro5 = true;
//private int m5 = 0;
//private int l5 = 0;
//private string[] separado5;
//private float dado_enviado2;
//private float comparado_35, comparado_25;
//private int curva5 = 1;
//float angulo_euler55 = 0f;
//int iniciar;

//// Variáveis para a comunicação UDP
//public UdpClient udpReceiver5;
//public Thread receiveThread5;
//private ConcurrentQueue<float> dataQueue5 = new ConcurrentQueue<float>();

//// Velocidade de rotação gradual
//private float velocidadeRotacao5; // Graus por segundo

//// Rotação atual e destino
//private float currentRotationZ5;
//private float targetRotationZ5;

//// Ângulos de destino específicos
//private float anguloInicial5 = -180f;
//private float anguloFinal5 = 180f;

//// Direção da rotação (-1 para anti-horário, 1 para horário)
//private int direcao5 = 1; // Por padrão, rotação horária

//// Flag para controlar transição entre rotações
//private bool emTransicao5 = false;

//// Start is called before the first frame update
//void Start()
//{
//    udpReceiver5 = new UdpClient(61565);
//    receiveThread5 = new Thread(new ThreadStart(_ReceiveData5));
//    receiveThread5.Start();
//}

//// Update is called once per frame
//void Update()
//{
//    if (l5 != 0)
//    {
//        verificar5();
//    }

//    // Verifica se há dados na fila para processar
//    if (dataQueue5.Count > 0 && !emTransicao5)
//    {
//        if (dataQueue5.TryDequeue(out float dados5))
//        {
//            //Debug.Log(" o dado 5 é: " + dados5);
//            // Determina o ângulo de destino com base no dado recebido
//            float targetAngle5 = dados5;
//            // Limita o ângulo dentro do intervalo permitido
//            targetAngle5 = Mathf.Clamp(targetAngle5, anguloInicial5, anguloFinal5);

//            // Determina a direção da rotação
//            if (dados5 > theta5_graus_old)
//            {
//                direcao5 = 1; // Rotação horária
//                velocidadeRotacao5 = 18f;
//            }
//            else
//            {
//                direcao5 = -1; // Rotação anti-horária
//                velocidadeRotacao5 = -18f;
//            }

//            // Atualiza o ângulo de rotação alvo
//            targetRotationZ5 = targetAngle5;

//            // Inicia a transição
//            emTransicao5 = true;

//            // Reinicia a flag de verificação
//            verdadeiro5 = false;
//        }
//    }

//    // Rotaciona gradualmente o objeto em torno do eixo Y
//    RotateObjectSmooth5();
//}

//// Método para verificar se a rotação atingiu o ângulo desejado
//void verificar5()
//{
//    //Debug.Log("Valor Z: " + Mathf.Round(elementos5[3].localEulerAngles.z * 1000f) / 1000f + " Valor do Theta5: " + Mathf.Round(theta5_graus5 * 1000f) / 1000f);
//    float angulo_euler15 = Mathf.Abs((Mathf.Round(elementos5[0].localEulerAngles.y * 1000) / 1000) - 360);
//    float angulo_euler25 = Mathf.Abs((Mathf.Round(elementos5[1].localEulerAngles.z * 1000) / 1000) - 360);
//    float angulo_euler35 = Mathf.Abs((Mathf.Round(elementos5[2].localEulerAngles.z * 1000) / 1000) - 360);
//    angulo_euler55 = Mathf.Abs((Mathf.Round(elementos5[3].localEulerAngles.z * 1000) / 1000) - 360);

//    if (theta1_graus5 < 0)
//    {
//        theta1_graus5 += 360;
//    }
//    if (theta1_graus5 == 0)
//    {
//        theta1_graus5 += 360;
//    }
//    if (theta2_graus5 < 0)
//    {
//        theta2_graus5 += 360;
//    }
//    if (theta3_graus5 < 0)
//    {
//        theta3_graus5 += 360;
//    }
//    if (comparado_35 > 360)
//    {
//        comparado_35 -= 360;
//    }

//    if (dado_enviado2 < 0)
//    {
//        dado_enviado2 += 360;
//    }

//    float dif15 = Mathf.Round(angulo_euler15 - theta1_graus5);
//    float dif25 = Mathf.Round(Mathf.Abs(angulo_euler25) - comparado_25);
//    float dif35 = Mathf.Round(angulo_euler35 - comparado_35);
//    float dif55 = angulo_euler55 - dado_enviado2;

//    //Debug.Log("Theta 1 lido " + angulo_euler15 + ". Angulo 1 enviado " + theta1_graus5 + ". A diferença é: " + dif15);
//    //Debug.Log("Theta 2 lido " + angulo_euler25 + ". Angulo 2 enviado " + comparado_25 + ". A diferença é: " + dif25);
//    //Debug.Log("Theta 3 lido " + angulo_euler35 + ". Angulo 3 enviado " + comparado_35 + ". A diferença é: " + dif35);
//    //Debug.Log("Theta 5 lido " + angulo_euler55 + ". Angulo 5 enviado " + dado_enviado2 + ". A diferença é: " + dif55);

//    if (/*Mathf.Abs(dif15) < 0.03f && Mathf.Abs(dif25) < 0.3f && Mathf.Abs(dif35) < 0.3f &&*/ Mathf.Abs(dif55) < 0.03f)
//    {
//        Debug.Log("O valor lido é: " + angulo_euler55 + ". O dado enviado é: " + dado_enviado2 + ". O valor de theta 5 é: " + theta5_graus5 + ". A diferença é: " + dif55);
//        verdadeiro5 = true;
//    }
//}

//// Método para encerrar a thread de recebimento ao destruir o objeto
//void OnDestroy()
//{
//    receiveThread5.Abort();
//}

//// Função principal de recebimento e processamento de dados UDP
//void _ReceiveData5()
//{
//    while (true)
//    {
//        IPEndPoint remoteEndPoint5 = new IPEndPoint(IPAddress.Any, 0);
//        byte[] receiveBytes5 = udpReceiver5.Receive(ref remoteEndPoint5);
//        string receivedData5 = Encoding.ASCII.GetString(receiveBytes5);

//        //Debug.Log("Received data in teste4: " + receivedData5);

//        string[] separado5 = receivedData5.Split(';');

//        // Parse dos pontos recebidos
//        pointx_0_5 = float.Parse(separado5[0]);
//        pointy_0_5 = float.Parse(separado5[1]);
//        pointz_0_5 = float.Parse(separado5[2]);
//        pointx_1_5 = float.Parse(separado5[3]);
//        pointy_1_5 = float.Parse(separado5[4]);
//        pointz_1_5 = float.Parse(separado5[5]);
//        pointx_2_5 = float.Parse(separado5[6]);
//        pointy_2_5 = float.Parse(separado5[7]);
//        pointz_2_5 = float.Parse(separado5[8]);
//        pointx_3_5 = float.Parse(separado5[9]);
//        pointy_3_5 = float.Parse(separado5[10]);
//        pointz_3_5 = float.Parse(separado5[11]);
//        pointx_4_5 = float.Parse(separado5[12]);
//        pointy_4_5 = float.Parse(separado5[13]);
//        pointz_4_5 = float.Parse(separado5[14]);
//        pointx_5_5 = float.Parse(separado5[15]);
//        pointy_5_5 = float.Parse(separado5[16]);
//        pointz_5_5 = float.Parse(separado5[17]);
//        pointx_6_5 = float.Parse(separado5[18]);
//        pointy_6_5 = float.Parse(separado5[19]);
//        pointz_6_5 = float.Parse(separado5[20]);
//        pointx_7_5 = float.Parse(separado5[21]);
//        pointy_7_5 = float.Parse(separado5[22]);
//        pointz_7_5 = float.Parse(separado5[23]);
//        pointx_8_5 = float.Parse(separado5[24]);
//        pointy_8_5 = float.Parse(separado5[25]);
//        pointz_8_5 = float.Parse(separado5[26]);
//        pointx_9_5 = float.Parse(separado5[27]);
//        pointy_9_5 = float.Parse(separado5[28]);
//        pointz_9_5 = float.Parse(separado5[29]);
//        pointx_10_5 = float.Parse(separado5[30]);
//        pointy_10_5 = float.Parse(separado5[31]);
//        pointz_10_5 = float.Parse(separado5[32]);
//        pointx_11_5 = float.Parse(separado5[33]);
//        pointy_11_5 = float.Parse(separado5[34]);
//        pointz_11_5 = float.Parse(separado5[35]);
//        pointx_12_5 = float.Parse(separado5[36]);
//        pointy_12_5 = float.Parse(separado5[37]);
//        pointz_12_5 = float.Parse(separado5[38]);
//        pointx_13_5 = float.Parse(separado5[39]);
//        pointy_13_5 = float.Parse(separado5[40]);
//        pointz_13_5 = float.Parse(separado5[41]);
//        pointx_14_5 = float.Parse(separado5[42]);
//        pointy_14_5 = float.Parse(separado5[43]);
//        pointz_14_5 = float.Parse(separado5[44]);

//        // Cálculo dos pontos de Bézier

//        // Primeira Curva
//        int i = 0;

//        for (float t5 = 0; t5 <= 1; t5 += 0.05f)
//        {
//            int j = 0;

//            _PositionX5 = Mathf.Pow(1 - t5, 4) * pointx_0_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointx_1_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointx_2_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointx_3_5 + Mathf.Pow(t5, 4) * pointx_4_5;
//            _PositionY5 = Mathf.Pow(1 - t5, 4) * pointy_0_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointy_1_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointy_2_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointy_3_5 + Mathf.Pow(t5, 4) * pointy_4_5;
//            _PositionZ5 = Mathf.Pow(1 - t5, 4) * pointz_0_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointz_1_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointz_2_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointz_3_5 + Mathf.Pow(t5, 4) * pointz_4_5;

//            _B1_5[i, j] = _PositionX5;
//            j++;
//            _B1_5[i, j] = _PositionY5;
//            j++;
//            _B1_5[i, j] = _PositionZ5;
//            i++;
//        }

//        //Segunda Curva
//        i = 0;

//        for (float t5 = 0; t5 <= 1; t5 += 0.05f)
//        {
//            int j = 0;

//            _PositionX5 = Mathf.Pow(1 - t5, 4) * pointx_5_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointx_6_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointx_7_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointx_8_5 + Mathf.Pow(t5, 4) * pointx_9_5;
//            _PositionY5 = Mathf.Pow(1 - t5, 4) * pointy_5_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointy_6_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointy_7_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointy_8_5 + Mathf.Pow(t5, 4) * pointy_9_5;
//            _PositionZ5 = Mathf.Pow(1 - t5, 4) * pointz_5_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointz_6_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointz_7_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointz_8_5 + Mathf.Pow(t5, 4) * pointz_9_5;

//            _B2_5[i, j] = _PositionX5;
//            j++;
//            _B2_5[i, j] = _PositionY5;
//            j++;
//            _B2_5[i, j] = _PositionZ5;
//            i++;
//        }

//        //Terceira Curva
//        i = 0;

//        for (float t5 = 0; t5 <= 1; t5 += 0.05f)
//        {
//            int j = 0;

//            _PositionX5 = Mathf.Pow(1 - t5, 4) * pointx_10_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointx_11_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointx_12_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointx_13_5 + Mathf.Pow(t5, 4) * pointx_14_5;
//            _PositionY5 = Mathf.Pow(1 - t5, 4) * pointy_10_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointy_11_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointy_12_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointy_13_5 + Mathf.Pow(t5, 4) * pointy_14_5;
//            _PositionZ5 = Mathf.Pow(1 - t5, 4) * pointz_10_5 + 4 * Mathf.Pow(1 - t5, 3) * t5 * pointz_11_5 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * pointz_12_5 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * pointz_13_5 + Mathf.Pow(t5, 4) * pointz_14_5;

//            _B3_5[i, j] = _PositionX5;
//            j++;
//            _B3_5[i, j] = _PositionY5;
//            j++;
//            _B3_5[i, j] = _PositionZ5;
//            i++;
//        }

//        // Cálculo do ângulo de rotação

//        if (curva5 == 1)
//        {
//            if (l5 == 0)
//            {
//                Debug.Log("O incremento m5 é: " + m5);
//                l5++;
//                theta15 = (Mathf.Atan2(_B1_5[m5, 1], _B1_5[m5, 0]));
//                theta1_graus5 = (float)Mathf.Round(theta15 * (180 / Mathf.PI) * 1000) / 1000;
//                k5 = (Mathf.Pow(_B1_5[m5, 0], 2) + Mathf.Pow(_B1_5[m5, 1], 2) + Mathf.Pow(_B1_5[m5, 2], 2) + Mathf.Pow(a15, 2) - 2 * a15 * (_B1_5[m5, 0] * Mathf.Cos(theta15) + _B1_5[m5, 1] * Mathf.Sin(theta15)) - Mathf.Pow(a25, 2) - Mathf.Pow(a35, 2) - Mathf.Pow(d45, 2)) / (2 * a25);
//                theta35 = (Mathf.Atan2(d45, a35) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a35, 2) + Mathf.Pow(d45, 2) - Mathf.Pow(k5, 2)), k5));
//                theta3_graus5 = theta35 * (180 / Mathf.PI);
//                comparado_35 = 306.09f - theta3_graus5;
//                theta235 = Mathf.Atan2(_B1_5[m5, 2] * (a35 + a25 * Mathf.Cos(theta35)) + (d45 + a25 * Mathf.Sin(theta35)) * (_B1_5[m5, 1] * Mathf.Sin(theta15) + _B1_5[m5, 0] * Mathf.Cos(theta15) - a15), -(d45 + (a25 * Mathf.Sin(theta35))) * _B1_5[m5, 2] + (a35 + a25 * Mathf.Cos(theta35)) * (_B1_5[m5, 1] * Mathf.Sin(theta15) + _B1_5[m5, 0] * Mathf.Cos(theta15) - a15));
//                theta23_graus5 = theta235 * (180 / Mathf.PI);
//                theta25 = theta235 - theta35;
//                theta2_graus5 = theta25 * (180 / Mathf.PI);
//                comparado_25 = 144.58f - theta2_graus5;
//                theta5_graus5 = theta2_graus5 + theta3_graus5;
//                dado_enviado2 = 79.6f + theta5_graus5;

//                dataQueue5.Enqueue(dado_enviado2);
//            }
//            else
//            {
//                Debug.Log("Verdadeiro é: " + verdadeiro5);
//                if (verdadeiro5 == true)
//                {
//                    m5++;
//                    theta5_graus_old = dado_enviado2;
//                    if (m5 < 20)
//                    {
//                        theta15 = (Mathf.Atan2(_B1_5[m5, 1], _B1_5[m5, 0]));
//                        theta1_graus5 = (float)Mathf.Round(theta15 * (180 / Mathf.PI) * 1000) / 1000;
//                        k5 = (Mathf.Pow(_B1_5[m5, 0], 2) + Mathf.Pow(_B1_5[m5, 1], 2) + Mathf.Pow(_B1_5[m5, 2], 2) + Mathf.Pow(a15, 2) - 2 * a15 * (_B1_5[m5, 0] * Mathf.Cos(theta15) + _B1_5[m5, 1] * Mathf.Sin(theta15)) - Mathf.Pow(a25, 2) - Mathf.Pow(a35, 2) - Mathf.Pow(d45, 2)) / (2 * a25);
//                        theta35 = (Mathf.Atan2(d45, a35) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a35, 2) + Mathf.Pow(d45, 2) - Mathf.Pow(k5, 2)), k5));
//                        theta3_graus5 = theta35 * (180 / Mathf.PI);
//                        comparado_35 = 306.09f - theta3_graus5;
//                        theta235 = Mathf.Atan2(_B1_5[m5, 2] * (a35 + a25 * Mathf.Cos(theta35)) + (d45 + a25 * Mathf.Sin(theta35)) * (_B1_5[m5, 1] * Mathf.Sin(theta15) + _B1_5[m5, 0] * Mathf.Cos(theta15) - a15), -(d45 + (a25 * Mathf.Sin(theta35))) * _B1_5[m5, 2] + (a35 + a25 * Mathf.Cos(theta35)) * (_B1_5[m5, 1] * Mathf.Sin(theta15) + _B1_5[m5, 0] * Mathf.Cos(theta15) - a15));
//                        theta23_graus5 = theta235 * (180 / Mathf.PI);
//                        theta25 = theta235 - theta35;
//                        theta2_graus5 = (float)Mathf.Round(theta25 * (180 / Mathf.PI) * 1000) / 1000;
//                        comparado_25 = 144.58f - theta2_graus5;
//                        theta5_graus5 = theta2_graus5 + theta3_graus5;
//                        dado_enviado2 = 79.6f + theta5_graus5;
//                        m5++;
//                        dataQueue5.Enqueue(dado_enviado2);
//                        verdadeiro5 = false;

//                        // Reinicia a transição entre ângulos
//                        emTransicao5 = false;
//                    }

//                    else
//                    {
//                        m5 = 0;
//                        //Atualiza o número da curva
//                        curva5++;
//                        targetRotationZ5 = dado_enviado2;
//                        emTransicao5 = false;
//                        verdadeiro5 = true;
//                    }
//                }
//            }
//        }

//        else
//        {
//            Debug.Log("Inicializando a segunda curva");
//            if (curva5 == 2)
//            {
//                if (verdadeiro5 == true)
//                {
//                    theta5_graus_old = dado_enviado2;
//                    if (m5 < 20)
//                    {
//                        Debug.Log("O incremento m5 é: " + m5);
//                        theta15 = (Mathf.Atan2(_B2_5[m5, 1], _B2_5[m5, 0]));
//                        theta1_graus5 = (float)Mathf.Round(theta15 * (180 / Mathf.PI) * 1000) / 1000;
//                        k5 = (Mathf.Pow(_B2_5[m5, 0], 2) + Mathf.Pow(_B2_5[m5, 1], 2) + Mathf.Pow(_B2_5[m5, 2], 2) + Mathf.Pow(a15, 2) - 2 * a15 * (_B2_5[m5, 0] * Mathf.Cos(theta15) + _B2_5[m5, 1] * Mathf.Sin(theta15)) - Mathf.Pow(a25, 2) - Mathf.Pow(a35, 2) - Mathf.Pow(d45, 2)) / (2 * a25);
//                        theta35 = (Mathf.Atan2(d45, a35) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a35, 2) + Mathf.Pow(d45, 2) - Mathf.Pow(k5, 2)), k5));
//                        theta3_graus5 = theta35 * (180 / Mathf.PI);
//                        comparado_35 = 306.09f - theta3_graus5;
//                        theta235 = Mathf.Atan2(_B2_5[m5, 2] * (a35 + a25 * Mathf.Cos(theta35)) + (d45 + a25 * Mathf.Sin(theta35)) * (_B2_5[m5, 1] * Mathf.Sin(theta15) + _B2_5[m5, 0] * Mathf.Cos(theta15) - a15), -(d45 + (a25 * Mathf.Sin(theta35))) * _B2_5[m5, 2] + (a35 + a25 * Mathf.Cos(theta35)) * (_B2_5[m5, 1] * Mathf.Sin(theta15) + _B2_5[m5, 0] * Mathf.Cos(theta15) - a15));
//                        theta23_graus5 = theta235 * (180 / Mathf.PI);
//                        theta25 = theta235 - theta35;
//                        theta2_graus5 = (float)Mathf.Round(theta25 * (180 / Mathf.PI) * 1000) / 1000;
//                        comparado_25 = 144.58f - theta2_graus5;
//                        theta5_graus5 = theta2_graus5 + theta3_graus5;
//                        dado_enviado2 = 79.6f + theta5_graus5;
//                        m5++;
//                        dataQueue5.Enqueue(dado_enviado2);
//                        verdadeiro5 = false;

//                        // Reinicia a transição entre ângulos
//                        emTransicao5 = false;
//                    }
//                    else
//                    {
//                        m5 = 0;
//                        //Atualiza o número da curva
//                        curva5++;
//                        targetRotationZ5 = dado_enviado2;
//                        emTransicao5 = false;
//                    }
//                }
//            }

//            else
//            {
//                if (verdadeiro5 == true)
//                {
//                    Debug.Log("Inicializando a terceira curva");
//                    theta5_graus_old = dado_enviado2;
//                    if (m5 < 20)
//                    {
//                        Debug.Log("O incremento m5 é: " + m5);
//                        theta15 = (Mathf.Atan2(_B3_5[m5, 1], _B3_5[m5, 0]));
//                        theta1_graus5 = (float)Mathf.Round(theta15 * (180 / Mathf.PI) * 1000) / 1000;
//                        k5 = (Mathf.Pow(_B3_5[m5, 0], 2) + Mathf.Pow(_B3_5[m5, 1], 2) + Mathf.Pow(_B3_5[m5, 2], 2) + Mathf.Pow(a15, 2) - 2 * a15 * (_B3_5[m5, 0] * Mathf.Cos(theta15) + _B3_5[m5, 1] * Mathf.Sin(theta15)) - Mathf.Pow(a25, 2) - Mathf.Pow(a35, 2) - Mathf.Pow(d45, 2)) / (2 * a25);
//                        theta35 = (Mathf.Atan2(d45, a35) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a35, 2) + Mathf.Pow(d45, 2) - Mathf.Pow(k5, 2)), k5));
//                        theta3_graus5 = theta35 * (180 / Mathf.PI);
//                        comparado_35 = 306.09f - theta3_graus5;
//                        theta235 = Mathf.Atan2(_B3_5[m5, 2] * (a35 + a25 * Mathf.Cos(theta35)) + (d45 + a25 * Mathf.Sin(theta35)) * (_B3_5[m5, 1] * Mathf.Sin(theta15) + _B3_5[m5, 0] * Mathf.Cos(theta15) - a15), -(d45 + (a25 * Mathf.Sin(theta35))) * _B3_5[m5, 2] + (a35 + a25 * Mathf.Cos(theta35)) * (_B3_5[m5, 1] * Mathf.Sin(theta15) + _B3_5[m5, 0] * Mathf.Cos(theta15) - a15));
//                        theta23_graus5 = theta235 * (180 / Mathf.PI);
//                        theta25 = theta235 - theta35;
//                        theta2_graus5 = (float)Mathf.Round(theta25 * (180 / Mathf.PI) * 1000) / 1000;
//                        comparado_25 = 144.58f - theta2_graus5;
//                        theta5_graus5 = theta2_graus5 + theta3_graus5;
//                        dado_enviado2 = 79.6f + theta5_graus5;
//                        m5++;
//                        dataQueue5.Enqueue(dado_enviado2);
//                        verdadeiro5 = false;

//                        // Reinicia a transição entre ângulos
//                        emTransicao5 = false;
//                    }
//                    else
//                    {
//                        m5 = 0;
//                        //Atualiza o número da curva
//                        curva5 = 1;
//                        targetRotationZ5 = dado_enviado2;
//                        emTransicao5 = false;
//                    }
//                }
//            }
//        }
//    }
//}


//// Função para rotacionar o objeto gradualmente em torno do eixo Y
//void RotateObjectSmooth5()
//{
//    // Calcula a rotação atual do objeto em torno do eixo Y
//    currentRotationZ5 = Mathf.LerpAngle(currentRotationZ5, targetRotationZ5, Time.deltaTime * velocidadeRotacao5 * direcao5);

//    // Aplica a rotação gradual
//    elementos5[3].localRotation = Quaternion.Euler(0f, 0f, -currentRotationZ5);
//}
//}
