using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class Copia_TESTE4_copia : MonoBehaviour
{
    [SerializeField] private Transform[] elementos;

    // Variáveis relacionadas aos dados recebidos
    private float[] pointX = new float[15];
    private float[] pointY = new float[15];
    private float[] pointZ = new float[15];
    private float theta1, theta2, theta3, theta5, a1 = 43.9f, a2 = 118f, a3 = 18.2f, d4 = 171.9f, k;
    private float[,] bezierCurve1 = new float[21, 3];
    private float[,] bezierCurve2 = new float[21, 3];
    private float[,] bezierCurve3 = new float[21, 3];

    // Variáveis de controle
    private bool isTransitioning = false;
    private int currentAngleIndex = 0;
    private int iteration = 0;
    private List<float> angles = new List<float>();
    private float currentRotationZ;
    private float targetRotationZ;

    // Variáveis para a comunicação UDP
    private UdpClient udpReceiver;
    private Thread receiveThread;
    private ConcurrentQueue<float> dataQueue = new ConcurrentQueue<float>();

    // Velocidade de rotação gradual
    private float rotationSpeed; // Graus por segundo

    // Ângulos de destino específicos
    private const float InitialAngle = -180f;
    private const float FinalAngle = 180f;

    // Direção da rotação (-1 para anti-horário, 1 para horário)
    private int rotationDirection = 1; // Por padrão, rotação horária

    // Start is called before the first frame update
    void Start()
    {
        udpReceiver = new UdpClient(61565);
        receiveThread = new Thread(ReceiveData);
        receiveThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isTransitioning && currentAngleIndex < angles.Count)
        {
            targetRotationZ = Mathf.Clamp(angles[currentAngleIndex], InitialAngle, FinalAngle);
            rotationDirection = (targetRotationZ > currentRotationZ) ? 1 : -1;
            rotationSpeed = rotationDirection * 10f;
            isTransitioning = true;
        }

        if (iteration == 59 && Mathf.Abs(Mathf.Abs(elementos[3].localRotation.z) - angles[59]) < 0.03f)
        {
            targetRotationZ = Mathf.Clamp(0, InitialAngle, FinalAngle);
            iteration = 0;
            currentAngleIndex = 0;
        }

        RotateObjectSmooth();
        CheckRotation();
    }

    // Método para verificar se a rotação atingiu o ângulo desejado
    void CheckRotation()
    {
        float angleEuler5 = Mathf.Abs(Mathf.Abs(elementos[3].localEulerAngles.z) - 360);

        float angleDifference = Mathf.Abs(angleEuler5 - angles[iteration]);

        if (angleDifference < 0.03f)
        {
            isTransitioning = false;
            currentAngleIndex++;
            iteration++;
        }
    }

    // Método para encerrar a thread de recebimento ao destruir o objeto
    void OnDestroy()
    {
        receiveThread.Abort();
    }

    // Função principal de recebimento e processamento de dados UDP
    void ReceiveData()
    {
        while (true)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receiveBytes = udpReceiver.Receive(ref remoteEndPoint);
            string receivedData = Encoding.ASCII.GetString(receiveBytes);

            string[] separatedData = receivedData.Split(';');

            for (int i = 0; i < 15; i++)
            {
                pointX[i] = float.Parse(separatedData[i * 3]);
                pointY[i] = float.Parse(separatedData[i * 3 + 1]);
                pointZ[i] = float.Parse(separatedData[i * 3 + 2]);
            }

            int start = int.Parse(separatedData[45]);

            if (start > 0)
            {
                CalculateAngles();
            }
        }
    }

    // Função que executa o cálculo dos pontos de Bézier e dos ângulos de movimentação
    void CalculateAngles()
    {
        CalculateBezierCurve(bezierCurve1, 0, 4);
        CalculateBezierCurve(bezierCurve2, 5, 9);
        CalculateBezierCurve(bezierCurve3, 10, 14);

        angles.Clear();

        for (int i = 0; i < 65; i++)
        {
            if (i < 21)
            {
                CalculateRotation(bezierCurve1, i);
            }
            else if (i < 42)
            {
                CalculateRotation(bezierCurve2, i - 21);
            }
            else
            {
                CalculateRotation(bezierCurve3, i - 42);
            }
        }
    }

    void CalculateBezierCurve(float[,] bezierCurve, int startIndex, int endIndex)
    {
        int i = 0;
        for (float t = 0; t <= 1; t += 0.05f, i++)
        {
            bezierCurve[i, 0] = CalculateBezierPoint(pointX, startIndex, endIndex, t);
            bezierCurve[i, 1] = CalculateBezierPoint(pointY, startIndex, endIndex, t);
            bezierCurve[i, 2] = CalculateBezierPoint(pointZ, startIndex, endIndex, t);
        }
    }

    float CalculateBezierPoint(float[] points, int startIndex, int endIndex, float t)
    {
        float oneMinusT = 1 - t;
        return Mathf.Pow(oneMinusT, 4) * points[startIndex] +
               4 * Mathf.Pow(oneMinusT, 3) * t * points[startIndex + 1] +
               6 * Mathf.Pow(t, 2) * Mathf.Pow(oneMinusT, 2) * points[startIndex + 2] +
               4 * Mathf.Pow(t, 3) * oneMinusT * points[startIndex + 3] +
               Mathf.Pow(t, 4) * points[endIndex];
    }

    void CalculateRotation(float[,] bezierCurve, int index)
    {
        float theta1 = Mathf.Atan2(bezierCurve[index, 1], bezierCurve[index, 0]) * Mathf.Rad2Deg;
        float k = (Mathf.Pow(bezierCurve[index, 0], 2) + Mathf.Pow(bezierCurve[index, 1], 2) + Mathf.Pow(bezierCurve[index, 2], 2) + Mathf.Pow(a1, 2) -
                   2 * a1 * (bezierCurve[index, 0] * Mathf.Cos(theta1) + bezierCurve[index, 1] * Mathf.Sin(theta1)) -
                   Mathf.Pow(a2, 2) - Mathf.Pow(a3, 2) - Mathf.Pow(d4, 2)) / (2 * a2);
        float theta3 = Mathf.Atan2(d4, a3) - Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(a3, 2) + Mathf.Pow(d4, 2) - Mathf.Pow(k, 2)), k) * Mathf.Rad2Deg;
        float theta23 = Mathf.Atan2(bezierCurve[index,2] * (a3 + a2 * Mathf.Cos(theta3)) + (d4 + a2* Mathf.Sin(theta3)) * (bezierCurve[index, 1] * Mathf.Sin(theta1) + bezierCurve[index, 0] * Mathf.Cos(theta1) - a1), -(d4 + (a2 * Mathf.Sin(theta3))) * bezierCurve[index, 2] + (a3 + a2 * Mathf.Cos(theta3)) * (bezierCurve[index, 1] * Mathf.Sin(theta1) + bezierCurve[index, 0] * Mathf.Cos(theta1) - a1));
        float theta2 = 144.58f - (theta23 - theta3) * Mathf.Rad2Deg;
        float theta5 = theta2 + theta3;
    }

    // Rotaciona gradualmente o objeto em torno do eixo Y
    void RotateObjectSmooth()
    {
        if (isTransitioning)
        {
            currentRotationZ = Mathf.MoveTowardsAngle(currentRotationZ, targetRotationZ, rotationSpeed * Time.deltaTime);
            elementos[3].localRotation = Quaternion.Euler(0, 0, currentRotationZ);

            if (Mathf.Approximately(currentRotationZ, targetRotationZ))
            {
                isTransitioning = false;
                currentAngleIndex++;
            }
        }
    }
}
