using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class copia_Teste3 : MonoBehaviour
{
    [SerializeField] private Transform[] elementos3;

    // Variáveis relacionadas aos dados recebidos
    private float _PositionX3, _PositionY3, _PositionZ3;
    private float pointx_0_3, pointx_1_3, pointx_2_3, pointx_3_3, pointx_4_3, pointx_5_3, pointx_6_3, pointx_7_3, pointx_8_3, pointx_9_3, pointx_10_3, pointx_11_3, pointx_12_3, pointx_13_3, pointx_14_3;
    private float pointy_0_3, pointy_1_3, pointy_2_3, pointy_3_3, pointy_4_3, pointy_5_3, pointy_6_3, pointy_7_3, pointy_8_3, pointy_9_3, pointy_10_3, pointy_11_3, pointy_12_3, pointy_13_3, pointy_14_3;
    private float pointz_0_3, pointz_1_3, pointz_2_3, pointz_3_3, pointz_4_3, pointz_5_3, pointz_6_3, pointz_7_3, pointz_8_3, pointz_9_3, pointz_10_3, pointz_11_3, pointz_12_3, pointz_13_3, pointz_14_3;
    private float theta13, theta1_graus3, theta23, theta2_graus3, theta233, theta23_graus3, theta33, theta3_graus3, theta53, theta5_graus3, a13 = 43.9f, a23 = 118f, a33 = 18.2f, d43 = 171.9f, k3;
    private float[,] _B1_3 = new float[21, 3];
    private float[,] _B2_3 = new float[21, 3];
    private float[,] _B3_3 = new float[21, 3];

    // Variáveis de controle
    private int m3 = 0;
    private string[] separado3;
    private List<float> comparado_21 = new List<float>();
    private List<float> comparado_22 = new List<float>();
    private List<float> comparado_25 = new List<float>();
    private int curva3 = 1;
    private List<float> dado_enviado1 = new List<float>();
    private int currentAngleIndex3 = 0;
    private int iniciar3;

    // Variáveis para a comunicação UDP
    public UdpClient udpReceiver3;
    public Thread receiveThread3;
    private ConcurrentQueue<float> dataQueue3 = new ConcurrentQueue<float>();
    

    // Velocidade de rotação gradual
    private float velocidadeRotacao3; // Graus por segundo

    // Rotação atual e destino
    private float currentRotationZ3;
    private float targetRotationZ3;

    // Ângulos de destino específicos
    private float anguloInicial3 = -180f;
    private float anguloFinal3 = 180f;

    // Direção da rotação (-1 para anti-horário, 1 para horário)
    private int direcao3 = 1;

    // Flag para controlar transição entre rotações
    private bool emTransicao3 = false;

    // Start is called before the first frame update
    void Start()
    {
        udpReceiver3 = new UdpClient(61564);
        receiveThread3 = new Thread(new ThreadStart(_ReceiveData3));
        receiveThread3.Start();
    }

    // Update is called once per frame
    void Update()
    {
        // Verifica se há dados na fila para processar
        if (!emTransicao3)
        {
            if (currentAngleIndex3 < dado_enviado1.Count)
            {
                Debug.Log("Entrou na rotação " + (currentAngleIndex3) +" do motor 3. E o valor enviado foi: " + dado_enviado1[currentAngleIndex3]);

                // Determina o ângulo de destino com base no dado recebido
                float targetAngle3 = dado_enviado1[currentAngleIndex3];

                // Limita o ângulo dentro do intervalo permitido
                targetAngle3 = Mathf.Clamp(targetAngle3, anguloInicial3, anguloFinal3);

                // Determina a direção da rotação
                if (targetAngle3 > currentRotationZ3)
                {
                    direcao3 = 1; // Rotação horária
                    velocidadeRotacao3 = 14f;
                }
                else
                {
                    direcao3 = -1; // Rotação anti-horária
                    velocidadeRotacao3 = -14f;
                }

                // Atualiza o ângulo de rotação alvo
                targetRotationZ3 = targetAngle3;

                // Inicia a transição
                emTransicao3 = true;
            }
        }

        // Rotaciona gradualmente o objeto em torno do eixo Y
        RotateObjectSmooth3();
        verificar3();
    }

    // Método para verificar se a rotação atingiu o ângulo desejado
    void verificar3()
    {
        //Debug.Log("Valor Z: " + Mathf.Round(elementos3[2].localEulerAngles.z * 1000f) / 1000f + " Valor do Theta3: " + Mathf.Round(theta3_graus3 * 1000f) / 1000f);
        float angulo_euler13 = Mathf.Abs(Mathf.Abs(elementos3[0].localEulerAngles.y) - 360);
        float angulo_euler23 = Mathf.Abs(Mathf.Abs(elementos3[1].localEulerAngles.z) - 360);
        float angulo_euler33 = Mathf.Abs(Mathf.Abs(elementos3[2].localEulerAngles.z) - 360);
        float angulo_euler53 = Mathf.Abs(Mathf.Abs(elementos3[3].localEulerAngles.z) - 360);

        //Debug.Log("Quantidade de angulos: " + comparado_21.Count + ". Indexador atual: " + (currentAngleIndex3 + 1));
        //Debug.Log("Pontos da lista: " + dado_enviado1.Count);
        if (comparado_21[currentAngleIndex3] <= 0)
        {
            comparado_21[currentAngleIndex3] += 360;
        }
        if (comparado_22[currentAngleIndex3] < 0)
        {
            comparado_22[currentAngleIndex3] += 360;
        }
        if (dado_enviado1[currentAngleIndex3] < 0)
        {
            dado_enviado1[currentAngleIndex3] += 360;
        }
        if (comparado_25[currentAngleIndex3] < 0 || comparado_25[currentAngleIndex3] > 360)
        {
            if (comparado_25[currentAngleIndex3] < 0)
            {
                comparado_25[currentAngleIndex3] += 360;
            }
            else
            {
                comparado_25[currentAngleIndex3] -= 360;
            }
        }
        
        float dif13 = Mathf.Abs(angulo_euler13 - comparado_21[currentAngleIndex3]);
        float dif23 = Mathf.Abs(angulo_euler23 - comparado_22[currentAngleIndex3]);
        float dif33 = Mathf.Abs(angulo_euler33 - dado_enviado1[currentAngleIndex3]);
        float dif53 = Mathf.Abs(angulo_euler53 - comparado_25[currentAngleIndex3]);

        //Debug.Log("Theta 1 lido " + angulo_euler13 + ". Angulo 1 enviado " + theta1_graus3 + ". A diferença é: " + dif13);
        //Debug.Log("Theta 2 lido " + angulo_euler23 + ". Angulo 2 enviado " + comparado_22 + ". A diferença é: " + dif23);
        Debug.Log("Theta 3 lido no motor 3: " + angulo_euler33 + ". Angulo 3 enviado " + dado_enviado1[currentAngleIndex3] + ". A diferença é: " + dif33);
        Debug.Log("Theta 5 lido no motor 3: " + angulo_euler53 + ". Angulo 5 enviado " + comparado_25[currentAngleIndex3] + ". A diferença é: " + dif53);
        if (/*dif13 < 0.03f && dif23 < 0.03f &&*/ dif33 < 0.03f && dif53 < 0.03f)
        {
            //Debug.Log("O valor lido é: " + angulo_euler33 + ". O dado enviado foi: " + dado_enviado1[currentAngleIndex3] + ". A diferença é: " + dif33);
            emTransicao3 = false;
            if (dado_enviado1.Count == (currentAngleIndex3 + 1))
            {
                elementos3[2].localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            if (currentAngleIndex3 < 59)
            {
                currentAngleIndex3++;
            }
        }
    }

    // Método para encerrar a thread de recebimento ao destruir o objeto
    void OnDestroy()
    {
        receiveThread3.Abort();
    }

    // Função principal de recebimento e processamento de dados UDP
    void _ReceiveData3()
    {
        while (true)
        {
            IPEndPoint remoteEndPoint3 = new IPEndPoint(IPAddress.Any, 0);
            byte[] receiveBytes3 = udpReceiver3.Receive(ref remoteEndPoint3);
            string receivedData3 = Encoding.ASCII.GetString(receiveBytes3);

            // Debug.Log("Received data in teste3: " + receivedData3);

            string[] separado3 = receivedData3.Split(';');

            // Parse dos pontos recebidos
            pointx_0_3 = float.Parse(separado3[0]);
            pointy_0_3 = float.Parse(separado3[1]);
            pointz_0_3 = float.Parse(separado3[2]);
            pointx_1_3 = float.Parse(separado3[3]);
            pointy_1_3 = float.Parse(separado3[4]);
            pointz_1_3 = float.Parse(separado3[5]);
            pointx_2_3 = float.Parse(separado3[6]);
            pointy_2_3 = float.Parse(separado3[7]);
            pointz_2_3 = float.Parse(separado3[8]);
            pointx_3_3 = float.Parse(separado3[9]);
            pointy_3_3 = float.Parse(separado3[10]);
            pointz_3_3 = float.Parse(separado3[11]);
            pointx_4_3 = float.Parse(separado3[12]);
            pointy_4_3 = float.Parse(separado3[13]);
            pointz_4_3 = float.Parse(separado3[14]);
            pointx_5_3 = float.Parse(separado3[15]);
            pointy_5_3 = float.Parse(separado3[16]);
            pointz_5_3 = float.Parse(separado3[17]);
            pointx_6_3 = float.Parse(separado3[18]);
            pointy_6_3 = float.Parse(separado3[19]);
            pointz_6_3 = float.Parse(separado3[20]);
            pointx_7_3 = float.Parse(separado3[21]);
            pointy_7_3 = float.Parse(separado3[22]);
            pointz_7_3 = float.Parse(separado3[23]);
            pointx_8_3 = float.Parse(separado3[24]);
            pointy_8_3 = float.Parse(separado3[25]);
            pointz_8_3 = float.Parse(separado3[26]);
            pointx_9_3 = float.Parse(separado3[27]);
            pointy_9_3 = float.Parse(separado3[28]);
            pointz_9_3 = float.Parse(separado3[29]);
            pointx_10_3 = float.Parse(separado3[30]);
            pointy_10_3 = float.Parse(separado3[31]);
            pointz_10_3 = float.Parse(separado3[32]);
            pointx_11_3 = float.Parse(separado3[33]);
            pointy_11_3 = float.Parse(separado3[34]);
            pointz_11_3 = float.Parse(separado3[35]);
            pointx_12_3 = float.Parse(separado3[36]);
            pointy_12_3 = float.Parse(separado3[37]);
            pointz_12_3 = float.Parse(separado3[38]);
            pointx_13_3 = float.Parse(separado3[39]);
            pointy_13_3 = float.Parse(separado3[40]);
            pointz_13_3 = float.Parse(separado3[41]);
            pointx_14_3 = float.Parse(separado3[42]);
            pointy_14_3 = float.Parse(separado3[43]);
            pointz_14_3 = float.Parse(separado3[44]);
            iniciar3 = int.Parse(separado3[45]);

            if (iniciar3 > 0)
            {
                curva3 = 1;
                m3 = 0;
                currentAngleIndex3 = 0;
                dado_enviado1.Clear();
                comparado_21.Clear();
                comparado_22.Clear();
                comparado_25.Clear();
                CalculateAngulos();
            }
        }

        void CalculateAngulos()
        {
            // Cálculo dos pontos de Bézier: primeira curva
            int i3 = 0;

            for (float t3 = 0; t3 <= 1; t3 += 0.05f)
            {
                int j3 = 0;

                _PositionX3 = Mathf.Pow(1 - t3, 4) * pointx_0_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointx_1_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointx_2_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointx_3_3 + Mathf.Pow(t3, 4) * pointx_4_3;
                _PositionY3 = Mathf.Pow(1 - t3, 4) * pointy_0_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointy_1_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointy_2_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointy_3_3 + Mathf.Pow(t3, 4) * pointy_4_3;
                _PositionZ3 = Mathf.Pow(1 - t3, 4) * pointz_0_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointz_1_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointz_2_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointz_3_3 + Mathf.Pow(t3, 4) * pointz_4_3;

                _B1_3[i3, j3] = _PositionX3;
                j3++;
                _B1_3[i3, j3] = _PositionY3;
                j3++;
                _B1_3[i3, j3] = _PositionZ3;
                i3++;
            }

            //Cálculo dos pontos de Bézier: segunda curva
            i3 = 0;
            for (float t3 = 0; t3 <= 1; t3 += 0.05f)
            {
                int j3 = 0;

                _PositionX3 = Mathf.Pow(1 - t3, 4) * pointx_5_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointx_6_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointx_7_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointx_8_3 + Mathf.Pow(t3, 4) * pointx_9_3;
                _PositionY3 = Mathf.Pow(1 - t3, 4) * pointy_5_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointy_6_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointy_7_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointy_8_3 + Mathf.Pow(t3, 4) * pointy_9_3;
                _PositionZ3 = Mathf.Pow(1 - t3, 4) * pointz_5_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointz_6_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointz_7_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointz_8_3 + Mathf.Pow(t3, 4) * pointz_9_3;

                _B2_3[i3, j3] = _PositionX3;
                j3++;
                _B2_3[i3, j3] = _PositionY3;
                j3++;
                _B2_3[i3, j3] = _PositionZ3;
                i3++;
            }

            //Cálculo dos pontos de Bézier: terceira curva
            i3 = 0;
            for (float t3 = 0; t3 <= 1; t3 += 0.05f)
            {
                int j3 = 0;

                _PositionX3 = Mathf.Pow(1 - t3, 4) * pointx_10_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointx_11_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointx_12_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointx_13_3 + Mathf.Pow(t3, 4) * pointx_14_3;
                _PositionY3 = Mathf.Pow(1 - t3, 4) * pointy_10_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointy_11_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointy_12_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointy_13_3 + Mathf.Pow(t3, 4) * pointy_14_3;
                _PositionZ3 = Mathf.Pow(1 - t3, 4) * pointz_10_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointz_11_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointz_12_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointz_13_3 + Mathf.Pow(t3, 4) * pointz_14_3;

                _B3_3[i3, j3] = _PositionX3;
                j3++;
                _B3_3[i3, j3] = _PositionY3;
                j3++;
                _B3_3[i3, j3] = _PositionZ3;
                i3++;
            }

            // Cálculo do ângulo de rotação
            for(int q = 0; q < 65; q++)
            {
                if (curva3 == 1)
                {
                    if (m3 < 20)
                    {
                        theta13 = (Mathf.Atan2(_B1_3[m3, 1], _B1_3[m3, 0]));
                        theta1_graus3 = theta13 * (180 / Mathf.PI);
                        comparado_21.Add(theta1_graus3);
                        k3 = (Mathf.Pow(_B1_3[m3, 0], 2) + Mathf.Pow(_B1_3[m3, 1], 2) + Mathf.Pow(_B1_3[m3, 2], 2) + Mathf.Pow(a13, 2) - 2 * a13 * (_B1_3[m3, 0] * Mathf.Cos(theta13) + _B1_3[m3, 1] * Mathf.Sin(theta13)) - Mathf.Pow(a23, 2) - Mathf.Pow(a33, 2) - Mathf.Pow(d43, 2)) / (2 * a23);
                        theta33 = (Mathf.Atan2(d43, a33) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a33, 2) + Mathf.Pow(d43, 2) - Mathf.Pow(k3, 2)), k3));
                        theta3_graus3 = theta33 * (180 / Mathf.PI) + 360;
                        dado_enviado1.Add(306.09f - theta3_graus3);
                        theta233 = Mathf.Atan2(_B1_3[m3, 2] * (a33 + a23 * Mathf.Cos(theta33)) + (d43 + a23 * Mathf.Sin(theta33)) * (_B1_3[m3, 1] * Mathf.Sin(theta13) + _B1_3[m3, 0] * Mathf.Cos(theta13) - a13), -(d43 + (a23 * Mathf.Sin(theta33))) * _B1_3[m3, 2] + (a33 + a23 * Mathf.Cos(theta33)) * (_B1_3[m3, 1] * Mathf.Sin(theta13) + _B1_3[m3, 0] * Mathf.Cos(theta13) - a13));
                        theta23_graus3 = theta233 * (180 / Mathf.PI);
                        theta23 = theta233 - theta33;
                        theta2_graus3 = theta23 * (180 / Mathf.PI);
                        comparado_22.Add(144.58f - theta2_graus3);
                        theta5_graus3 = theta2_graus3 + theta3_graus3;
                        comparado_25.Add(79.6f + theta5_graus3);
                        m3++;
                    }

                    else
                    {
                        //Atualiza o número da curva
                        curva3++;

                        //Reinicia o ciclo partindo da posição final
                        m3 = 0;
                    }
                }

                else
                {
                    if (curva3 == 2)
                    {
                        if (m3 < 20)
                        {
                            theta13 = (Mathf.Atan2(_B2_3[m3, 1], _B2_3[m3, 0]));
                            theta1_graus3 = theta13 * (180 / Mathf.PI);
                            comparado_21.Add(theta1_graus3);
                            k3 = (Mathf.Pow(_B2_3[m3, 0], 2) + Mathf.Pow(_B2_3[m3, 1], 2) + Mathf.Pow(_B2_3[m3, 2], 2) + Mathf.Pow(a13, 2) - 2 * a13 * (_B2_3[m3, 0] * Mathf.Cos(theta13) + _B2_3[m3, 1] * Mathf.Sin(theta13)) - Mathf.Pow(a23, 2) - Mathf.Pow(a33, 2) - Mathf.Pow(d43, 2)) / (2 * a23);
                            theta33 = (Mathf.Atan2(d43, a33) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a33, 2) + Mathf.Pow(d43, 2) - Mathf.Pow(k3, 2)), k3));
                            theta3_graus3 = theta33 * (180 / Mathf.PI) + 360;
                            dado_enviado1.Add(306.09f - theta3_graus3);
                            theta233 = Mathf.Atan2(_B2_3[m3, 2] * (a33 + a23 * Mathf.Cos(theta33)) + (d43 + a23 * Mathf.Sin(theta33)) * (_B2_3[m3, 1] * Mathf.Sin(theta13) + _B2_3[m3, 0] * Mathf.Cos(theta13) - a13), -(d43 + (a23 * Mathf.Sin(theta33))) * _B2_3[m3, 2] + (a33 + a23 * Mathf.Cos(theta33)) * (_B2_3[m3, 1] * Mathf.Sin(theta13) + _B2_3[m3, 0] * Mathf.Cos(theta13) - a13));
                            theta23_graus3 = theta233 * (180 / Mathf.PI);
                            theta23 = theta233 - theta33;
                            theta2_graus3 = theta23 * (180 / Mathf.PI);
                            comparado_22.Add(144.58f - theta2_graus3);
                            theta5_graus3 = theta2_graus3 + theta3_graus3;
                            comparado_25.Add(79.6f + theta5_graus3);
                            m3++;
                        }

                        else
                        {
                            //Atualiza o número da curva
                            curva3++;

                            //Reinicia o ciclo partindo da posição final
                            m3 = 0;
                        }
                    }

                    else
                    {
                        if (curva3 == 3)
                        {
                            if (m3 < 20)
                            {
                                theta13 = (Mathf.Atan2(_B3_3[m3, 1], _B3_3[m3, 0]));
                                theta1_graus3 = theta13 * (180 / Mathf.PI);
                                comparado_21.Add(theta1_graus3);
                                k3 = (Mathf.Pow(_B3_3[m3, 0], 2) + Mathf.Pow(_B3_3[m3, 1], 2) + Mathf.Pow(_B3_3[m3, 2], 2) + Mathf.Pow(a13, 2) - 2 * a13 * (_B3_3[m3, 0] * Mathf.Cos(theta13) + _B3_3[m3, 1] * Mathf.Sin(theta13)) - Mathf.Pow(a23, 2) - Mathf.Pow(a33, 2) - Mathf.Pow(d43, 2)) / (2 * a23);
                                theta33 = (Mathf.Atan2(d43, a33) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a33, 2) + Mathf.Pow(d43, 2) - Mathf.Pow(k3, 2)), k3));
                                theta3_graus3 = theta33 * (180 / Mathf.PI) + 360;
                                dado_enviado1.Add(306.09f - theta3_graus3);
                                theta233 = Mathf.Atan2(_B3_3[m3, 2] * (a33 + a23 * Mathf.Cos(theta33)) + (d43 + a23 * Mathf.Sin(theta33)) * (_B3_3[m3, 1] * Mathf.Sin(theta13) + _B3_3[m3, 0] * Mathf.Cos(theta13) - a13), -(d43 + (a23 * Mathf.Sin(theta33))) * _B3_3[m3, 2] + (a33 + a23 * Mathf.Cos(theta33)) * (_B3_3[m3, 1] * Mathf.Sin(theta13) + _B3_3[m3, 0] * Mathf.Cos(theta13) - a13));
                                theta23_graus3 = theta233 * (180 / Mathf.PI);
                                theta23 = theta233 - theta33;
                                theta2_graus3 = theta23 * (180 / Mathf.PI);
                                comparado_22.Add(144.58f - theta2_graus3);
                                theta5_graus3 = theta2_graus3 + theta3_graus3;
                                comparado_25.Add(79.6f + theta5_graus3);
                                m3++;
                            }

                            else
                            {
                                //Atualiza o número da curva
                                curva3 = 0;

                                //Reinicia o ciclo partindo da posição final
                                m3 = 0;
                            }
                        }
                    }
                }
            }
        }
    }

    //Função para rotacionar o objeto gradualmente em torno do eixo Z
    void RotateObjectSmooth3()
    {
        // Calcula a rotação atual do objeto em torno do eixo Z
        currentRotationZ3 = Mathf.LerpAngle(currentRotationZ3, targetRotationZ3, Time.deltaTime * velocidadeRotacao3 * direcao3);

        // Aplica a rotação gradual
        elementos3[2].localRotation = Quaternion.Euler(0f, 0f, -currentRotationZ3);
    }
}







