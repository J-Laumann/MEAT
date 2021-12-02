using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlingMeat : MonoBehaviour
{

    Rigidbody2D rb;
    AudioSource audioSource;
    GameController gc;

    public string meatName;
    public int health;

    public Vector2 startForce;
    public Vector2 hitForce;

    public GameObject blood, coin;
    public AudioClip coinSound;
    public Color splatColor;
    public AudioClip[] squishes;
    public GameObject[] splats;
    public bool rotten;

    public float invulnTime;

    // Start is called before the first frame update
    void Start()
    {
        gc = GameObject.Find("GameController").GetComponent<GameController>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        rb.AddForce(startForce);
        rb.AddTorque(100 * Mathf.Clamp(transform.position.x, -1, 1));
        CheckRotten();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Apply torque when center mass isnt 0 (when pinned)
        if(rb.centerOfMass != Vector2.zero)
        {
            float force = 30;
            float deadzone = 0.1f;
            if (rb.worldCenterOfMass.y < transform.TransformPoint(0, 0, 0).y)
                deadzone = 0f;
            float dist = rb.worldCenterOfMass.x - transform.TransformPoint(0, 0, 0).x;
            if(dist > deadzone)
            {
                rb.AddTorque(dist * force);
            }
            else if(dist < -deadzone)
            {
                rb.AddTorque(dist * force);
            }
        }
    }

    public void HitMeat(Vector2 hitPos, int damage)
    {
        Vector2 tempHitForce = hitForce;
        Vector2 mouseWorldPos = hitPos;

        //Calculates a force based on how offset your hit was from center of meat
        Vector2 offset = mouseWorldPos - new Vector2(transform.position.x, transform.position.y);
        tempHitForce.x = tempHitForce.x * offset.x;
        tempHitForce.y = tempHitForce.y * -(offset.y - 0.5f);
        rb.velocity = Vector2.zero;
        rb.AddForce(tempHitForce);

        //Effects (Spawns hit particle and sets color based on meat)
        GameObject bloodEffect = Instantiate(blood, transform.position, transform.rotation);
        var psMain = bloodEffect.GetComponent<ParticleSystem>().main;
        psMain.startColor = splatColor;
        audioSource.PlayOneShot(squishes[Random.Range(0, squishes.Length)]);

        //Takes damage and handles "death"
        health -= damage;
        if (health <= 0)
        {
            GameObject splat = Instantiate(splats[Random.Range(0, splats.Length)], transform.position, transform.rotation);
            splat.GetComponentInChildren<SpriteRenderer>().color = splatColor;
            psMain = splat.GetComponentInChildren<ParticleSystem>().main;
            psMain.startColor = splatColor;
            splat.transform.localScale = transform.localScale;

            if(Random.Range(0,100) < 10)
            {
                for (int i = 0; i < Random.Range(3, 5); i++) {
                    GameObject coinEffect = Instantiate(coin, transform.position, Quaternion.identity);
                    Rigidbody2D coinRb = coinEffect.GetComponent<Rigidbody2D>();
                    coinRb.AddForce(new Vector2(Random.Range(-200, 200), 250));
                    Destroy(coinEffect, 5f);
                }
                AudioSource.PlayClipAtPoint(coinSound, transform.position);
                PlayerPrefs.SetInt("Money", PlayerPrefs.GetInt("Money") + 10);
                if (gc)
                {
                    gc.moneyGainedThisGame += 10;
                    gc.UpdateMoneyText();
                }
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "KillwGore")
        {
            HitMeat(collision.transform.position, 1000);
        }
        CheckMeleeHit(collision);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckMeleeHit(collision);
    }

    private void CheckMeleeHit(Collider2D collision)
    {
        if (invulnTime <= Time.time)
        {
            if (gameObject.layer == 7)
            {
                if (collision.gameObject.tag == "Blade")
                {
                    if ((((Vector2)(gc.weaponEffect.transform.position) - gc.weaponEffectLastPos).magnitude) / Time.deltaTime > 0.2f)
                    {
                        invulnTime = Time.time + 0.3f;
                        HitMeat(collision.transform.position, gc.equippedWeapon.damage);
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Wall")
        {
            GetComponent<Collider2D>().isTrigger = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (gc.GAMEMODE == 0)
        {
            if (collision.gameObject.tag == "Cauldron" && gameObject.layer != 6)
            {
                audioSource.PlayOneShot(gc.sizzle);
                gameObject.layer = 6;
                Vector2 resetVeloc = rb.velocity;
                resetVeloc.y = 0;
                resetVeloc.x = 0;
                rb.velocity = resetVeloc;
                rb.sharedMaterial = null;
                gc.AddIngredient(gameObject);
            }
        }
        if(collision.gameObject.tag == "Meat")
        {
            FlingMeat colMeat = collision.gameObject.GetComponent<FlingMeat>();
            if (colMeat)
            {
                if (colMeat.rotten)
                {
                    //Rotten spread chance
                    if (Random.Range(0, 100) < 25)
                    {
                        rotten = true;
                        CheckRotten();
                    }
                }
            }
        }
        if(collision.gameObject.tag == "Kill")
        {
            Destroy(gameObject);
        }
        if(collision.gameObject.tag == "KillwGore")
        {
            HitMeat(collision.transform.position, 1000);
        }
    }

    public void CheckRotten()
    {
        if (rotten)
        {
            Instantiate(gc.rottingPrefab, transform);
        }
    }
}
