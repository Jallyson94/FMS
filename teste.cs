using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

public class teste : MonoBehaviour
{
    [SerializeField] private Transform[] elementos;

    // Variáveis relacionadas aos dados recebidos
    private float Px_0, Py_0, Pz_0, Px_1, Py_1, Pz_1, Px_2, Py_2, Pz_2, Px_3, Py_3, Pz_3, Px_4, Py_4, Pz_4, _PositionX, _PositionY, _PositionZ;
    private float theta1, theta1_graus, theta2, theta2_graus, theta23, theta23_graus, theta3, theta3_graus, theta5_graus, a1 = 43.9f, a2 = 118f, a3 = 18.2f, d4 = 171.9f, k;
    private float[,] _B = new float[20, 3];

    // Variáveis de controle
    private float theta1_graus_old = 0;
    private bool verdadeiro = false;
    private int m = 0;
    private int l = 0;
    private string[] separado;

    // Variáveis para a comunicação UDP
    public UdpClient udpReceiver;
    public Thread receiveThread;
    private ConcurrentQueue<float> dataQueue = new ConcurrentQueue<float>();

    // Velocidade de rotação gradual
    private float velocidadeRotacao = 0.5f; // Graus por segundo

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
        if (l != 0)
        {
            verificar();
        }

        // Verifica se há dados na fila para processar
        if (dataQueue.Count > 0 && !emTransicao)
        {
            if (dataQueue.TryDequeue(out float dados))
            {
                //Debug.Log("Removido objeto: " + dados + ". Contagem de elementos na fila: " + dataQueue.Count);

                // Determina o ângulo de destino com base no dado recebido
                float targetAngle = dados;

                // Limita o ângulo dentro do intervalo permitido
                targetAngle = Mathf.Clamp(targetAngle, anguloInicial, anguloFinal);

                // Determina a direção da rotação
                if (dados > currentRotationY)
                {
                    direcao = 1; // Rotação horária
                    velocidadeRotacao = 0.36f;
                }
                else
                {
                    direcao = -1; // Rotação anti-horária
                    velocidadeRotacao = -0.36f;
                }

                // Atualiza o ângulo de rotação alvo
                targetRotationY = targetAngle;

                // Inicia a transição
                emTransicao = true;

                // Reinicia a flag de verificação
                verdadeiro = false;
            }
        }

