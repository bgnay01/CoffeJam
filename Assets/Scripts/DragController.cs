using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;


[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PhysicsDragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Rigidbody rb;
    private Camera cam;
    private Vector3 offset;
    private float zCoord;
    private bool isDragging;

    [Header("Sürükleme Ayarları")]
    [SerializeField] private float liftHeight = 0.5f;
    [SerializeField] private float dragForce = 20f;
    [SerializeField] private LayerMask collisionLayer;

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
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        zCoord = cam.WorldToScreenPoint(transform.position).z;
        offset = transform.position - GetInputWorldPos();
        rb.isKinematic = false;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        transform.position += Vector3.up * liftHeight;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 targetPosition = GetInputWorldPos() + offset;
        targetPosition.y = transform.position.y;

        // Çarpışma kontrolü ile velocity hesaplama
        if (CanMoveTo(targetPosition))
        {
            Vector3 forceDirection = (targetPosition - transform.position);
            rb.linearVelocity = forceDirection * dragForce;
        }
        else
        {
            rb.linearVelocity *= 0.5f; // Engel varsa yavaşlat
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        rb.isKinematic = true;
        rb.useGravity = true;
        StartCoroutine(SmoothSnap());
    }

    private Vector3 GetInputWorldPos()
    {
        Vector3 inputPos = Input.touchCount > 0 ?
            Input.GetTouch(0).position :
            Input.mousePosition;

        inputPos.z = zCoord;
        return cam.ScreenToWorldPoint(inputPos);
    }

    private bool CanMoveTo(Vector3 targetPosition)
    {
        // 3D OverlapBox ile çarpışma kontrolü
        Collider[] colliders = Physics.OverlapBox(
            targetPosition,
            GetComponent<Collider>().bounds.extents * 0.9f,
            Quaternion.identity,
            collisionLayer
        );

        foreach (Collider col in colliders)
        {
            if (col.gameObject != gameObject && !col.CompareTag("Tile"))
                return false;
        }
        return true;
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
        rb.linearVelocity = Vector3.zero;
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