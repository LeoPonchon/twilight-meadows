using UnityEngine;

public class DialogueRegistry : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] dialogues;

    public MonoBehaviour[] Dialogues => dialogues;
}
