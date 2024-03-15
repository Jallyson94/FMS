using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementLona : MonoBehaviour
{
    private Renderer rend;
    private ArrayList objetos = new ArrayList();
    [SerializeField] private float velocity = 20f;
    [SerializeField] private Transform lona;

    void Awake()
    {
        rend = lona.GetComponent<Renderer>();
    }

    void FixedUpdate()
    {
        rend.materials[0].SetTextureOffset("_MainTex", new Vector2(0, velocity) * Time.time);

        if (Input.GetKeyDown("s") == true)
        {
            velocity = 0;
        }
        
        if(Input.GetKeyDown("n") == true)
        {
            velocity = 2f;
        }

        Moviment();
    }

    private void Moviment()
    {
     
        foreach (Transform obj in objetos)
        {
            obj.position += new Vector3(0, 0, -velocity) * Time.deltaTime * 0.1f;
        }
    }

    public void OnTriggerEnter(Collider collider)
    {
        objetos.Add(collider.transform);
    }

    public void OnTriggerExit(Collider collider)
    {
        objetos.Remove(collider.transform);
    }
}
