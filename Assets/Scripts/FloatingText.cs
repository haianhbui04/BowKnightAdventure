using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public TextMesh textMesh;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int randomNumber = Random.Range(1, 101);
        textMesh.text = randomNumber.ToString();
        Destroy(this.gameObject, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
