using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;

public class teste3 : MonoBehaviour
{
    [SerializeField] private Transform[] elementos3;

    // Variáveis relacionadas aos dados recebidos
    private float Px_03, Py_03, Pz_03, Px_13, Py_13, Pz_13, Px_23, Py_23, Pz_23, Px_33, Py_33, Pz_33, Px_43, Py_43, Pz_43, _PositionX3, _PositionY3, _PositionZ3;
    private float theta13, theta1_graus3, theta23, theta2_graus3, theta233, theta23_graus3, theta33, theta3_graus3, theta53, theta5_graus3, a13 = 43.9f, a23 = 118f, a33 = 18.2f, d43 = 171.9f, k3;
    private float[,] _B3 = new float[20, 3];

    // Variáveis de controle
    private float theta3_graus_old = 0;
    private bool verdadeiro3 = false;
    private int m3 = 0;
    private int l3 = 0;
    private string[] separado3;

    // Variáveis para a comunicação UDP
    public UdpClient udpReceiver3;
    public Thread receiveThread3;
    private ConcurrentQueue<float> dataQueue3 = new ConcurrentQueue<float>();

    //RigidBody do objeto a ser rotacionado
    private Rigidbody rb;

    // Definição do torque a ser alimentado
    //private float torque = 1f;

    // Velocidade de rotação gradual
    private float velocidadeRotacao3 = 0.36f; // Graus por segundo

    // Rotação atual e destino
    private float currentRotationZ3;
    private float targetRotationZ3;

    // Ângulos de destino específicos
    private float anguloInicial3 = -180f;
    private float anguloFinal3 = 180f;

    // Direção da rotação (-1 para anti-horário, 1 para horário)
    private int direcao3 = 1; // Por padrão, rotação horária

    // Flag para controlar transição entre rotações
    private bool emTransicao3 = false;

    // Start is called before the first frame update
    void Start()
    {
        udpReceiver3 = new UdpClient(61564);
        receiveThread3 = new Thread(new ThreadStart(_ReceiveData3));
        receiveThread3.Start();

        //rb = elementos3[2].GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (l3 != 0)
        {
            verificar3();
        }

        // Verifica se há dados na fila para processar
        if (dataQueue3.Count > 0 && !emTransicao3)
        {
            if (dataQueue3.TryDequeue(out float dados3))
            {
                // Debug.Log("Removido objeto: " + dados3 + ". Contagem de elementos na fila: " + dataQueue3.Count);

                // Determina o ângulo de destino com base no dado recebido
                float targetAngle3 = dados3;

                // Limita o ângulo dentro do intervalo permitido
                targetAngle3 = Mathf.Clamp(targetAngle3, anguloInicial3, anguloFinal3);

                // Determina a direção da rotação
                if (dados3 > theta3_graus_old)
                {
                    direcao3 = 1; // Rotação horária
                    velocidadeRotacao3 = 0.36f;
                }
                else
                {
                    direcao3 = -1; // Rotação anti-horária
                    velocidadeRotacao3 = -0.36f;
                }

                // Atualiza o ângulo de rotação alvo
                targetRotationZ3 = targetAngle3;

                // Inicia a transição
                emTransicao3 = true;

                // Reinicia a flag de verificação
                verdadeiro3 = false;
            }
        }

        // Rotaciona gradualmente o objeto em torno do eixo Y
        RotateObjectSmooth3();
    }

