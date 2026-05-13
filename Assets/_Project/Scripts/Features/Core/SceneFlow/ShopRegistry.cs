using UnityEngine;

public class ShopRegistry : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] shops;

    public MonoBehaviour[] Shops => shops;
}

