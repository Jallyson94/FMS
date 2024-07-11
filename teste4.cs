using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

public class teste4 : MonoBehaviour
{
    [SerializeField] private Transform[] elementos5;

    // Variáveis relacionadas aos dados recebidos
    private float Px_05, Py_05, Pz_05, Px_15, Py_15, Pz_15, Px_25, Py_25, Pz_25, Px_35, Py_35, Pz_35, Px_45, Py_45, Pz_45, _PositionX5, _PositionY5, _PositionZ5;
    private float theta15, theta1_graus5, theta25, theta2_graus5, theta235, theta23_graus5, theta35, theta3_graus5, theta5_graus5, a15 = 43.9f, a25 = 118f, a35 = 18.2f, d45 = 171.9f, k5;
    private float[,] _B5 = new float[20, 3];

    // Variáveis de controle
    private float theta5_graus_old = 0;
    private bool verdadeiro5 = false;
    private int m5 = 0;
    private int l5 = 0;
    private string[] separado5;

    // Variáveis para a comunicação UDP
    public UdpClient udpReceiver5;
    public Thread receiveThread5;
    private ConcurrentQueue<float> dataQueue5 = new ConcurrentQueue<float>();

    // Velocidade de rotação gradual
    private float velocidadeRotacao5 = 0.36f; // Graus por segundo

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
        if (l5 != 0)
        {
            verificar5();
        }

        // Verifica se há dados na fila para processar
        if (dataQueue5.Count > 0 && !emTransicao5)
        {
            if (dataQueue5.TryDequeue(out float dados5))
            {
                //  Debug.Log("Removido objeto: " + dados5 + ". Contagem de elementos na fila: " + dataQueue5.Count);

                // Determina o ângulo de destino com base no dado recebido
                float targetAngle5 = dados5;

                // Limita o ângulo dentro do intervalo permitido
                targetAngle5 = Mathf.Clamp(targetAngle5, anguloInicial5, anguloFinal5);

                // Determina a direção da rotação
                if (dados5 > theta5_graus_old)
                {
                    direcao5 = 1; // Rotação horária
                    velocidadeRotacao5 = 0.36f;
                }
                else
                {
                    direcao5 = -1; // Rotação anti-horária
                    velocidadeRotacao5 = -0.36f;
                }

                // Atualiza o ângulo de rotação alvo
                targetRotationZ5 = targetAngle5;

                // Inicia a transição
                emTransicao5 = true;

                // Reinicia a flag de verificação
                verdadeiro5 = false;
            }
        }

