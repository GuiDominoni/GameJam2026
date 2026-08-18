using UnityEngine;
using UnityEngine.Events;

public enum TrashType 
{ 
    plastic,
    paper,
    glass,
    metal
}
public class Trash : MonoBehaviour
{
    [SerializeField] private TrashType _trash;
    private AudioSource _trashDescarted;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Garbage>() != null)
        {
            Garbage garbage = collision.GetComponent<Garbage>();
            if(garbage.garbageType.Equals(GarbageType.plastic) && _trash.Equals(TrashType.plastic))
            {
                GameController.Instance.AddTrash();
                Destroy(gameObject);
            }
            else if (garbage.garbageType.Equals(GarbageType.paper) && _trash.Equals(TrashType.paper))
            {
                GameController.Instance.AddTrash();
                Destroy(gameObject);
            }
            else if (garbage.garbageType.Equals(GarbageType.glass) && _trash.Equals(TrashType.glass))
            {
                GameController.Instance.AddTrash();
                Destroy(gameObject);
            }
            else if (garbage.garbageType.Equals(GarbageType.metal) && _trash.Equals(TrashType.metal))
            {
                GameController.Instance.AddTrash();
                Destroy(gameObject);
            }
        }
    }
}