//using UnityEngine;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Collections.Concurrent;

//public class copia_Teste3 : MonoBehaviour
//{
//    [SerializeField] private Transform[] elementos3;

//    // Variáveis relacionadas aos dados recebidos
//    private float _PositionX3, _PositionY3, _PositionZ3;
//    private float pointx_0_3, pointx_1_3, pointx_2_3, pointx_3_3, pointx_4_3, pointx_5_3, pointx_6_3, pointx_7_3, pointx_8_3, pointx_9_3, pointx_10_3, pointx_11_3, pointx_12_3, pointx_13_3, pointx_14_3;
//    private float pointy_0_3, pointy_1_3, pointy_2_3, pointy_3_3, pointy_4_3, pointy_5_3, pointy_6_3, pointy_7_3, pointy_8_3, pointy_9_3, pointy_10_3, pointy_11_3, pointy_12_3, pointy_13_3, pointy_14_3;
//    private float pointz_0_3, pointz_1_3, pointz_2_3, pointz_3_3, pointz_4_3, pointz_5_3, pointz_6_3, pointz_7_3, pointz_8_3, pointz_9_3, pointz_10_3, pointz_11_3, pointz_12_3, pointz_13_3, pointz_14_3;
//    private float theta13, theta1_graus3, theta23, theta2_graus3, theta233, theta23_graus3, theta33, theta3_graus3, theta53, theta5_graus3, a13 = 43.9f, a23 = 118f, a33 = 18.2f, d43 = 171.9f, k3;
//    private float[,] _B1_3 = new float[21, 3];
//    private float[,] _B2_3 = new float[21, 3];
//    private float[,] _B3_3 = new float[21, 3];

