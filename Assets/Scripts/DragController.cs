using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PhysicsDragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Rigidbody rb;
    private Camera cam;
    private Vector3 offset;
    private float zCoord;
    private bool isDragging;

    [Header("Sürükleme Ayarları")]
    [SerializeField] private float liftHeight = 0.1f;
    [SerializeField] private float dragForce = 5f;

    [Header("Snap Ayarları")]
    [SerializeField] private float snapDistance = 1.0f;
    [SerializeField] private float snapSpeed = 5f;
    [SerializeField] private string snapTag = "Tile";

    private Vector3 startPosition;
    private Coroutine snapCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        startPosition = transform.position;
    }
    void Start()
    {
        StartCoroutine(SmoothSnap());
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        zCoord = cam.WorldToScreenPoint(transform.position).z;
        offset = transform.position - GetInputWorldPos(eventData.position);
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        transform.position = new Vector3(transform.position.x, liftHeight, transform.position.z);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 targetPosition = GetInputWorldPos(eventData.position) + offset;
        targetPosition.y = transform.position.y;

        Vector3 forceDirection = targetPosition - transform.position;
        rb.linearVelocity = forceDirection * dragForce;


    }
    void Update()
    {
        if (!isDragging && gameObject.transform.position.y != 0)
        {
            gameObject.transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        rb.isKinematic = true;
        rb.useGravity = true;
        StartCoroutine(SmoothSnap());
    }

    private Vector3 GetInputWorldPos(Vector2 pointerPosition)
    {
        Vector3 inputPos = new Vector3(pointerPosition.x, pointerPosition.y, zCoord);
        return cam.ScreenToWorldPoint(inputPos);
    }


    private IEnumerator SmoothSnap()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag(snapTag);
        Vector3 nearestPos = FindNearestTile(tiles);
        float duration = Vector3.Distance(transform.position, nearestPos) / snapSpeed;

        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector3 newPos = Vector3.Lerp(startPos, nearestPos, elapsed / duration);
            transform.position = newPos;
            yield return null;
        }

        // Final adjustment
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
        }

        transform.position = new Vector3(nearestPos.x, 0, nearestPos.z);
    }

    private Vector3 FindNearestTile(GameObject[] tiles)
    {
        Vector3 nearestPos = startPosition;
        float minDistance = Mathf.Infinity;

        foreach (GameObject tile in tiles)
        {
            Vector2 tilePos = new Vector2(tile.transform.position.x, tile.transform.position.z);
            Vector2 objPos = new Vector2(transform.position.x, transform.position.z);
            float distance = Vector2.Distance(tilePos, objPos);

            if (distance < minDistance && distance <= snapDistance)
            {
                minDistance = distance;
                nearestPos = tile.transform.position;
                nearestPos.y = transform.position.y;
            }
        }
        return nearestPos;
    }
}