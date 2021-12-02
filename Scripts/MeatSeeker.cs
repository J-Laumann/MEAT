using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeatSeeker : MonoBehaviour
{

    public string targetID;
    Transform target;
    Transform meatParent;
    Rigidbody2D rb;
    SpriteRenderer sr;
    public float force, lifespan;
    public GameObject leaveEffect;
    public Sprite targetSprite;
    ParticleSystemRenderer psr;

    private void Start()
    {
        if(targetID == null || targetID == "")
        {
            Destroy(gameObject);
            return;
        }
        meatParent = GameObject.FindObjectOfType<GameController>().meatParent;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        psr = GetComponentInChildren<ParticleSystemRenderer>();
        psr.material.mainTexture = targetSprite.texture;
        Invoke("Leave", lifespan);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0)
            return;

        if (target == null || target.gameObject.layer == 6)
        {
            FindTarget();
            if (Vector3.Distance(transform.position, Vector3.zero) > 1)
            {
                if (transform.position.x > 0)
                    sr.flipX = true;
                else
                    sr.flipX = false;
                rb.AddForce((Vector3.zero - transform.position).normalized * force);
            }
        }
        else
        {
            if (transform.position.x > target.position.x)
                sr.flipX = true;
            else
                sr.flipX = false;
            rb.AddForce((target.position - transform.position).normalized * force);
        }
    }

    void FindTarget()
    {
        float closestDist = 1000;
        int closestIndex = -1;
        for(int i = 0; i < meatParent.childCount; i++)
        {
            if (meatParent.GetChild(i).gameObject.layer != 6)
            {
                if (meatParent.GetChild(i).name.Substring(0, 3).Contains(targetID))
                {
                    if (Vector2.Distance(transform.position, meatParent.GetChild(i).position) < closestDist)
                    {
                        closestDist = Vector2.Distance(transform.position, meatParent.GetChild(i).position);
                        closestIndex = i;
                    }
                }
            }
        }
        if(closestIndex > -1)
        {
            target = meatParent.GetChild(closestIndex);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Substring(0, 3).Contains(targetID))
        {
            if (collision.gameObject.layer != 6)
            {
                FlingMeat meat = collision.gameObject.GetComponent<FlingMeat>();
                if (meat)
                    meat.HitMeat(transform.position, 1000);
            }
        }
    }

    void Leave()
    {
        Instantiate(leaveEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
