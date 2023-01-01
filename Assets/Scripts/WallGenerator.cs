using UnityEngine;
using UnityEngine.UIElements;

public class WallGenerator : MonoBehaviour
{
    private readonly string CONNECTTAG = "Pole";
    private readonly string WALLTAG = "Wall";
    private readonly string MATERIALCOLOR = "_BaseColor";

    [Header("Camera Setting")]
    [SerializeField] private Camera MainCamera = null;
    [Space(10)]

    [Header("Wall Setting")]
    [SerializeField] private Transform WallModelTransform = null;
    [SerializeField] private Transform WallBodyTransform = null;
    [SerializeField] private Transform WallPrefabTransform = null;
    [SerializeField] private TagCollider WallModelCollider = null;
    [SerializeField] private float WallRadius = 0f;
    [SerializeField] private int[] GenerateLayerMask = null;
    [SerializeField] private int[] RemoveLayerMask = null;
    [Space(10)]

    private bool isDragging = false;
    private bool isConnectFirst = false;
    private bool isConnectLast = false;

    private int preWallRange = 0;
    private int nextWallRange = 0;
    private int prevWallRange = 0;
    private int prevConnectHashCode = 0;

    private int generateLayerMask = 0;
    private int removeLayerMask = 0;

    private Vector3 startPosition = Vector3.zero;

    private MeshRenderer selectedRenderer = null;

    [Header("Gate Setting")]
    [SerializeField] private Transform GateModelTransform = null;
    [SerializeField] private TagCollider GateModelCollider = null;
    [Space(10)]

    private bool isSnapped = false;
    private Transform selectedWall = null;

    [Header("Material Setting")]
    [SerializeField] private Material ModelMaterial = null;
    [SerializeField] private Color WallColor = default;
    [SerializeField] private Color SelectColor = default;
    [Space(10)]

    [Header("UI Setting")]
    [SerializeField] private WorkingTypeUI WorkingUI = null;
    [Space(10)]

    private byte generateType = 0;

    private void Start()
    {
        for (int i = 0; i < GenerateLayerMask.Length; ++i)
            generateLayerMask += GenerateLayerMask[i];
        generateLayerMask = ~generateLayerMask;

        for (int i = 0; i < RemoveLayerMask.Length; ++i)
            removeLayerMask += RemoveLayerMask[i];
        removeLayerMask = ~removeLayerMask;
    }

    private void Update()
    {
        switch (generateType)
        {
            case 1:
                MoveWallModel();
                DragWallModel();
                break;
            case 2:
                RemoveWall();
                break;
            case 3:
                MoveGateModel();
                break;
        }

        if (Input.GetMouseButtonDown(1))
        {
            switch (generateType)
            {
                case 1:
                    CancelGenerateWall();
                    break;
                case 2:
                    CancelRemoveWall();
                    break;
                case 3:
                    CancelGenerateGate();
                    break;
            }
        }
    }

    #region Start Functions
    public void StartInstallWall()
    {
        if (generateType.Equals(0))
        {
            generateType = 1;
            WorkingUI.SetGenerateType(1);
            ModelMaterial.SetColor(MATERIALCOLOR, Color.green);

            WallModelTransform.gameObject.SetActive(true);
            WallModelCollider.enabled = true;
        }
    }

    public void StartInstallGate()
    {
        if (generateType.Equals(0))
        {
            generateType = 2;
            WorkingUI.SetGenerateType(2);

            GateModelTransform.gameObject.SetActive(true);
            GateModelCollider.enabled = true;
        }
    }

    public void StartRemoveWall()
    {
        if (generateType.Equals(0))
        {
            generateType = 3;
            WorkingUI.SetGenerateType(3);
        }
    }
    #endregion

