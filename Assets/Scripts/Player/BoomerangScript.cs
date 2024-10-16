using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangScript : MonoBehaviour
{
    public GameObject player;
    public Vector2 direction;
    public bool outGoing = true;
    public float outGoingSpeed = 1;
    public float returnSpeed = 1;
    public float lifeTime = 5;
    private float lifeRemaining;
    public LayerMask collideWith;

    // Start is called before the first frame update
    void Start()
    {
        lifeRemaining = lifeTime;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        lifeRemaining -= Time.fixedDeltaTime;
        if (lifeRemaining > 0)
        {
            transform.Translate(direction * 0.05f * outGoingSpeed * lifeRemaining);
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, 0.05f * returnSpeed * -lifeRemaining);
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((((1 << collision.gameObject.layer) & collideWith) != 0) && lifeRemaining > 0)
        {
            lifeRemaining = 0;

        }
        else if ((((1 << collision.gameObject.layer) & collideWith) != 0) && lifeRemaining < 0.5f)
        {
            Destroy(this.gameObject);
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player") && lifeRemaining < 0)
        {
            Destroy(this.gameObject);
        }
    }

}