//    // Variáveis de controle
//    private float theta3_graus_old = 0;
//    private bool verdadeiro3 = false;
//    private int m3 = 0;
//    private int l3 = 0;
//    private string[] separado3;
//    private float comparado_21, comparado_22, comparado_23;
//    private int curva3 = 1;

//    // Variáveis para a comunicação UDP
//    public UdpClient udpReceiver3;
//    public Thread receiveThread3;
//    private ConcurrentQueue<float> dataQueue3 = new ConcurrentQueue<float>();
//    private float dado_enviado1;

//    // Velocidade de rotação gradual
//    private float velocidadeRotacao3; // Graus por segundo

//    // Rotação atual e destino
//    private float currentRotationZ3;
//    private float targetRotationZ3;

//    // Ângulos de destino específicos
//    private float anguloInicial3 = -180f;
//    private float anguloFinal3 = 180f;

//    // Direção da rotação (-1 para anti-horário, 1 para horário)
//    private int direcao3 = 1;

//    // Flag para controlar transição entre rotações
//    private bool emTransicao3 = false;

//    // Start is called before the first frame update
//    void Start()
//    {
//        udpReceiver3 = new UdpClient(61564);
//        receiveThread3 = new Thread(new ThreadStart(_ReceiveData3));
//        receiveThread3.Start();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (l3 != 0)
//        {
//            verificar3();
//        }

