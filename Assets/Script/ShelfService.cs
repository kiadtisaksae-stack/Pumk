using UnityEngine;

public class ShelfService : MonoBehaviour
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent(out GuestAI guest))
        {
            if (guest.servicePool != null)
            {

            }
        }
    }
}
