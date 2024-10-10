using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public Transform player;
    float screenPosX;
    float screenPosY;
    public Vector2 offset;
    float cameraSpeed;


    void FixedUpdate()
    {
        
        //lockToPlayer();

    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        //RoomBounds bounding = collision.GetComponent<RoomBounds>();
        //print("cora");
        
        //if (transform.position.x > bounding.minimum.x && transform.position.x < bounding.maximum.x)
        //{
        //    transform.position = new Vector3(Mathf.Clamp(player.position.x, bounding.minimum.x, bounding.maximum.x), Mathf.Clamp(player.position.y, bounding.minimum.y, bounding.maximum.y), -10);
        //}

    }


    //public void lockToPlayer()
    //{
    //    screenPosX = Mathf.FloorToInt((player.position.x) / 16 - 0.0625f / 2);
    //    screenPosY = Mathf.FloorToInt((player.position.y) / 11 + 0.5f);

    //    cameraSpeed = 5f * Vector3.Distance(new Vector3(screenPosX * 16 + offset.x, screenPosY * 11 + offset.y, -2), transform.position);
    //    transform.position = Vector3.MoveTowards(transform.position, new Vector3(screenPosX * 16 + offset.x, screenPosY * 11 + offset.y, -2), 0.3f + cameraSpeed * Time.deltaTime);

    //    if (transform.position != new Vector3(screenPosX * 16 + offset.x, screenPosY * 11 + offset.y, -2))
    //    {
    //        //player.GetComponent<Test2DController>().frozen = true;
    //        //player.position = new Vector3(Mathf.Clamp(player.position.x, screenPosX * 16 + offset.x - 6.5f, screenPosX * 16 + offset.x + 6.5f), player.position.y, player.position.z);
    //    }
    //    else
    //    {
    //        //player.GetComponent<Test2DController>().frozen = false;
    //    }
    //}
}