//        // Verifica se há dados na fila para processar
//        if (dataQueue3.Count > 0 && !emTransicao3)
//        {
//            if (dataQueue3.TryDequeue(out float dados3))
//            {
//                // Debug.Log("Removido objeto: " + dados3 + ". Contagem de elementos na fila: " + dataQueue3.Count);

//                // Determina o ângulo de destino com base no dado recebido
//                float targetAngle3 = dados3;

//                // Limita o ângulo dentro do intervalo permitido
//                targetAngle3 = Mathf.Clamp(targetAngle3, anguloInicial3, anguloFinal3);

//                // Determina a direção da rotação
//                if (dados3 > theta3_graus_old)
//                {
//                    direcao3 = 1; // Rotação horária
//                    velocidadeRotacao3 = 14f;
//                }
//                else
//                {
//                    direcao3 = -1; // Rotação anti-horária
//                    velocidadeRotacao3 = -14f;
//                }

//                // Atualiza o ângulo de rotação alvo
//                targetRotationZ3 = targetAngle3;

//                // Inicia a transição
//                emTransicao3 = true;

//                // Reinicia a flag de verificação
//                verdadeiro3 = false;
//            }
//        }

//        // Rotaciona gradualmente o objeto em torno do eixo Y
//        RotateObjectSmooth3();
//    }

