using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownMovement : MonoBehaviour
{
    [SerializeField]
    private float normalSpeed = 0.5f;   // Vitesse par défaut
    private float moveSpeed;            // Vitesse qui sera modifiée en fonction des entrées
    private Rigidbody2D rb;             // Référence au Rigidbody2D du personnage
    private Vector2 movement;           // Stocke la direction du mouvement

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        moveSpeed = normalSpeed;        // Initialiser moveSpeed avec la vitesse normale
    }

    void Update()
    {
        // Gérer les entrées du joueur (axes Horizontal et Vertical)
        movement.x = Input.GetAxis("Horizontal");
        movement.y = Input.GetAxis("Vertical");

        // Si le joueur maintient la touche Shift, la vitesse est définie à 1, sinon à la vitesse normale
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            moveSpeed = normalSpeed * 2;             // Vitesse plus rapide lorsqu'on appuie sur Shift
        }
        else
        {
            moveSpeed = normalSpeed;    // Retour à la vitesse normale
        }
    }

    void FixedUpdate()
    {
        // Appliquer le mouvement au Rigidbody2D
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
