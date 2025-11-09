using UnityEngine;

public class Suriken : MonoBehaviour
{
    public Transform[] points;
    public int currentPointIndex;
    public float timeToMove = 3f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MoveSuriken();
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    void MoveSuriken()
    {
        currentPointIndex++;

        if (currentPointIndex >= points.Length)
        {
            currentPointIndex = 0;
        }

        this.gameObject.LeanMove(points[currentPointIndex].position, timeToMove).setEaseOutBack().setOnComplete(MoveSuriken);
    }
}