//    // Método para verificar se a rotação atingiu o ângulo desejado
//    void verificar3()
//    {
//        //Debug.Log("Valor Z: " + Mathf.Round(elementos3[2].localEulerAngles.z * 1000f) / 1000f + " Valor do Theta3: " + Mathf.Round(theta3_graus3 * 1000f) / 1000f);
//        float angulo_euler13 = (Mathf.Abs((elementos3[0].localEulerAngles.y * 1000) / 1000) - 360);
//        float angulo_euler23 = (Mathf.Abs((elementos3[1].localEulerAngles.z * 1000) / 1000) - 360);
//        float angulo_euler33 = (Mathf.Abs((elementos3[2].localEulerAngles.z * 1000) / 1000) - 360);
//        float angulo_euler53 = (Mathf.Abs((elementos3[3].localEulerAngles.z * 1000) / 1000) - 360);

//        if ((decimal)theta1_graus3 < 0)
//        {
//            theta1_graus3 += 360;
//        }
//        if (theta1_graus3 == 0)
//        {
//            theta1_graus3 += 360;
//        }
//        if ((decimal)theta2_graus3 < 0)
//        {
//            theta2_graus3 += 360;
//        }
//        if ((decimal)dado_enviado1 < 0)
//        {
//            dado_enviado1 += 360;
//        }
//        if ((decimal)theta5_graus3 < 0)
//        {
//            theta5_graus3 += 360;
//        }

//        float dif13 = Mathf.Round((float)Mathf.Abs(angulo_euler13) - theta1_graus3);
//        float dif23 = Mathf.Round((float)Mathf.Abs(angulo_euler23) - comparado_22);
//        float dif33 = Mathf.Round((float)Mathf.Abs(angulo_euler33) - dado_enviado1);
//        float dif53 = Mathf.Round((float)Mathf.Abs(angulo_euler53) - comparado_23);

//        //Debug.Log("Theta 1 lido " + angulo_euler13 + ". Angulo 1 enviado " + theta1_graus3 + ". A diferença é: " + dif13);
//        //Debug.Log("Theta 2 lido " + angulo_euler23 + ". Angulo 2 enviado " + comparado_22 + ". A diferença é: " + dif23);
//        //Debug.Log("Theta 3 lido " + angulo_euler33 + ". Angulo 3 enviado " + dado_enviado1 + ". A diferença é: " + dif33);
//        //Debug.Log("Theta 5 lido " + angulo_euler53 + ". Angulo 5 enviado " + comparado_23 + ". A diferença é: " + dif53);

//        if (/*Mathf.Abs(dif13) < 0.03f && Mathf.Abs(dif23) < 0.03f &&*/ Mathf.Abs(dif33) < 0.03f /*&& Mathf.Abs(dif53) < 0.03f*/)
//        {
//            Debug.Log("O valor lido é: " + angulo_euler33 + ". O dado enviado foi: " + dado_enviado1 + ". O valor enviado é: " + dado_enviado1 + ". A diferença é: " + dif33);
//            verdadeiro3 = true;
//        }
//    }

//    // Método para encerrar a thread de recebimento ao destruir o objeto
//    void OnDestroy()
//    {
//        receiveThread3.Abort();
//    }

//    // Função principal de recebimento e processamento de dados UDP
//    void _ReceiveData3()
//    {
//        while (true)
//        {
//            IPEndPoint remoteEndPoint3 = new IPEndPoint(IPAddress.Any, 0);
//            byte[] receiveBytes3 = udpReceiver3.Receive(ref remoteEndPoint3);
//            string receivedData3 = Encoding.ASCII.GetString(receiveBytes3);

//            // Debug.Log("Received data in teste3: " + receivedData3);

//            string[] separado3 = receivedData3.Split(';');

