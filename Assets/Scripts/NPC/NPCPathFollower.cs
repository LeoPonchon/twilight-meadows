using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fait suivre au NPC une liste de points (waypoints) avec un temps d'arrêt à chaque point.
/// Déplacement simple en ligne droite avec Rigidbody2D.MovePosition.
/// </summary>
public class NPCPathFollower : MonoBehaviour
{
	[System.Serializable]
	public class Waypoint
	{
		public Transform targetTransform;
		public float stopTimeSeconds = 0f;
		
		public Vector2 GetPosition()
		{
			return targetTransform != null ? targetTransform.position : Vector2.zero;
		}
	}

	[Header("Path")]
	[SerializeField] private List<Waypoint> waypoints = new List<Waypoint>();
	[SerializeField] private bool loop = true;

	[Header("Movement")]
	[SerializeField] private float moveSpeed = 2f;
	[SerializeField] private float reachThreshold = 0.05f;

	private Rigidbody2D rb2d;
	private Animator animator;
	private NPCController npcController;
	private int currentIndex = 0;
	private float stopTimer = 0f;
	private int playerBlockers = 0;
	private Vector2 lastMoveDir = Vector2.down;

	private void Awake()
	{
		rb2d = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		npcController = GetComponent<NPCController>();
		if (rb2d == null)
		{
			rb2d = gameObject.AddComponent<Rigidbody2D>();
		}

		// Config RB pour éviter déviation par collisions
		rb2d.bodyType = RigidbodyType2D.Kinematic;
		rb2d.gravityScale = 0f;
		rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;
	}

	private void FixedUpdate()
	{
		if (waypoints == null || waypoints.Count == 0) return;
		if (playerBlockers > 0 || IsDialogueOpen())
		{
			SetAnimatorMoving(false);
			return; // Pause si le joueur bloque ou dialogue en cours
		}

		var target = waypoints[currentIndex];
		if (target.targetTransform == null) return; // Skip si le transform est null
		
		Vector2 currentPos = rb2d.position;
		Vector2 toTarget = target.GetPosition() - currentPos;
		float distance = toTarget.magnitude;

		// Gestion du stop aux points
		if (distance <= reachThreshold)
		{
			if (stopTimer <= 0f)
			{
				stopTimer = target.stopTimeSeconds;
			}
			if (stopTimer > 0f)
			{
				stopTimer -= Time.fixedDeltaTime;
				SetAnimatorMoving(false);
				return;
			}

			// Passer au prochain point
			currentIndex++;
			if (currentIndex >= waypoints.Count)
			{
				if (loop)
				{
					currentIndex = 0;
				}
				else
				{
					currentIndex = waypoints.Count - 1;
					return;
				}
			}
			return;
		}

		// Mouvement vers le point
		Vector2 direction = toTarget.normalized;
		UpdateAnimatorDirection(direction);
		Vector2 nextPos = currentPos + direction * moveSpeed * Time.fixedDeltaTime;
		rb2d.MovePosition(nextPos);
		SetAnimatorMoving(true);
	}

	private void UpdateAnimatorDirection(Vector2 direction)
	{
		if (direction.sqrMagnitude > 0.0001f)
		{
			lastMoveDir = direction;
		}
		if (animator != null)
		{
			animator.SetFloat("moveX", lastMoveDir.x);
			animator.SetFloat("moveY", lastMoveDir.y);
		}
	}

	private void SetAnimatorMoving(bool moving)
	{
		if (animator == null) return;
		
		// Utiliser un paramètre booléen pour marche/idle si disponible
		if (animator.parameters.Length > 0)
		{
			// Chercher un paramètre "isMoving" ou similaire
			var isMovingParam = System.Array.Find(animator.parameters, p => p.name == "isMoving" || p.name == "moving");
			if (isMovingParam != null && isMovingParam.type == AnimatorControllerParameterType.Bool)
			{
				animator.SetBool(isMovingParam.name, moving);
			}
		}
		
		// Toujours définir la direction
		animator.SetFloat("moveX", lastMoveDir.x);
		animator.SetFloat("moveY", lastMoveDir.y);
		
		// Fallback : utiliser la vitesse si pas de paramètre booléen
		animator.speed = moving ? 1f : 0f;
	}

	private bool IsDialogueOpen()
	{
		return npcController != null && npcController.dialogueUI != null && npcController.dialogueUI.activeInHierarchy;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.collider.CompareTag("Player"))
		{
			playerBlockers++;
			Debug.Log($"NPC bloqué par joueur (collision). Blockers: {playerBlockers}");
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if (collision.collider.CompareTag("Player"))
		{
			playerBlockers = Mathf.Max(0, playerBlockers - 1);
			Debug.Log($"NPC débloqué par joueur (collision). Blockers: {playerBlockers}");
		}
	}

	public void SetWaypoints(List<Waypoint> newWaypoints, bool shouldLoop)
	{
		waypoints = newWaypoints ?? new List<Waypoint>();
		loop = shouldLoop;
		currentIndex = 0;
		stopTimer = 0f;
	}
}


