using UnityEngine;

public class EggCollectible : MonoBehaviour
{
    public bool Collected { get; private set; }

    private Vector3 startPosition;
    private float bobOffset;

    private void Awake()
    {
        startPosition = transform.position;
        bobOffset = Random.Range(0f, 6.28f);
    }

    private void Update()
    {
        if (Collected)
        {
            return;
        }

        float bob = Mathf.Sin(Time.time * 2.4f + bobOffset) * 0.08f;
        transform.position = new Vector3(startPosition.x, startPosition.y + bob, startPosition.z);
    }

    public void MarkCollected()
    {
        Collected = true;
        gameObject.SetActive(false);
    }
}
