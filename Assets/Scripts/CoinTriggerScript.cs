using UnityEngine;

public class CoinTrigger : MonoBehaviour
{

   
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger"+ collision.name);
        Destroy(gameObject);
    }
}