//            // Parse dos pontos recebidos
//            pointx_0_3 = float.Parse(separado3[0]);
//            pointy_0_3 = float.Parse(separado3[1]);
//            pointz_0_3 = float.Parse(separado3[2]);
//            pointx_1_3 = float.Parse(separado3[3]);
//            pointy_1_3 = float.Parse(separado3[4]);
//            pointz_1_3 = float.Parse(separado3[5]);
//            pointx_2_3 = float.Parse(separado3[6]);
//            pointy_2_3 = float.Parse(separado3[7]);
//            pointz_2_3 = float.Parse(separado3[8]);
//            pointx_3_3 = float.Parse(separado3[9]);
//            pointy_3_3 = float.Parse(separado3[10]);
//            pointz_3_3 = float.Parse(separado3[11]);
//            pointx_4_3 = float.Parse(separado3[12]);
//            pointy_4_3 = float.Parse(separado3[13]);
//            pointz_4_3 = float.Parse(separado3[14]);
//            pointx_5_3 = float.Parse(separado3[15]);
//            pointy_5_3 = float.Parse(separado3[16]);
//            pointz_5_3 = float.Parse(separado3[17]);
//            pointx_6_3 = float.Parse(separado3[18]);
//            pointy_6_3 = float.Parse(separado3[19]);
//            pointz_6_3 = float.Parse(separado3[20]);
//            pointx_7_3 = float.Parse(separado3[21]);
//            pointy_7_3 = float.Parse(separado3[22]);
//            pointz_7_3 = float.Parse(separado3[23]);
//            pointx_8_3 = float.Parse(separado3[24]);
//            pointy_8_3 = float.Parse(separado3[25]);
//            pointz_8_3 = float.Parse(separado3[26]);
//            pointx_9_3 = float.Parse(separado3[27]);
//            pointy_9_3 = float.Parse(separado3[28]);
//            pointz_9_3 = float.Parse(separado3[29]);
//            pointx_10_3 = float.Parse(separado3[30]);
//            pointy_10_3 = float.Parse(separado3[31]);
//            pointz_10_3 = float.Parse(separado3[32]);
//            pointx_11_3 = float.Parse(separado3[33]);
//            pointy_11_3 = float.Parse(separado3[34]);
//            pointz_11_3 = float.Parse(separado3[35]);
//            pointx_12_3 = float.Parse(separado3[36]);
//            pointy_12_3 = float.Parse(separado3[37]);
//            pointz_12_3 = float.Parse(separado3[38]);
//            pointx_13_3 = float.Parse(separado3[39]);
//            pointy_13_3 = float.Parse(separado3[40]);
//            pointz_13_3 = float.Parse(separado3[41]);
//            pointx_14_3 = float.Parse(separado3[42]);
//            pointy_14_3 = float.Parse(separado3[43]);
//            pointz_14_3 = float.Parse(separado3[44]);

//            // Cálculo dos pontos de Bézier: primeira curva
//            int i3 = 0;

//            for (float t3 = 0; t3 <= 1; t3 += 0.05f)
//            {
//                int j3 = 0;

//                _PositionX3 = Mathf.Pow(1 - t3, 4) * pointx_0_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointx_1_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointx_2_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointx_3_3 + Mathf.Pow(t3, 4) * pointx_4_3;
//                _PositionY3 = Mathf.Pow(1 - t3, 4) * pointy_0_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointy_1_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointy_2_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointy_3_3 + Mathf.Pow(t3, 4) * pointy_4_3;
//                _PositionZ3 = Mathf.Pow(1 - t3, 4) * pointz_0_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointz_1_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointz_2_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointz_3_3 + Mathf.Pow(t3, 4) * pointz_4_3;

//                _B1_3[i3, j3] = _PositionX3;
//                j3++;
//                _B1_3[i3, j3] = _PositionY3;
//                j3++;
//                _B1_3[i3, j3] = _PositionZ3;
//                i3++;
//            }

//            //Cálculo dos pontos de Bézier: segunda curva
//            i3 = 0;
//            for (float t3 = 0; t3 <= 1; t3 += 0.05f)
//            {
//                int j3 = 0;

//                _PositionX3 = Mathf.Pow(1 - t3, 4) * pointx_5_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointx_6_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointx_7_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointx_8_3 + Mathf.Pow(t3, 4) * pointx_9_3;
//                _PositionY3 = Mathf.Pow(1 - t3, 4) * pointy_5_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointy_6_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointy_7_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointy_8_3 + Mathf.Pow(t3, 4) * pointy_9_3;
//                _PositionZ3 = Mathf.Pow(1 - t3, 4) * pointz_5_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointz_6_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointz_7_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointz_8_3 + Mathf.Pow(t3, 4) * pointz_9_3;

//                _B2_3[i3, j3] = _PositionX3;
//                j3++;
//                _B2_3[i3, j3] = _PositionY3;
//                j3++;
//                _B2_3[i3, j3] = _PositionZ3;
//                i3++;
//            }

//            //Cálculo dos pontos de Bézier: terceira curva
//            i3 = 0;
//            for (float t3 = 0; t3 <= 1; t3 += 0.05f)
//            {
//                int j3 = 0;

//                _PositionX3 = Mathf.Pow(1 - t3, 4) * pointx_10_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointx_11_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointx_12_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointx_13_3 + Mathf.Pow(t3, 4) * pointx_14_3;
//                _PositionY3 = Mathf.Pow(1 - t3, 4) * pointy_10_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointy_11_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointy_12_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointy_13_3 + Mathf.Pow(t3, 4) * pointy_14_3;
//                _PositionZ3 = Mathf.Pow(1 - t3, 4) * pointz_10_3 + 4 * Mathf.Pow(1 - t3, 3) * t3 * pointz_11_3 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * pointz_12_3 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * pointz_13_3 + Mathf.Pow(t3, 4) * pointz_14_3;

//                _B3_3[i3, j3] = _PositionX3;
//                j3++;
//                _B3_3[i3, j3] = _PositionY3;
//                j3++;
//                _B3_3[i3, j3] = _PositionZ3;
//                i3++;
//            }

//            // Cálculo do ângulo de rotação

