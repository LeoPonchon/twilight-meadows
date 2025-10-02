using UnityEngine;

/// <summary>
/// Zone de vente: quand le joueur est dans le trigger, on autorise la vente d'items.
/// A placer sur un GameObject avec un Collider2D en isTrigger=true.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class VendorZone : MonoBehaviour
{
    public static bool IsPlayerInside { get; private set; }

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IsPlayerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IsPlayerInside = false;
        }
    }
}