        // Rotaciona gradualmente o objeto em torno do eixo Y
        RotateObjectSmooth();
    }

    // Método para verificar se a rotação atingiu o ângulo desejado
    void verificar()
    {
        //Debug.Log("Valor Y: " + Mathf.Round(elementos[0].localEulerAngles.y * 1000f) / 1000f + " Valor do Theta1: " + Mathf.Round(theta1_graus * 1000f) / 1000f);
        float angulo_euler1 = Mathf.Abs(Mathf.Round(elementos[0].localEulerAngles.y * 1000) / 1000);
        float angulo_euler2 = Mathf.Abs((Mathf.Round(elementos[1].localEulerAngles.z * 1000) / 1000) - 360);
        float angulo_euler3 = Mathf.Abs((Mathf.Round(elementos[2].localEulerAngles.z * 1000) / 1000) - 360);
        float angulo_euler5 = Mathf.Abs((Mathf.Round(elementos[3].localEulerAngles.z * 1000) / 1000) - 360);
        if ((decimal)theta1_graus < 0)
        {
            theta1_graus += 360;
        }
        if ((decimal)theta2_graus < 0)
        {
            theta2_graus += 360;
        }
        if ((decimal)theta3_graus < 0)
        {
            theta3_graus += 360;
        }
        if((decimal)theta5_graus < 0)
        {
            theta5_graus += 360;
        }

        float dif1 = Mathf.Round((float)angulo_euler1 - theta1_graus);
        float dif2 = Mathf.Round((float)angulo_euler2 - theta2_graus);
        float dif3 = Mathf.Round((float)angulo_euler3 - theta3_graus);
        float dif5 = Mathf.Round((float)angulo_euler5 - theta5_graus);

        //Debug.Log("O valor lido é: " + angulo_euler1 + ". O valor de theta 1 é: " + (decimal)Mathf.Round(theta1_graus) + ". O incremento é: " + m);

        if (Mathf.Abs(dif1) < 0.3f && Mathf.Abs(dif2) < 0.3f && Mathf.Abs(dif3) < 0.3f /*&& Mathf.Abs(dif5) < 0.3f*/)
        {
            Debug.Log("O valor lido é: " + angulo_euler1 + ". O valor de theta 1 é: " + theta1_graus + ". A diferença é: " + dif1);
            verdadeiro = true;
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

            //Debug.Log("Received data in teste: " + receivedData);

            string[] separado = receivedData.Split(';');

            // Parse dos pontos recebidos
            Px_0 = float.Parse(separado[0]);
            Py_0 = float.Parse(separado[2]);
            Pz_0 = float.Parse(separado[1]);
            Px_1 = float.Parse(separado[3]);
            Py_1 = float.Parse(separado[5]);
            Pz_1 = float.Parse(separado[4]);
            Px_2 = float.Parse(separado[6]);
            Py_2 = float.Parse(separado[8]);
            Pz_2 = float.Parse(separado[7]);
            Px_3 = float.Parse(separado[9]);
            Py_3 = float.Parse(separado[11]);
            Pz_3 = float.Parse(separado[10]);
            Px_4 = float.Parse(separado[12]);
            Py_4 = float.Parse(separado[14]);
            Pz_4 = float.Parse(separado[13]);

            // Cálculo dos pontos de Bézier
            int i = 0;

            for (float t = 0; t <= 1; t += 0.05f)
            {
                int j = 0;

                _PositionX = Mathf.Pow(1 - t, 4) * Px_0 + 4 * Mathf.Pow(1 - t, 3) * t * Px_1 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * Px_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * Px_3 + Mathf.Pow(t, 4) * Px_4;
                _PositionY = Mathf.Pow(1 - t, 4) * Py_0 + 4 * Mathf.Pow(1 - t, 3) * t * Py_1 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * Py_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * Py_3 + Mathf.Pow(t, 4) * Py_4;
                _PositionZ = Mathf.Pow(1 - t, 4) * Pz_0 + 4 * Mathf.Pow(1 - t, 3) * t * Pz_1 + 6 * Mathf.Pow(t, 2) * Mathf.Pow(1 - t, 2) * Pz_2 + 4 * Mathf.Pow(t, 3) * (1 - t) * Pz_3 + Mathf.Pow(t, 4) * Pz_4;

                _B[i, j] = _PositionX;
                j++;
                _B[i, j] = _PositionY;
                j++;
                _B[i, j] = _PositionZ;
                i++;
            }

            // Cálculo do ângulo de rotação
            if (l == 0)
            {
                Debug.Log("O incremento m1 é: " + m);
                l++;
                theta1 = (Mathf.Atan2(_B[m, 2], _B[m, 0]));
                theta1_graus = (float)Mathf.Round(theta1 * (180 / Mathf.PI) * 1000) / 1000;
                k = (Mathf.Pow(_B[m, 0], 2) + Mathf.Pow(_B[m, 2], 2) + Mathf.Pow(_B[m, 1], 2) + Mathf.Pow(a1, 2) - 2 * a1 * (_B[m, 0] * Mathf.Cos(theta1) + _B[m, 2] * Mathf.Sin(theta1)) - Mathf.Pow(a2, 2) - Mathf.Pow(a3, 2) - Mathf.Pow(d4, 2)) / (2 * a2);
                theta3 = (Mathf.Atan2(d4, a3) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a3, 2) + Mathf.Pow(d4, 2) - Mathf.Pow(k, 2)), k));
                theta3_graus = (float)Mathf.Round(theta3 * (180 / Mathf.PI) * 1000) / 1000;
                theta23 = Mathf.Atan2(_B[m, 1] * (a3 + a2 * Mathf.Cos(theta3)) + (d4 + a2 * Mathf.Sin(theta3)) * (_B[m, 2] * Mathf.Sin(theta1) + _B[m, 0] * Mathf.Cos(theta1) - a1), -(d4 + (a2 * Mathf.Sin(theta3))) * _B[m, 1] + (a3 + a2 * Mathf.Cos(theta3)) * (_B[m, 2] * Mathf.Sin(theta1) + _B[m, 0] * Mathf.Cos(theta1) - a1));
                theta23_graus = theta23 * (180 / Mathf.PI);
                theta2 = theta23 - theta3;
                theta2_graus = (float)Mathf.Round(theta2 * (180 / Mathf.PI) * 1000) / 1000;
                theta5_graus = (float)Mathf.Round((theta2_graus + theta3_graus) * (180 / Mathf.PI) * 1000) / 1000;
                dataQueue.Enqueue(theta1_graus);
            }
            else
            {
                if (verdadeiro == true)
                {
                    theta1_graus_old = theta1_graus;
                    m++;
                    Debug.Log("O incremento m1 é: " + m);
                    theta1 = (Mathf.Atan2(_B[m, 2], _B[m, 1]));
                    theta1_graus = (float)Mathf.Round(theta1 * (180 / Mathf.PI) * 1000) / 1000;
                    k = (Mathf.Pow(_B[m, 0], 2) + Mathf.Pow(_B[m, 2], 2) + Mathf.Pow(_B[m, 1], 2) + Mathf.Pow(a1, 2) - 2 * a1 * (_B[m, 0] * Mathf.Cos(theta1) + _B[m, 2] * Mathf.Sin(theta1)) - Mathf.Pow(a2, 2) - Mathf.Pow(a3, 2) - Mathf.Pow(d4, 2)) / (2 * a2);
                    theta3 = (Mathf.Atan2(d4, a3) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a3, 2) + Mathf.Pow(d4, 2) - Mathf.Pow(k, 2)), k));
                    theta3_graus = theta3 * (180 / Mathf.PI);
                    theta23 = Mathf.Atan2(_B[m, 1] * (a3 + a2 * Mathf.Cos(theta3)) + (d4 + a2 * Mathf.Sin(theta3)) * (_B[m, 2] * Mathf.Sin(theta1) + _B[m, 0] * Mathf.Cos(theta1) - a1), -(d4 + (a2 * Mathf.Sin(theta3))) * _B[m, 1] + (a3 + a2 * Mathf.Cos(theta3)) * (_B[m, 2] * Mathf.Sin(theta1) + _B[m, 0] * Mathf.Cos(theta1) - a1));
                    theta23_graus = theta23 * (180 / Mathf.PI);
                    theta2 = theta23 - theta3;
                    theta2_graus = (float)Mathf.Round(theta2 * (180 / Mathf.PI) * 1000) / 1000;
                    theta5_graus = (float)Mathf.Round((theta2_graus + theta3_graus) * (180 / Mathf.PI) * 1000) / 1000;
                    dataQueue.Enqueue(theta1_graus);
                    verdadeiro = false;

                    // Reinicia a transição entre ângulos
                    emTransicao = false;
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
        elementos[0].localRotation = Quaternion.Euler(0f, currentRotationY, 0f);
    }
}