//            if (curva3 == 1)
//            {
//                if (l3 == 0)
//                {
//                    Debug.Log("O incremento m3 é: " + m3);
//                    l3++;
//                    theta13 = (Mathf.Atan2(_B1_3[m3, 1], _B1_3[m3, 0]));
//                    theta1_graus3 = (float)Mathf.Round(theta13 * (180 / Mathf.PI) * 1000) / 1000;
//                    k3 = (Mathf.Pow(_B1_3[m3, 0], 2) + Mathf.Pow(_B1_3[m3, 1], 2) + Mathf.Pow(_B1_3[m3, 2], 2) + Mathf.Pow(a13, 2) - 2 * a13 * (_B1_3[m3, 0] * Mathf.Cos(theta13) + _B1_3[m3, 1] * Mathf.Sin(theta13)) - Mathf.Pow(a23, 2) - Mathf.Pow(a33, 2) - Mathf.Pow(d43, 2)) / (2 * a23);
//                    theta33 = (Mathf.Atan2(d43, a33) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a33, 2) + Mathf.Pow(d43, 2) - Mathf.Pow(k3, 2)), k3));
//                    theta3_graus3 = theta33 * (180 / Mathf.PI) + 360;
//                    theta233 = Mathf.Atan2(_B1_3[m3, 2] * (a33 + a23 * Mathf.Cos(theta33)) + (d43 + a23 * Mathf.Sin(theta33)) * (_B1_3[m3, 1] * Mathf.Sin(theta13) + _B1_3[m3, 0] * Mathf.Cos(theta13) - a13), -(d43 + (a23 * Mathf.Sin(theta33))) * _B1_3[m3, 2] + (a33 + a23 * Mathf.Cos(theta33)) * (_B1_3[m3, 1] * Mathf.Sin(theta13) + _B1_3[m3, 0] * Mathf.Cos(theta13) - a13));
//                    theta23_graus3 = theta233 * (180 / Mathf.PI);
//                    theta23 = theta233 - theta33;
//                    theta2_graus3 = (float)Mathf.Round(theta23 * (180 / Mathf.PI) * 1000) / 1000;
//                    comparado_22 = 144.58f - theta2_graus3;
//                    theta5_graus3 = theta2_graus3 + theta3_graus3;
//                    if (theta5_graus3 > 360)
//                    {
//                        theta5_graus3 -= 360f;
//                    }
//                    comparado_23 = 79.6f + theta5_graus3;
//                    dado_enviado1 = 306.09f - theta3_graus3;
//                    m3++;
//                    dataQueue3.Enqueue(dado_enviado1);
//                }

//                else
//                {
//                    if (verdadeiro3 == true)
//                    {
//                        theta3_graus_old = dado_enviado1;
//                        if (m3 < 21)
//                        {
//                            Debug.Log("O incremento m3 é: " + m3);
//                            theta13 = (Mathf.Atan2(_B1_3[m3, 1], _B1_3[m3, 0]));
//                            theta1_graus3 = (float)Mathf.Round(theta13 * (180 / Mathf.PI) * 1000) / 1000;
//                            k3 = (Mathf.Pow(_B1_3[m3, 0], 2) + Mathf.Pow(_B1_3[m3, 1], 2) + Mathf.Pow(_B1_3[m3, 2], 2) + Mathf.Pow(a13, 2) - 2 * a13 * (_B1_3[m3, 0] * Mathf.Cos(theta13) + _B1_3[m3, 1] * Mathf.Sin(theta13)) - Mathf.Pow(a23, 2) - Mathf.Pow(a33, 2) - Mathf.Pow(d43, 2)) / (2 * a23);
//                            theta33 = (Mathf.Atan2(d43, a33) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a33, 2) + Mathf.Pow(d43, 2) - Mathf.Pow(k3, 2)), k3));
//                            theta3_graus3 = theta33 * (180 / Mathf.PI) + 360;
//                            theta233 = Mathf.Atan2(_B1_3[m3, 2] * (a33 + a23 * Mathf.Cos(theta33)) + (d43 + a23 * Mathf.Sin(theta33)) * (_B1_3[m3, 1] * Mathf.Sin(theta13) + _B1_3[m3, 0] * Mathf.Cos(theta13) - a13), -(d43 + (a23 * Mathf.Sin(theta33))) * _B1_3[m3, 2] + (a33 + a23 * Mathf.Cos(theta33)) * (_B1_3[m3, 1] * Mathf.Sin(theta13) + _B1_3[m3, 0] * Mathf.Cos(theta13) - a13));
//                            theta23_graus3 = theta233 * (180 / Mathf.PI);
//                            theta23 = theta233 - theta33;
//                            theta2_graus3 = (float)Mathf.Round(theta23 * (180 / Mathf.PI) * 1000) / 1000;
//                            comparado_22 = 144.58f - theta2_graus3;
//                            theta5_graus3 = theta2_graus3 + theta3_graus3;
//                            if (theta5_graus3 > 360)
//                            {
//                                theta5_graus3 -= 360f;
//                            }
//                            comparado_23 = 79.6f + theta5_graus3;
//                            dado_enviado1 = 306.09f - theta3_graus3;
//                            m3++;
//                            dataQueue3.Enqueue(dado_enviado1);
//                            verdadeiro3 = false;

//                            // Reinicia a transição entre ângulos
//                            emTransicao3 = false;
//                        }

//                        else
//                        {
//                            //Atualiza o número da curva
//                            curva3++;

//                            //Reinicia o ciclo partindo da posição final
//                            m3 = 0;
//                            targetRotationZ3 = dado_enviado1;
//                            emTransicao3 = false;
//                        }
//                    }
//                }
//            }

