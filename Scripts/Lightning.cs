using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    public LayerMask hitLayer;
    public float radius;
    public int damage;

    LineRenderer lr;
    List<LineRenderer> bolts;

    public void Start()
    {
        bolts = new List<LineRenderer>();
        lr = GetComponent<LineRenderer>();
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, radius, hitLayer);
        foreach(Collider2D col in cols)
        {
            if(col.gameObject.tag == "Meat")
            {
                FlingMeat meat = col.GetComponent<FlingMeat>();
                if (meat)
                {
                    GameObject obj = new GameObject();
                    obj.transform.SetParent(transform);
                    obj.transform.localPosition = Vector3.zero;
                    LineRenderer line = obj.AddComponent<LineRenderer>();
                    line.material = lr.material;
                    line.startColor = lr.startColor;
                    line.endColor = lr.endColor;
                    line.widthCurve = lr.widthCurve;

                    int spikes = Random.Range(3, 6);
                    line.positionCount = spikes + 2;
                    line.SetPosition(0, transform.position);
                    for (int i = 1; i <= spikes; i++)
                    {
                        float xp = 0;
                        Vector2 point = Vector2.zero;
                        float spikeLength = spikes + 1;
                        float temp = i;
                        if (i % 2 == 0)
                        {
                            while (xp >= 0)
                            {
                                point.x = transform.position.x + ((col.transform.position.x - transform.position.x) * (temp / spikeLength));
                                point.y = transform.position.y + ((col.transform.position.y - transform.position.y) * (temp / spikeLength));
                                point += Random.insideUnitCircle;
                                Vector2 v1 = new Vector2(col.transform.position.x - transform.position.x, col.transform.position.y - transform.position.y);
                                Vector2 v2 = new Vector2(point.x - transform.position.x, point.y - transform.position.y);
                                xp = v1.x * v2.y - v1.y * v2.x;
                            }
                        }
                        else
                        {
                            while (xp <= 0)
                            {
                                point.x = transform.position.x + ((col.transform.position.x - transform.position.x) * (temp / spikeLength));
                                point.y = transform.position.y + ((col.transform.position.y - transform.position.y) * (temp / spikeLength));
                                point += Random.insideUnitCircle;
                                Vector2 v1 = new Vector2(col.transform.position.x - transform.position.x, col.transform.position.y - transform.position.y);
                                Vector2 v2 = new Vector2(point.x - transform.position.x, point.y - transform.position.y);
                                xp = v1.x * v2.y - v1.y * v2.x;
                            }
                        }
                        line.SetPosition(i, point);
                    }
                    line.SetPosition(spikes + 1, col.transform.position);

                    bolts.Add(line);

                    meat.HitMeat(transform.position, damage);
                }
            }
        }
    }

    public void Update()
    {
        foreach (LineRenderer bolt in bolts)
        {
            bolt.startColor = new Color(bolt.startColor.r, bolt.startColor.g, bolt.startColor.b, bolt.startColor.a - (Time.deltaTime * 2));
            bolt.endColor = new Color(bolt.endColor.r, bolt.endColor.g, bolt.endColor.b, bolt.endColor.a - (Time.deltaTime));
        }
    }
}