        // Rotaciona gradualmente o objeto em torno do eixo Y
        RotateObjectSmooth5();
    }

    // Método para verificar se a rotação atingiu o ângulo desejado
    void verificar5()
    {
        //Debug.Log("Valor Z: " + Mathf.Round(elementos5[3].localEulerAngles.z * 1000f) / 1000f + " Valor do Theta5: " + Mathf.Round(theta5_graus5 * 1000f) / 1000f);
        float angulo_euler15 = Mathf.Abs(Mathf.Round(elementos5[0].localEulerAngles.y * 1000) / 1000);
        float angulo_euler25 = Mathf.Abs((Mathf.Round(elementos5[1].localEulerAngles.z * 1000) / 1000) - 360);
        float angulo_euler35 = Mathf.Abs((Mathf.Round(elementos5[2].localEulerAngles.z * 1000) / 1000) - 360);
        float angulo_euler55 = Mathf.Abs((Mathf.Round(elementos5[3].localEulerAngles.z * 1000) / 1000) - 360);
        if ((decimal)theta1_graus5 < 0)
        {
            theta1_graus5 += 360;
        }
        if ((decimal)theta2_graus5 < 0)
        {
            theta2_graus5 += 360;
        }
        if ((decimal)theta3_graus5 < 0)
        {
            theta3_graus5 += 360;
        }
        if ((decimal)theta5_graus5 < 0)
        {
            theta5_graus5 += 360;
        }

        float dif15 = Mathf.Round((float)angulo_euler15 - theta1_graus5);
        float dif25 = Mathf.Round((float)angulo_euler25 - theta2_graus5);
        float dif35 = Mathf.Round((float)angulo_euler35 - theta3_graus5);
        float dif55 = Mathf.Round((float)angulo_euler55 - theta5_graus5);

        //Debug.Log("O valor lido é: " + angulo_euler55 + ". O valor de theta 5 é: " + theta5_graus5 + ". O incremento é: " + m5);

        if (Mathf.Abs(dif15) < 0.3f && Mathf.Abs(dif25) < 0.3f && Mathf.Abs(dif35) < 0.3f && Mathf.Abs(dif55) < 0.3f)
        {
            Debug.Log("O valor lido é: " + angulo_euler55 + ". O valor de theta 5 é: " + theta5_graus5 + ". A diferença é: " + dif55);
            verdadeiro5 = true;
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

            // Debug.Log("Received data in teste4: " + receivedData5);

            string[] separado5 = receivedData5.Split(';');

            // Parse dos pontos recebidos
            Px_05 = float.Parse(separado5[0]);
            Py_05 = float.Parse(separado5[2]);
            Pz_05 = float.Parse(separado5[1]);
            Px_15 = float.Parse(separado5[3]);
            Py_15 = float.Parse(separado5[5]);
            Pz_15 = float.Parse(separado5[4]);
            Px_25 = float.Parse(separado5[6]);
            Py_25 = float.Parse(separado5[8]);
            Pz_25 = float.Parse(separado5[7]);
            Px_35 = float.Parse(separado5[9]);
            Py_35 = float.Parse(separado5[11]);
            Pz_35 = float.Parse(separado5[10]);
            Px_45 = float.Parse(separado5[12]);
            Py_45 = float.Parse(separado5[14]);
            Pz_45 = float.Parse(separado5[13]);

            // Cálculo dos pontos de Bézier
            int i = 0;

            for (float t5 = 0; t5 <= 1; t5 += 0.05f)
            {
                int j = 0;

                _PositionX5 = Mathf.Pow(1 - t5, 4) * Px_05 + 4 * Mathf.Pow(1 - t5, 3) * t5 * Px_15 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * Px_25 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * Px_35 + Mathf.Pow(t5, 4) * Px_45;
                _PositionY5 = Mathf.Pow(1 - t5, 4) * Py_05 + 4 * Mathf.Pow(1 - t5, 3) * t5 * Py_15 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * Py_25 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * Py_35 + Mathf.Pow(t5, 4) * Py_45;
                _PositionZ5 = Mathf.Pow(1 - t5, 4) * Pz_05 + 4 * Mathf.Pow(1 - t5, 3) * t5 * Pz_15 + 6 * Mathf.Pow(t5, 2) * Mathf.Pow(1 - t5, 2) * Pz_25 + 4 * Mathf.Pow(t5, 3) * (1 - t5) * Pz_35 + Mathf.Pow(t5, 4) * Pz_45;

                _B5[i, j] = _PositionX5;
                j++;
                _B5[i, j] = _PositionY5;
                j++;
                _B5[i, j] = _PositionZ5;
                i++;
            }

            // Cálculo do ângulo de rotação
            if (l5 == 0)
            {
                Debug.Log("O incremento m5 é: " + m5);
                l5++;
                theta15 = (Mathf.Atan2(_B5[m5, 2], _B5[m5, 0]));
                theta1_graus5 = (float)Mathf.Round(theta15 * (180 / Mathf.PI) * 1000) / 1000;
                k5 = (Mathf.Pow(_B5[m5, 0], 2) + Mathf.Pow(_B5[m5, 2], 2) + Mathf.Pow(_B5[m5, 1], 2) + Mathf.Pow(a15, 2) - 2 * a15 * (_B5[m5, 0] * Mathf.Cos(theta15) + _B5[m5, 2] * Mathf.Sin(theta15)) - Mathf.Pow(a25, 2) - Mathf.Pow(a35, 2) - Mathf.Pow(d45, 2)) / (2 * a25);
                theta35 = (Mathf.Atan2(d45, a35) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a35, 2) + Mathf.Pow(d45, 2) - Mathf.Pow(k5, 2)), k5));
                theta3_graus5 = theta35 * (180 / Mathf.PI);
                theta235 = Mathf.Atan2(_B5[m5, 1] * (a35 + a25 * Mathf.Cos(theta35)) + (d45 + a25 * Mathf.Sin(theta35)) * (_B5[m5, 2] * Mathf.Sin(theta15) + _B5[m5, 0] * Mathf.Cos(theta15) - a15), -(d45 + (a25 * Mathf.Sin(theta35))) * _B5[m5, 1] + (a35 + a25 * Mathf.Cos(theta35)) * (_B5[m5, 2] * Mathf.Sin(theta15) + _B5[m5, 0] * Mathf.Cos(theta15) - a15));
                theta23_graus5 = theta235 * (180 / Mathf.PI);
                theta25 = theta235 - theta35;
                theta2_graus5 = (float)Mathf.Round(theta25 * (180 / Mathf.PI) * 1000) / 1000;
                theta5_graus5 = theta2_graus5 + theta3_graus5;
                dataQueue5.Enqueue(theta5_graus5);
            }
            else
            {
                if (verdadeiro5 == true)
                {
                    theta5_graus_old = theta5_graus5;
                    m5++;
                    Debug.Log("O incremento m5 é: " + m5);
                    theta15 = (Mathf.Atan2(_B5[m5, 2], _B5[m5, 1]));
                    theta1_graus5 = (float)Mathf.Round(theta15 * (180 / Mathf.PI) * 1000) / 1000;
                    k5 = (Mathf.Pow(_B5[m5, 0], 2) + Mathf.Pow(_B5[m5, 2], 2) + Mathf.Pow(_B5[m5, 1], 2) + Mathf.Pow(a15, 2) - 2 * a15 * (_B5[m5, 0] * Mathf.Cos(theta15) + _B5[m5, 2] * Mathf.Sin(theta15)) - Mathf.Pow(a25, 2) - Mathf.Pow(a35, 2) - Mathf.Pow(d45, 2)) / (2 * a25);
                    theta35 = (Mathf.Atan2(d45, a35) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a35, 2) + Mathf.Pow(d45, 2) - Mathf.Pow(k5, 2)), k5));
                    theta3_graus5 = theta35 * (180 / Mathf.PI);
                    theta235 = Mathf.Atan2(_B5[m5, 1] * (a35 + a25 * Mathf.Cos(theta35)) + (d45 + a25 * Mathf.Sin(theta35)) * (_B5[m5, 2] * Mathf.Sin(theta15) + _B5[m5, 0] * Mathf.Cos(theta15) - a15), -(d45 + (a25 * Mathf.Sin(theta35))) * _B5[m5, 1] + (a35 + a25 * Mathf.Cos(theta35)) * (_B5[m5, 2] * Mathf.Sin(theta15) + _B5[m5, 0] * Mathf.Cos(theta15) - a15));
                    theta23_graus5 = theta235 * (180 / Mathf.PI);
                    theta25 = theta235 - theta35;
                    theta2_graus5 = (float)Mathf.Round(theta25 * (180 / Mathf.PI) * 1000) / 1000;
                    theta5_graus5 = theta2_graus5 + theta3_graus5;
                    dataQueue5.Enqueue(theta5_graus5);
                    verdadeiro5 = false;

                    // Reinicia a transição entre ângulos
                    emTransicao5 = false;
                }
            }
        }
    }

    // Função para rotacionar o objeto gradualmente em torno do eixo Y
    void RotateObjectSmooth5()
    {
        // Calcula a rotação atual do objeto em torno do eixo Y
        currentRotationZ5 = Mathf.LerpAngle(currentRotationZ5, targetRotationZ5, Time.deltaTime * velocidadeRotacao5 * direcao5);

        // Aplica a rotação gradual
        elementos5[3].localRotation = Quaternion.Euler(0f, 0f, -currentRotationZ5);
    }
}
