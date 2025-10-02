# Architecture du système d'inventaire

Ce document décrit l'architecture refactorisée du système d'inventaire pour le jeu Projet Ferme.

## Structure des fichiers

Le système d'inventaire est maintenant divisé en plusieurs composants spécialisés :

- `Inventory.cs` : Modèle de données de l'inventaire
- `ItemStack.cs` : Représente une pile d'objets
- `InventoryUI.cs` : Contrôleur principal de l'UI d'inventaire
- `InventorySlotManager.cs` : Gère la création et la mise à jour des slots
- `InventoryDragAndDrop.cs` : Gère le drag & drop des objets
- `InventoryTooltip.cs` : Gère l'affichage des tooltips
- `SlotSelector.cs` : Gère le sélecteur visuel des slots
- `HotbarController.cs` : Gère la sélection dans la barre d'outils

## Configuration dans Unity

### Objet principal InventoryUI

1. Créez un GameObject vide nommé "InventoryUI"
2. Ajoutez les composants suivants :
   - `InventoryUI` (script)
   - `InventorySlotManager` (script)
   - `InventoryDragAndDrop` (script)
   - `InventoryTooltip` (script)
   - `SlotSelector` (script)

3. Configurez les références :
   - Sur `InventoryUI` :
     - Assignez la référence à l'objet `Inventory`
     - Assignez la référence au `PlayerInput`

   - Sur `InventorySlotManager` :
     - Assignez la référence à l'objet `Inventory`
     - Assignez le prefab `ItemSlot`
     - Assignez le `Transform` du panel d'inventaire
     - Assignez le `Transform` du panel de hotbar

   - Sur `InventoryDragAndDrop` :
     - Assignez la référence à l'objet `Inventory`
     - Assignez la référence au `InventorySlotManager`

   - Sur `InventoryTooltip` :
     - Assignez la référence au panel de tooltip
     - Assignez la référence au TextMeshProUGUI du tooltip

   - Sur `SlotSelector` :
     - Assignez le prefab du sélecteur

### Objet HotbarController

1. Configurez les références :
   - Assignez le prefab du sélecteur
   - Assignez le `Transform` du parent des slots
   - Assignez la référence au `InventoryUI`

## Communication entre les composants

- `InventoryUI` est le coordinateur central qui initialise tous les sous-composants
- `InventorySlotManager` expose des événements pour les interactions avec les slots
- `InventoryUI` s'abonne à ces événements et délègue aux autres composants selon le besoin
- `Inventory` expose un événement `OnInventoryChanged` qui est utilisé pour mettre à jour l'UI quand l'inventaire change

## Points importants

1. L'ordre d'initialisation est crucial : assurez-vous que l'Inventory est initialisé avant les composants d'UI
2. Évitez les dépendances circulaires entre les scripts
3. Les références entre objets doivent être configurées correctement dans l'inspecteur
4. Utilisez les nullchecks pour éviter les NullReferenceExceptions

## Créer un inventaire fonctionnel

1. Ajoutez le préfab "InventoryCanvas" à votre scène
2. Associez-y les scripts mentionnés ci-dessus
3. Configurez toutes les références nécessaires
4. Assurez-vous que votre Inventory est créé avant d'initialiser l'UI

## Dépannage

Si vous rencontrez des erreurs "NullReferenceException", vérifiez :
1. Que toutes les références sont correctement assignées dans l'Inspecteur
2. Que l'ordre d'exécution des scripts est approprié
3. Que les GameObjects sont actifs au moment de l'initialisation 