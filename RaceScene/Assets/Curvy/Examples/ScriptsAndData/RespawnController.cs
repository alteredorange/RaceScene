using UnityEngine;
using System.Collections;

public class RespawnController : MonoBehaviour {
    public float StartingMinHeight = 20;
    public float StartingMaxHeight = 40;
    public Rect StartingRange = new Rect(-5, -5, 10, 10);
    public Collider RespawnCollider;

	// Use this for initialization
	void Start () {
        Spawn();
	}

    void OnCollisionEnter(Collision col)
    {
        if (col.collider == RespawnCollider)
            Spawn();
    }

    void Spawn()
    {
        transform.position = new Vector3(Random.Range(StartingRange.xMin, StartingRange.xMax),
                                    Random.Range(StartingMinHeight,StartingMaxHeight),
                                    Random.Range(StartingRange.yMin, StartingRange.yMax));
    }
}
