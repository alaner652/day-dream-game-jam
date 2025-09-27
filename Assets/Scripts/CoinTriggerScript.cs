using UnityEngine;

public class CoinTrigger : MonoBehaviour
{
    public int coinValue;

    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameManager.Instance.AddScore(coinValue);
        Debug.Log("Trigger"+ collision.name);
        Destroy(gameObject);
    }
}
