using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class WeaponAttackSystem : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Inventory playerInventory;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private Transform playerTransform;
    
    [Header("Tilemaps")]
    [SerializeField] private Tilemap foliageTilemap;
    
    [Header("Configuration")]
    [SerializeField] private LayerMask enemyLayerMask = -1;
    [SerializeField] private float attackCooldown = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    private float lastAttackTime = 0f;
    private Camera mainCamera;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        
        // Trouver automatiquement les références si elles ne sont pas assignées
        if (playerInventory == null)
            playerInventory = FindObjectOfType<Inventory>();
        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<InventoryUI>();
        if (playerTransform == null)
            playerTransform = transform;
    }
    
    private void Update()
    {
        HandleAttackInput();
    }
    
    private void HandleAttackInput()
    {
        // Vérifier si le clic gauche est pressé
        if (!Input.GetMouseButtonDown(0))
            return;
        
        // Éviter les clics sur l'UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
        
        // Vérifier le cooldown
        if (Time.time - lastAttackTime < attackCooldown)
            return;
        
        // Vérifier si une arme est équipée
        if (!IsWeaponEquipped())
            return;
        
        // Effectuer l'attaque
        PerformAttack();
    }
    
    private bool IsWeaponEquipped()
    {
        if (playerInventory == null || inventoryUI == null)
            return false;
        
        int slot = inventoryUI.GetCurrentHotbarSlot();
        ItemStack stack = playerInventory.GetItemInSlot(slot);
        
        if (stack == null || stack.itemData == null)
            return false;
        
        // Vérifier si c'est une arme (épée, lance, arc)
        ToolKind toolKind = ItemInstanceFactory.GetToolKind(stack.itemData);
        return toolKind == ToolKind.Sword || toolKind == ToolKind.Spear || toolKind == ToolKind.Bow;
    }
    
    private void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        // Obtenir l'arme équipée
        int slot = inventoryUI.GetCurrentHotbarSlot();
        ItemStack stack = playerInventory.GetItemInSlot(slot);
        ToolKind toolKind = ItemInstanceFactory.GetToolKind(stack.itemData);
        
        // Supprimer les tiles de hautes herbes autour du joueur
        RemoveGrassTilesAroundPlayer();
        
        // Effectuer l'attaque selon le type d'arme
        switch (toolKind)
        {
            case ToolKind.Sword:
                PerformSwordAttack(stack);
                break;
            case ToolKind.Spear:
                PerformSpearAttack(stack);
                break;
            case ToolKind.Bow:
                PerformBowAttack(stack);
                break;
        }
        
        // Consommer la durabilité de l'arme
        ConsumeWeaponDurability(stack);
    }
    
    private void RemoveGrassTilesAroundPlayer()
    {
        if (foliageTilemap == null)
            return;
        
        Vector3Int playerCell = foliageTilemap.WorldToCell(playerTransform.position);
        
        // Zone autour du joueur (rayon de 2 tiles)
        for (int x = playerCell.x - 2; x <= playerCell.x + 2; x++)
        {
            for (int y = playerCell.y - 2; y <= playerCell.y + 2; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase tile = foliageTilemap.GetTile(cell);
                
                // Supprimer toutes les tiles présentes sur la tilemap foliage
                if (tile != null)
                {
                    foliageTilemap.SetTile(cell, null);
                }
            }
        }
    }
    
    private void PerformSwordAttack(ItemStack stack)
    {
        if (stack.itemData is SwordData swordData)
        {
            // Attaque en zone autour du joueur
            Vector2 attackCenter = playerTransform.position;
            float attackRadius = swordData.attackRadius;
            
            // Détecter les ennemis dans la zone d'attaque
            Collider2D[] enemies = Physics2D.OverlapCircleAll(attackCenter, attackRadius, enemyLayerMask);
            
            foreach (Collider2D enemy in enemies)
            {
                // Infliger des dégâts à l'ennemi
                DealDamageToEnemy(enemy.gameObject, swordData.damage);
            }
            
            Debug.Log($"Attaque à l'épée - Dégâts: {swordData.damage}, Rayon: {attackRadius}");
        }
    }
    
    private void PerformSpearAttack(ItemStack stack)
    {
        if (stack.itemData is SpearData spearData)
        {
            // Attaque en ligne dans la direction de la souris
            Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 attackDirection = (mouseWorldPos - (Vector2)playerTransform.position).normalized;
            Vector2 attackEnd = (Vector2)playerTransform.position + attackDirection * spearData.attackRange;
            
            // Détecter les ennemis dans la ligne d'attaque
            RaycastHit2D[] hits = Physics2D.RaycastAll(playerTransform.position, attackDirection, spearData.attackRange, enemyLayerMask);
            
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null)
                {
                    DealDamageToEnemy(hit.collider.gameObject, spearData.damage);
                }
            }
            
            Debug.Log($"Attaque à la lance - Dégâts: {spearData.damage}, Portée: {spearData.attackRange}");
        }
    }
    
    private void PerformBowAttack(ItemStack stack)
    {
        if (stack.itemData is BowData bowData)
        {
            // Attaque à distance vers la position de la souris
            Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 attackDirection = (mouseWorldPos - (Vector2)playerTransform.position).normalized;
            
            // Ajouter une légère imprécision
            float accuracyOffset = Random.Range(-bowData.accuracy, bowData.accuracy);
            attackDirection = Quaternion.Euler(0, 0, accuracyOffset * 10f) * attackDirection;
            
            // Détecter les ennemis dans la direction de tir
            RaycastHit2D hit = Physics2D.Raycast(playerTransform.position, attackDirection, bowData.attackRange, enemyLayerMask);
            
            if (hit.collider != null)
            {
                DealDamageToEnemy(hit.collider.gameObject, bowData.damage);
            }
            
            Debug.Log($"Attaque à l'arc - Dégâts: {bowData.damage}, Portée: {bowData.attackRange}");
        }
    }
    
    private void DealDamageToEnemy(GameObject enemy, int damage)
    {
        // Ici vous pouvez ajouter la logique pour infliger des dégâts
        // Par exemple, chercher un composant StatsManager ou EnemyHealth
        Debug.Log($"Dégâts infligés à {enemy.name}: {damage}");
        
        // Exemple de logique de dégâts (à adapter selon votre système)
        var statsComponent = enemy.GetComponent<StatsManager>();
        if (statsComponent != null)
        {
            statsComponent.health -= damage;
            if (statsComponent.health <= 0)
            {
                statsComponent.health = 0;
                Debug.Log($"{enemy.name} est mort!");
            }
        }
    }
    
    private void ConsumeWeaponDurability(ItemStack stack)
    {
        if (stack.itemInstance is ToolInstance toolInstance)
        {
            toolInstance.Use();
            
            // Mettre à jour l'UI si nécessaire
            if (inventoryUI != null)
            {
                inventoryUI.UpdateInventoryUI();
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || playerTransform == null)
            return;
        
        // Dessiner la zone de suppression des herbes
        Gizmos.color = Color.red;
        Vector3Int playerCell = foliageTilemap != null ? foliageTilemap.WorldToCell(playerTransform.position) : Vector3Int.zero;
        Vector3 worldPos = foliageTilemap != null ? foliageTilemap.CellToWorld(playerCell) : playerTransform.position;
        Gizmos.DrawWireCube(worldPos + Vector3.one * 0.5f, Vector3.one * 5f);
        
        // Dessiner la zone d'attaque de l'épée si une épée est équipée
        if (IsWeaponEquipped())
        {
            int slot = inventoryUI != null ? inventoryUI.GetCurrentHotbarSlot() : 0;
            ItemStack stack = playerInventory != null ? playerInventory.GetItemInSlot(slot) : null;
            
            if (stack?.itemData is SwordData swordData)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(playerTransform.position, swordData.attackRadius);
            }
        }
    }
}