    #region Generate Wall Functions
    private void MoveWallModel()
    {
        if (isDragging) return;

        var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000, generateLayerMask))
        {
            Vector3 position;
            if (hit.transform.CompareTag(CONNECTTAG))
            {
                if (!isConnectFirst)
                {
                    isConnectFirst = true;
                    WallModelCollider.enabled = false;
                }

                position = hit.transform.position;

                if (Input.GetMouseButtonDown(0))
                    StartDrag();
            }
            else
            {
                if (isConnectFirst)
                {
                    isConnectFirst = false;
                    WallModelCollider.enabled = true;
                }

                position = hit.point;

                if (!WallModelCollider.IsConflict && Input.GetMouseButtonDown(0))
                    StartDrag();
            }
            position.y = WallModelTransform.localScale.y / 2;

            preWallRange = 0;
            nextWallRange = 1;
            prevWallRange = 0;

            WallBodyTransform.position = position;
        }

        void StartDrag()
        {
            startPosition = WallBodyTransform.position;

            preWallRange = 1;
            nextWallRange = 2;
            prevWallRange = 1;
            WallModelTransform.localScale = new Vector3(1, 2, WallRadius);
            WallModelTransform.position = Vector3.Lerp(startPosition, startPosition + WallBodyTransform.forward * WallRadius, .5f);
            WallModelTransform.gameObject.SetActive(false);

            isDragging = true;
            isConnectLast = false;
        }
    }

    private void DragWallModel()
    {
        if (!isDragging) return;

        var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000, generateLayerMask))
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (!WallModelCollider.IsConflict && WallModelTransform.gameObject.activeSelf)
                {
                    if (isConnectLast)
                    {
                        var preWallDist = 0f;
                        var maxWallDist = WallModelTransform.localScale.z - WallRadius;
                        Vector3 prevPosition;
                        Vector3 nextPosition;
                        preWallRange = 0;
                        nextWallRange = 1;

                        // MIDDILE
                        while (preWallDist < maxWallDist)
                        {
                            preWallDist += WallRadius;

                            prevPosition = startPosition + WallBodyTransform.forward * WallRadius * preWallRange;
                            nextPosition = startPosition + WallBodyTransform.forward * WallRadius * nextWallRange;
                            GenerateWall(prevPosition, nextPosition, WallRadius);
                            preWallRange++;
                            nextWallRange++;
                        }

                        // LAST
                        var lastDist = maxWallDist + WallRadius - preWallDist;
                        if (lastDist > .01f)
                        {
                            prevPosition = startPosition + WallBodyTransform.forward * WallRadius * preWallRange;
                            nextPosition = startPosition + WallBodyTransform.forward * WallModelTransform.localScale.z;
                            GenerateWall(prevPosition, nextPosition, lastDist);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < preWallRange; ++i)
                            GenerateWall(startPosition + WallBodyTransform.forward * WallRadius * i, startPosition + WallBodyTransform.forward * WallRadius * (i + 1), WallRadius);
                    }

                    WallModelTransform.gameObject.SetActive(false);
                }

                CancelGenerateWall();
                WorkingUI.SetGenerateType(0);
            }
            else
            {
                if (isConnectLast)
                {
                    if (prevConnectHashCode.Equals(hit.transform.GetHashCode()) || !hit.transform.CompareTag(CONNECTTAG))
                    {
                        isConnectLast = false;
                        if (preWallRange.Equals(0)) WallModelTransform.gameObject.SetActive(false);
                    }
                    return;
                }
                else if (hit.transform.CompareTag(CONNECTTAG))
                {

                    var position = hit.transform.position;
                    position.y = startPosition.y;
                    WallBodyTransform.LookAt(position);

                    var dist = (startPosition - position).magnitude;
                    SetWallModelTransform(dist, Vector3.Lerp(startPosition, position, .5f));

                    if (dist < WallRadius)
                    {
                        WallModelTransform.gameObject.SetActive(false);
                        WallModelCollider.enabled = false;
                    }
                    else
                    {
                        WallModelTransform.gameObject.SetActive(true);
                        WallModelCollider.enabled = true;
                    }

                    prevConnectHashCode = hit.transform.GetHashCode();
                    isConnectLast = true;
                }
                else
                {
                    var position = hit.point;
                    position.y = startPosition.y;
                    WallBodyTransform.LookAt(position);

                    var range = (position - startPosition).magnitude / WallRadius;
                    if (nextWallRange < range)
                    {
                        preWallRange++;
                        nextWallRange++;
                        prevWallRange++;
                        SetWallModelTransform(WallRadius * preWallRange, Vector3.Lerp(startPosition, startPosition + WallBodyTransform.forward * WallRadius * preWallRange, .5f));

                        if (preWallRange.Equals(1))
                        {
                            WallModelTransform.gameObject.SetActive(true);
                            WallModelCollider.enabled = true;
                        }
                    }
                    if (prevWallRange > range)
                    {
                        preWallRange--;
                        nextWallRange--;
                        prevWallRange--;
                        SetWallModelTransform(WallRadius * preWallRange, Vector3.Lerp(startPosition, startPosition + WallBodyTransform.forward * WallRadius * preWallRange, .5f));

                        if (preWallRange.Equals(0))
                        {
                            WallModelTransform.gameObject.SetActive(false);
                            WallModelCollider.enabled = false;
                        }
                    }
                }
            }
        }

        void SetWallModelTransform(float dist, Vector3 position)
        {
            WallModelTransform.localScale = new Vector3(1, 2, dist);
            WallModelTransform.position = position;
            WallModelCollider.transform.localScale = new Vector3(1, 2, dist - 2f);
            WallModelCollider.transform.position = position;
        }

        void GenerateWall(Vector3 prevPosition, Vector3 nextPosition, float dist)
        {
            var position = Vector3.Lerp(prevPosition, nextPosition, .5f);
            var wall = Instantiate(WallPrefabTransform, position, WallBodyTransform.rotation);
            wall.transform.GetChild(0).localScale = new Vector3(1, 2, dist);
        }
    }
    
    private void CancelGenerateWall()
    {
        WallModelTransform.gameObject.SetActive(false);
        WallModelTransform.localScale = new Vector3(1, 2, 1);
        WallModelTransform.localPosition = Vector3.zero;
        WallBodyTransform.rotation = Quaternion.identity;
        WallBodyTransform.position = Vector3.zero;

        isDragging = false;
        isConnectFirst = false;
        isConnectLast = false;
        prevConnectHashCode = 0;

        WallModelCollider.transform.localScale = new Vector3(.5f, 2, .5f);
        WallModelCollider.transform.localPosition = Vector3.zero;
        WallModelCollider.enabled = false;
        WorkingUI.SetGenerateType(0);
        generateType = 0;
    }
    #endregion

    #region Remove Wall Functions
    private void RemoveWall()
    {
        var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000, removeLayerMask))
        {
            if (selectedRenderer != null && !hit.transform.GetHashCode().Equals(selectedRenderer.transform.GetHashCode()))
            {
                selectedRenderer.material.SetColor(MATERIALCOLOR, WallColor);
                selectedRenderer = null;
            }
            if (selectedRenderer == null && hit.transform.CompareTag(WALLTAG))
            {
                selectedRenderer = hit.transform.GetChild(0).GetComponent<MeshRenderer>();
                selectedRenderer.material.SetColor(MATERIALCOLOR, SelectColor);
            }
        }
        else if (selectedRenderer != null)
        {
            selectedRenderer.material.SetColor(MATERIALCOLOR, WallColor);
            selectedRenderer = null;
        }

        if (Input.GetMouseButtonDown(0) && selectedRenderer != null)
        {
            DestroyImmediate(selectedRenderer.transform.parent.gameObject);
            selectedRenderer = null;
        }
    }

    private void CancelRemoveWall()
    {
        if (selectedRenderer != null)
        {
            selectedRenderer.material.SetColor(MATERIALCOLOR, WallColor);
            selectedRenderer = null;
        }

        WorkingUI.SetGenerateType(0);
        generateType = 0;
    }
    #endregion

    #region Generate Gate Functions
    private void MoveGateModel()
    {
        var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000, generateLayerMask))
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (selectedWall == null)
                {
                    Instantiate(GateModelTransform, GateModelTransform.position, GateModelTransform.rotation);
                }
                else
                {
                    DestroyImmediate(selectedWall);
                    Instantiate(GateModelTransform, GateModelTransform.position, GateModelTransform.rotation);
                }
                CancelGenerateGate();
            }
            else if(hit.transform.CompareTag(WALLTAG))
            {
                GateModelTransform.transform.position = hit.transform.position;
                GateModelTransform.transform.rotation = hit.transform.rotation;
            }
            else
            {
                GateModelTransform.position = hit.point;
                if (isSnapped)
                {
                    isSnapped = false;
                    GateModelTransform.rotation = Quaternion.identity;
                }
            }
        }
    }

    private void CancelGenerateGate()
    {
        isSnapped = false;
        selectedWall = null;

        GateModelCollider.enabled = false;
        GateModelTransform.gameObject.SetActive(false);
    }
    #endregion

    #region Conflict Wall Functions
    public void SetModelConflict(bool isConflict)
    {
        if (isConflict) ModelMaterial.SetColor(MATERIALCOLOR, Color.red);
        else ModelMaterial.SetColor(MATERIALCOLOR, Color.green);
    }
    #endregion
}