//            else
//            {
//                if (curva3 == 2)
//                {
//                    if (verdadeiro3 == true)
//                    {
//                        theta3_graus_old = dado_enviado1;
//                        if (m3 < 21)
//                        {
//                            Debug.Log("O incremento m3 é: " + m3);
//                            theta13 = (Mathf.Atan2(_B2_3[m3, 1], _B2_3[m3, 0]));
//                            theta1_graus3 = (float)Mathf.Round(theta13 * (180 / Mathf.PI) * 1000) / 1000;
//                            k3 = (Mathf.Pow(_B2_3[m3, 0], 2) + Mathf.Pow(_B2_3[m3, 1], 2) + Mathf.Pow(_B2_3[m3, 2], 2) + Mathf.Pow(a13, 2) - 2 * a13 * (_B2_3[m3, 0] * Mathf.Cos(theta13) + _B2_3[m3, 1] * Mathf.Sin(theta13)) - Mathf.Pow(a23, 2) - Mathf.Pow(a33, 2) - Mathf.Pow(d43, 2)) / (2 * a23);
//                            theta33 = (Mathf.Atan2(d43, a33) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a33, 2) + Mathf.Pow(d43, 2) - Mathf.Pow(k3, 2)), k3));
//                            theta3_graus3 = theta33 * (180 / Mathf.PI) + 360;
//                            theta233 = Mathf.Atan2(_B2_3[m3, 2] * (a33 + a23 * Mathf.Cos(theta33)) + (d43 + a23 * Mathf.Sin(theta33)) * (_B2_3[m3, 1] * Mathf.Sin(theta13) + _B2_3[m3, 0] * Mathf.Cos(theta13) - a13), -(d43 + (a23 * Mathf.Sin(theta33))) * _B2_3[m3, 2] + (a33 + a23 * Mathf.Cos(theta33)) * (_B2_3[m3, 1] * Mathf.Sin(theta13) + _B2_3[m3, 0] * Mathf.Cos(theta13) - a13));
//                            theta23_graus3 = theta233 * (180 / Mathf.PI);
//                            theta23 = theta233 - theta33;
//                            theta2_graus3 = (float)Mathf.Round(theta23 * (180 / Mathf.PI) * 1000) / 1000;
//                            comparado_22 = 144.58f - theta2_graus3;
//                            theta5_graus3 = theta2_graus3 + theta3_graus3;
//                            if (theta5_graus3 > 360)
//                            {
//                                theta5_graus3 -= 360f;
//                            }
//                            comparado_23 = 79.6f + theta5_graus3;
//                            dado_enviado1 = 306.09f - theta3_graus3;
//                            m3++;
//                            dataQueue3.Enqueue(dado_enviado1);
//                            verdadeiro3 = false;

//                            // Reinicia a transição entre ângulos
//                            emTransicao3 = false;
//                        }

//                        else
//                        {
//                            //Atualiza o número da curva
//                            curva3++;

//                            //Reinicia o ciclo partindo da posição final
//                            m3 = 0;
//                            targetRotationZ3 = dado_enviado1;
//                            emTransicao3 = false;
//                        }
//                    }
//                }

//                else
//                {
//                    if (verdadeiro3 == true)
//                    {
//                        theta3_graus_old = dado_enviado1;
//                        if (m3 < 21)
//                        {
//                            Debug.Log("O incremento m3 é: " + m3);
//                            theta13 = (Mathf.Atan2(_B3_3[m3, 1], _B3_3[m3, 0]));
//                            theta1_graus3 = (float)Mathf.Round(theta13 * (180 / Mathf.PI) * 1000) / 1000;
//                            k3 = (Mathf.Pow(_B3_3[m3, 0], 2) + Mathf.Pow(_B3_3[m3, 1], 2) + Mathf.Pow(_B3_3[m3, 2], 2) + Mathf.Pow(a13, 2) - 2 * a13 * (_B3_3[m3, 0] * Mathf.Cos(theta13) + _B3_3[m3, 1] * Mathf.Sin(theta13)) - Mathf.Pow(a23, 2) - Mathf.Pow(a33, 2) - Mathf.Pow(d43, 2)) / (2 * a23);
//                            theta33 = (Mathf.Atan2(d43, a33) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a33, 2) + Mathf.Pow(d43, 2) - Mathf.Pow(k3, 2)), k3));
//                            theta3_graus3 = theta33 * (180 / Mathf.PI) + 360;
//                            theta233 = Mathf.Atan2(_B3_3[m3, 2] * (a33 + a23 * Mathf.Cos(theta33)) + (d43 + a23 * Mathf.Sin(theta33)) * (_B3_3[m3, 1] * Mathf.Sin(theta13) + _B3_3[m3, 0] * Mathf.Cos(theta13) - a13), -(d43 + (a23 * Mathf.Sin(theta33))) * _B3_3[m3, 2] + (a33 + a23 * Mathf.Cos(theta33)) * (_B3_3[m3, 1] * Mathf.Sin(theta13) + _B3_3[m3, 0] * Mathf.Cos(theta13) - a13));
//                            theta23_graus3 = theta233 * (180 / Mathf.PI);
//                            theta23 = theta233 - theta33;
//                            theta2_graus3 = (float)Mathf.Round(theta23 * (180 / Mathf.PI) * 1000) / 1000;
//                            comparado_22 = 144.58f - theta2_graus3;
//                            theta5_graus3 = theta2_graus3 + theta3_graus3;
//                            if (theta5_graus3 > 360)
//                            {
//                                theta5_graus3 -= 360f;
//                            }
//                            comparado_23 = 79.6f + theta5_graus3;
//                            dado_enviado1 = 306.09f - theta3_graus3;
//                            m3++;
//                            dataQueue3.Enqueue(dado_enviado1);
//                            verdadeiro3 = false;

//                            // Reinicia a transição entre ângulos
//                            emTransicao3 = false;
//                        }

//                        else
//                        {
//                            //Atualiza o número da curva
//                            curva3++;

//                            //Reinicia o ciclo partindo da posição final
//                            m3 = 0;
//                            targetRotationZ3 = dado_enviado1;
//                            emTransicao3 = false;
//                        }
//                    }
//                }
//            }
//        }
//    }

//    //Função para rotacionar o objeto gradualmente em torno do eixo Z
//    void RotateObjectSmooth3()
//    {
//        // Calcula a rotação atual do objeto em torno do eixo Z
//        currentRotationZ3 = Mathf.LerpAngle(currentRotationZ3, targetRotationZ3, Time.deltaTime * velocidadeRotacao3 * direcao3);

//        // Aplica a rotação gradual
//        elementos3[2].localRotation = Quaternion.Euler(0f, 0f, -currentRotationZ3);
//    }
//}