    // Método para verificar se a rotação atingiu o ângulo desejado
    void verificar3()
    {
        //Debug.Log("Valor Z: " + Mathf.Round(elementos3[2].localEulerAngles.z * 1000f) / 1000f + " Valor do Theta3: " + Mathf.Round(theta3_graus3 * 1000f) / 1000f);
        float angulo_euler13 = Mathf.Abs(Mathf.Round(elementos3[0].localEulerAngles.y * 1000) / 1000);
        float angulo_euler23 = Mathf.Abs((Mathf.Round(elementos3[1].localEulerAngles.z * 1000) / 1000) - 360);
        float angulo_euler33 = Mathf.Abs((Mathf.Round(elementos3[2].localEulerAngles.z * 1000) / 1000) - 360);
        float angulo_euler53 = Mathf.Abs((Mathf.Round(elementos3[3].localEulerAngles.z * 1000) / 1000) - 360);

        if ((decimal)theta1_graus3 < 0)
        {
            theta1_graus3 += 360;
        }
        if ((decimal)theta2_graus3 < 0)
        {
            theta2_graus3 += 360;
        }
        if ((decimal)theta3_graus3 < 0)
        {
            theta3_graus3 += 360;
        }
        if ((decimal)theta5_graus3 < 0)
        {
            theta5_graus3 += 360;
        }

        float dif13 = Mathf.Round((float)angulo_euler13 - theta1_graus3);
        float dif23 = Mathf.Round((float)angulo_euler23 - theta2_graus3);
        float dif33 = Mathf.Round((float)angulo_euler33 - theta3_graus3);
        float dif53 = Mathf.Round((float)angulo_euler53 - theta5_graus3);


        //Debug.Log("O valor lido é: " + angulo_euler33 + ". O valor de theta 3 é: " + theta3_graus3 + ". O incremento é: " + m3);

        if (Mathf.Abs(dif13) < 0.3f && Mathf.Abs(dif23) < 0.3f && Mathf.Abs(dif33) < 0.3f /*&& Mathf.Abs(dif53) < 0.3f*/)
        {
            Debug.Log("O valor lido é: " + angulo_euler33 + ". O valor de theta 3 é: " + theta3_graus3 + ". A diferença é: " + dif33);
            verdadeiro3 = true;
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
            Px_03 = float.Parse(separado3[0]);
            Py_03 = float.Parse(separado3[2]);
            Pz_03 = float.Parse(separado3[1]);
            Px_13 = float.Parse(separado3[3]);
            Py_13 = float.Parse(separado3[5]);
            Pz_13 = float.Parse(separado3[4]);
            Px_23 = float.Parse(separado3[6]);
            Py_23 = float.Parse(separado3[8]);
            Pz_23 = float.Parse(separado3[7]);
            Px_33 = float.Parse(separado3[9]);
            Py_33 = float.Parse(separado3[11]);
            Pz_33 = float.Parse(separado3[10]);
            Px_43 = float.Parse(separado3[12]);
            Py_43 = float.Parse(separado3[14]);
            Pz_43 = float.Parse(separado3[13]);

            // Cálculo dos pontos de Bézier
            int i3 = 0;

            for (float t3 = 0; t3 <= 1; t3 += 0.05f)
            {
                int j3 = 0;

                _PositionX3 = Mathf.Pow(1 - t3, 4) * Px_03 + 4 * Mathf.Pow(1 - t3, 3) * t3 * Px_13 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * Px_23 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * Px_33 + Mathf.Pow(t3, 4) * Px_43;
                _PositionY3 = Mathf.Pow(1 - t3, 4) * Py_03 + 4 * Mathf.Pow(1 - t3, 3) * t3 * Py_13 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * Py_23 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * Py_33 + Mathf.Pow(t3, 4) * Py_43;
                _PositionZ3 = Mathf.Pow(1 - t3, 4) * Pz_03 + 4 * Mathf.Pow(1 - t3, 3) * t3 * Pz_13 + 6 * Mathf.Pow(t3, 2) * Mathf.Pow(1 - t3, 2) * Pz_23 + 4 * Mathf.Pow(t3, 3) * (1 - t3) * Pz_33 + Mathf.Pow(t3, 4) * Pz_43;

                _B3[i3, j3] = _PositionX3;
                j3++;
                _B3[i3, j3] = _PositionY3;
                j3++;
                _B3[i3, j3] = _PositionZ3;
                i3++;
            }

            // Cálculo do ângulo de rotação
            if (l3 == 0)
            {
                Debug.Log("O incremento m3 é: " + m3);
                l3++;
                theta13 = (Mathf.Atan2(_B3[m3, 2], _B3[m3, 0]));
                theta1_graus3 = (float)Mathf.Round(theta13 * (180 / Mathf.PI) * 1000) / 1000;
                k3 = (Mathf.Pow(_B3[m3, 0], 2) + Mathf.Pow(_B3[m3, 2], 2) + Mathf.Pow(_B3[m3, 1], 2) + Mathf.Pow(a13, 2) - 2 * a13 * (_B3[m3, 0] * Mathf.Cos(theta13) + _B3[m3, 2] * Mathf.Sin(theta13)) - Mathf.Pow(a23, 2) - Mathf.Pow(a33, 2) - Mathf.Pow(d43, 2)) / (2 * a23);
                theta33 = (Mathf.Atan2(d43, a33) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a33, 2) + Mathf.Pow(d43, 2) - Mathf.Pow(k3, 2)), k3));
                theta3_graus3 = theta33 * (180 / Mathf.PI);
                theta233 = Mathf.Atan2(_B3[m3, 1] * (a33 + a23 * Mathf.Cos(theta33)) + (d43 + a23 * Mathf.Sin(theta33)) * (_B3[m3, 2] * Mathf.Sin(theta13) + _B3[m3, 0] * Mathf.Cos(theta13) - a13), -(d43 + (a23 * Mathf.Sin(theta33))) * _B3[m3, 1] + (a33 + a23 * Mathf.Cos(theta33)) * (_B3[m3, 2] * Mathf.Sin(theta13) + _B3[m3, 0] * Mathf.Cos(theta13) - a13));
                theta23_graus3 = theta233 * (180 / Mathf.PI);
                theta23 = theta233 - theta33;
                theta2_graus3 = (float)Mathf.Round(theta23 * (180 / Mathf.PI) * 1000) / 1000;
                theta5_graus3 = theta2_graus3 + theta3_graus3;
                dataQueue3.Enqueue(theta3_graus3);
            }
            else
            {
                if (verdadeiro3 == true)
                {
                    theta3_graus_old = theta3_graus3;
                    m3++;
                    Debug.Log("O incremento m3 é: " + m3);
                    theta13 = (Mathf.Atan2(_B3[m3, 2], _B3[m3, 1]));
                    theta1_graus3 = (float)Mathf.Round(theta13 * (180 / Mathf.PI) * 1000) / 1000;
                    k3 = (Mathf.Pow(_B3[m3, 0], 2) + Mathf.Pow(_B3[m3, 2], 2) + Mathf.Pow(_B3[m3, 1], 2) + Mathf.Pow(a13, 2) - 2 * a13 * (_B3[m3, 0] * Mathf.Cos(theta13) + _B3[m3, 2] * Mathf.Sin(theta13)) - Mathf.Pow(a23, 2) - Mathf.Pow(a33, 2) - Mathf.Pow(d43, 2)) / (2 * a23);
                    theta33 = (Mathf.Atan2(d43, a33) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a33, 2) + Mathf.Pow(d43, 2) - Mathf.Pow(k3, 2)), k3));
                    theta3_graus3 = theta33 * (180 / Mathf.PI);
                    theta233 = Mathf.Atan2(_B3[m3, 1] * (a33 + a23 * Mathf.Cos(theta33)) + (d43 + a23 * Mathf.Sin(theta33)) * (_B3[m3, 2] * Mathf.Sin(theta13) + _B3[m3, 0] * Mathf.Cos(theta13) - a13), -(d43 + (a23 * Mathf.Sin(theta33))) * _B3[m3, 1] + (a33 + a23 * Mathf.Cos(theta33)) * (_B3[m3, 2] * Mathf.Sin(theta13) + _B3[m3, 0] * Mathf.Cos(theta13) - a13));
                    theta23_graus3 = theta233 * (180 / Mathf.PI);
                    theta23 = theta233 - theta33;
                    theta2_graus3 = (float)Mathf.Round(theta23 * (180 / Mathf.PI) * 1000) / 1000;
                    theta5_graus3 = theta2_graus3 + theta3_graus3;
                    dataQueue3.Enqueue(theta3_graus3);
                    verdadeiro3 = false;

                    // Reinicia a transição entre ângulos
                    emTransicao3 = false;
                }
            }
        }
    }

    //Função para rotacionar o objeto gradualmente em torno do eixo Y
    void RotateObjectSmooth3()
    {
        // Calcula a rotação atual do objeto em torno do eixo Y
        currentRotationZ3 = Mathf.LerpAngle(currentRotationZ3, targetRotationZ3, Time.deltaTime * velocidadeRotacao3 * direcao3);

        // Aplica a rotação gradual
        elementos3[2].localRotation = Quaternion.Euler(0f, 0f, -currentRotationZ3);
    }
}
