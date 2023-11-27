using System.Collections;
using TMPro;
using UnityEngine;

public class ContadorScript : MonoBehaviour
{
    public Rigidbody2D rb; // Asigna el Rigidbody en el Inspector
    public TextMeshProUGUI text;
    public TextMeshProUGUI textInicio;
    public float contador; // Contador en segundos con dos decimales
    private bool empezarContador; // Indica si debe empezar el contador
    private bool detenerContador; // Indica si debe detener el contador y la corrutina

    void Update()
    {
        // Comprueba si la velocidad en el eje y es diferente de cero
        if (rb.velocity.y > 0.5f)
        {
            // Inicia el contador si aún no ha comenzado
            if (!empezarContador)
            {
                textInicio.gameObject.SetActive(false);
                empezarContador = true;
                StartCoroutine(ContadorCoroutine());
            }
        }
    }

    IEnumerator ContadorCoroutine()
    {
        contador = 0f;
        while (true)
        {
            // Aumenta el contador en centésimas de segundo
            contador += 0.01f;
            text.text = contador.ToString("F2");
            yield return new WaitForSeconds(0.01f);

            // Verifica si se debe detener el contador y la corrutina
            if (detenerContador)
            {
                break;
            }
        }
    }

    public void DetenerContador()
    {
        // Establece la variable detenerContador a true para detener el contador y la corrutina
        detenerContador = true;
    }
}
