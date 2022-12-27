using UnityEngine;

public class Dragger : MonoBehaviour
{
    private readonly string CONNECTTAG = "Pole";

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

    [Header("Test Setting"), Space(10)]
    [SerializeField] private int TestCount = 0;
    [SerializeField] private Transform TestTransform = null;

    private bool isStarted = false;
    private bool isDragging = false;

    private int preWallRange = 0;
    private int nextWallRange = 0;
    private int prevWallRange = 0;

    private Transform[] TestTransformArray = null;

    private void Start()
    {
        TestTransformArray = new Transform[TestCount];
        for (int i = 0; i < TestCount; ++i)
        {
            TestTransformArray[i] = Instantiate(TestTransform, Vector3.forward * WallRadius * i, Quaternion.identity);
            TestTransformArray[i].SetParent(TestTransform.parent);
        }

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
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, -1))
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
            preWallRange = 1;
            nextWallRange = 2;
            prevWallRange = 1;
            TargetModelTransform.localScale = new Vector3(1, 2, WallRadius);
            TargetModelTransform.position = Vector3.Lerp(TestTransformArray[0].position, TestTransformArray[1].position, .5f);

            isDragging = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            for (int i = 0; i < preWallRange; ++i)
            {
                var position = Vector3.Lerp(TestTransformArray[i].position, TestTransformArray[i + 1].position, .5f);
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
            if (Physics.Raycast(ray, out RaycastHit hit, 1000, -1))
            {
                Vector3 position;
                if (hit.transform.CompareTag(CONNECTTAG))
                {
                    position = hit.transform.position;
                }
                else
                {
                    position = hit.point;
                }

                position.y = TestTransformArray[0].position.y;
                TargetBodyTransform.LookAt(position);

                var range = Mathf.Clamp((position - TestTransformArray[0].position).magnitude / WallRadius, 1, TestCount);
                if (nextWallRange < range && nextWallRange < TestCount)
                {
                    preWallRange++;
                    nextWallRange++;
                    prevWallRange++;
                    TargetModelTransform.localScale = new Vector3(1, 2, WallRadius * preWallRange);
                    TargetModelTransform.position = Vector3.Lerp(TestTransformArray[0].position, TestTransformArray[preWallRange].position, .5f);
                }
                if (prevWallRange > range)
                {
                    preWallRange--;
                    nextWallRange--;
                    prevWallRange--;
                    TargetModelTransform.localScale = new Vector3(1, 2, WallRadius * preWallRange);
                    TargetModelTransform.position = Vector3.Lerp(TestTransformArray[0].position, TestTransformArray[preWallRange].position, .5f);
                }
            }
        }
    }
}
