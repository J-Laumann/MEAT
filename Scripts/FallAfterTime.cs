using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallAfterTime : MonoBehaviour
{
    public float fallTimer, torqueRange;
    
    // Start is called before the first frame update
    void Start()
    {
        Invoke("Fall", fallTimer);
    }

    void Fall()
    {
        GetComponent<Collider2D>().isTrigger = true;
        Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
        rb.AddTorque(Random.Range(-torqueRange, torqueRange));
        Destroy(gameObject, 5f);
    }


}
