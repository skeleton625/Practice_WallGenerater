using UnityEngine;

public class Dragger : MonoBehaviour
{
    private readonly string CONNECTTAG = "Pole";
    private readonly string BUILDINGTAG = "Building";
    private readonly string MATERIALCOLOR = "_BaseColor";

    [Header("Camera Setting"), Space(10)]
    [SerializeField] private Camera MainCamera = null;

    [Header("Target Setting"), Space(10)]
    [SerializeField] private Transform WallTransform = null;
    [SerializeField] private Transform TargetModelTransform = null;
    [SerializeField] private Transform TargetBodyTransform = null;
    [SerializeField] private Transform TargetFirst = null;
    [SerializeField] private Transform TargetLast = null;
    [SerializeField] private Transform TargetCollider = null;
    [SerializeField] private float WallRadius = 0f;
    [SerializeField] private int WallLimitCount = 0;
    [SerializeField] private int[] LayerMasks = null;

    private int preLayerMask = 0;

    [Header(" Material Setting"), Space(10)]
    [SerializeField] private Material ModelMaterial = null;

    private bool isConflict = false;
    private bool isStarted = false;
    private bool isDragging = false;

    private int preWallRange = 0;
    private int nextWallRange = 0;
    private int prevWallRange = 0;

    private Vector3 startPosition = Vector3.zero;

    private void Start()
    {
        for (int i = 0; i < LayerMasks.Length; ++i)
            preLayerMask += LayerMasks[i];
        preLayerMask = ~preLayerMask;
    }

    private void Update()
    {
        if (isStarted)
        {
            MoveBlock();
            DragBlock();
        }
    }

    public void StartInstallBlock()
    {
        if (!isStarted)
        {
            isStarted = true;
            TargetModelTransform.gameObject.SetActive(true);
        }
    }

    private void MoveBlock()
    {
        if (!isDragging)
        {
            var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, preLayerMask))
            {
                Vector3 position;
                if (hit.transform.CompareTag(CONNECTTAG))
                {
                    position = hit.transform.position;
                    position.y = TargetModelTransform.localScale.y / 2;
                }
                else
                {
                    position = hit.point;
                    position.y = TargetModelTransform.localScale.y / 2;
                }

                preWallRange = 0;
                nextWallRange = 1;
                prevWallRange = 0;

                TargetBodyTransform.position = position;
            }
        }
    }

    private void DragBlock()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPosition = TargetBodyTransform.position;

            preWallRange = 1;
            nextWallRange = 2;
            prevWallRange = 1;
            TargetModelTransform.localScale = new Vector3(1, 2, WallRadius);
            TargetModelTransform.position = Vector3.Lerp(startPosition, startPosition + TargetBodyTransform.forward * WallRadius, .5f);
            ModelMaterial.SetColor(MATERIALCOLOR, Color.green);

            isDragging = true;
            isConflict = false;
        }
        if (Input.GetMouseButtonUp(0))
        {
            for (int i = 0; i < preWallRange; ++i)
            {
                var position = Vector3.Lerp(startPosition + TargetBodyTransform.forward * WallRadius * i, startPosition + TargetBodyTransform.forward * WallRadius * (i + 1), .5f);
                Instantiate(WallTransform, position, TargetBodyTransform.rotation);
            }

            var colliderFirst = Instantiate(TargetCollider, TargetFirst.position, TargetBodyTransform.rotation);
            var colliderLast = Instantiate(TargetCollider, TargetLast.position, TargetBodyTransform.rotation);
            colliderFirst.gameObject.SetActive(true);
            colliderLast.gameObject.SetActive(true);

            TargetModelTransform.gameObject.SetActive(false);
            TargetModelTransform.localScale = new Vector3(1, 2, 1);
            TargetModelTransform.localPosition = Vector3.zero;
            TargetBodyTransform.rotation = Quaternion.identity;
            TargetBodyTransform.position = Vector3.zero;
            isDragging = false;
            isStarted = false;
        }
        if (isDragging)
        {
            var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, preLayerMask))
            {
                if (hit.transform.CompareTag(CONNECTTAG))
                {
                    var position = hit.transform.position;
                    position.y = startPosition.y;
                    TargetBodyTransform.LookAt(position);

                    var range = Mathf.Clamp((position - startPosition).magnitude / WallRadius, 1, WallLimitCount);
                    if (nextWallRange < range && nextWallRange < WallLimitCount)
                    {
                        preWallRange++;
                        nextWallRange++;
                        prevWallRange++;
                        TargetModelTransform.localScale = new Vector3(1, 2, WallRadius * preWallRange);
                        TargetModelTransform.position = Vector3.Lerp(startPosition, startPosition + TargetBodyTransform.forward * WallRadius * preWallRange, .5f);
                    }
                    if (prevWallRange > range)
                    {
                        preWallRange--;
                        nextWallRange--;
                        prevWallRange--;
                        TargetModelTransform.localScale = new Vector3(1, 2, WallRadius * preWallRange);
                        TargetModelTransform.position = Vector3.Lerp(startPosition, startPosition + TargetBodyTransform.forward * WallRadius * preWallRange, .5f);
                    }
                }
                else
                {
                    var position = hit.point;
                    position.y = startPosition.y;
                    TargetBodyTransform.LookAt(position);

                    var range = Mathf.Clamp((position - startPosition).magnitude / WallRadius, 1, WallLimitCount);
                    if (nextWallRange < range && nextWallRange < WallLimitCount)
                    {
                        preWallRange++;
                        nextWallRange++;
                        prevWallRange++;
                        TargetModelTransform.localScale = new Vector3(1, 2, WallRadius * preWallRange);
                        TargetModelTransform.position = Vector3.Lerp(startPosition, startPosition + TargetBodyTransform.forward * WallRadius * preWallRange, .5f);
                    }
                    if (prevWallRange > range)
                    {
                        preWallRange--;
                        nextWallRange--;
                        prevWallRange--;
                        TargetModelTransform.localScale = new Vector3(1, 2, WallRadius * preWallRange);
                        TargetModelTransform.position = Vector3.Lerp(startPosition, startPosition + TargetBodyTransform.forward * WallRadius * preWallRange, .5f);
                    }
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(BUILDINGTAG) && !isConflict)
        {
            ModelMaterial.SetColor(MATERIALCOLOR, Color.red);
            isConflict = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(BUILDINGTAG))
        {
            ModelMaterial.SetColor(MATERIALCOLOR, Color.green);
            isConflict = false;
        }
    }
}
