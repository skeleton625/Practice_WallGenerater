using System.Collections.Generic;
using UnityEngine;

public class WallGenerator : MonoBehaviour
{
    private readonly string CONNECTTAG = "Pole";
    private readonly string BUILDINGTAG = "Building";
    private readonly string MATERIALCOLOR = "_BaseColor";

    [Header("Camera Setting"), Space(10)]
    [SerializeField] private Camera MainCamera = null;

    [Header("Target Setting"), Space(10)]
    [SerializeField] private Transform TargetModelTransform = null;
    [SerializeField] private Transform TargetBodyTransform = null;
    [SerializeField] private Transform WallPrefabTransform = null;
    [SerializeField] private Transform TargetCollider = null;
    [SerializeField] private TagCollider ModelCollider = null;
    [SerializeField] private float WallRadius = 0f;
    [SerializeField] private int[] GenerateLayerMask = null;
    [SerializeField] private int[] RemoveLayerMask = null;

    private int generateLayerMask = 0;
    private int removeLayerMask = 0;

    [Header("Material Setting"), Space(10)]
    [SerializeField] private Material ModelMaterial = null;
    [SerializeField] private Color WallColor = default;
    [SerializeField] private Color SelectColor = default;

    [Header("UI Setting"), Space(10)]
    [SerializeField] private WorkingTypeUI WorkingUI = null;

    private byte generateType = 0;

    private bool isDragging = false;
    private bool isConnectFirst = false;
    private bool isConnectLast = false;

    private int preWallRange = 0;
    private int nextWallRange = 0;
    private int prevWallRange = 0;

    private Vector3 startPosition = Vector3.zero;

    private MeshRenderer selectedRenderer = null;

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
                MoveBlock();
                DragBlock();
                break;
            case 2:
                RemoveWall();
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
            }
        }
    }

    #region Wall Functions
    public void StartInstallWall()
    {
        if (generateType.Equals(0))
        {
            generateType = 1;
            WorkingUI.SetGenerateType(1);
            ModelMaterial.SetColor(MATERIALCOLOR, Color.green);
            
            TargetModelTransform.gameObject.SetActive(true);
            ModelCollider.enabled = true;
        }
    }

    public void StartRemoveWall()
    {
        if (generateType.Equals(0))
        {
            generateType = 2;
            WorkingUI.SetGenerateType(2);
        }
    }
    #endregion

    #region Generate Wall Functions
    private void MoveBlock()
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
                    ModelCollider.enabled = false;
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
                    ModelCollider.enabled = true;
                }

                position = hit.point;

                if (!ModelCollider.IsConflict && Input.GetMouseButtonDown(0))
                    StartDrag();
            }
            position.y = TargetModelTransform.localScale.y / 2;

            preWallRange = 0;
            nextWallRange = 1;
            prevWallRange = 0;

            TargetBodyTransform.position = position;
        }

        void StartDrag()
        {
            startPosition = TargetBodyTransform.position;

            preWallRange = 1;
            nextWallRange = 2;
            prevWallRange = 1;
            TargetModelTransform.localScale = new Vector3(1, 2, WallRadius);
            TargetModelTransform.position = Vector3.Lerp(startPosition, startPosition + TargetBodyTransform.forward * WallRadius, .5f);
            TargetModelTransform.gameObject.SetActive(false);

            isDragging = true;
            isConnectLast = false;
        }
    }

    private void DragBlock()
    {
        if (!isDragging) return;

        var ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000, generateLayerMask))
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (!ModelCollider.IsConflict && TargetModelTransform.gameObject.activeSelf)
                {
                    Transform collider;
                    if (isConnectLast)
                    {
                        var preWallDist = WallRadius;
                        var maxWallDist = TargetModelTransform.localScale.z - WallRadius;
                        Vector3 prevPosition;
                        Vector3 lastPosition;
                        Vector3 position;

                        preWallRange = 0;
                        nextWallRange = 1;

                        // FIRST
                        prevPosition = startPosition + TargetBodyTransform.forward * WallRadius * preWallRange;
                        lastPosition = startPosition + TargetBodyTransform.forward * WallRadius * nextWallRange;
                        position = Vector3.Lerp(prevPosition, lastPosition, .5f);

                        Instantiate(WallPrefabTransform, position, TargetBodyTransform.rotation);
                        if (!isConnectFirst)
                        {
                            collider = Instantiate(TargetCollider, prevPosition, TargetBodyTransform.rotation);
                            collider.gameObject.SetActive(true);
                        }
                        preWallRange++;
                        nextWallRange++;

                        // MIDDILE
                        while (preWallDist < maxWallDist)
                        {
                            preWallDist += WallRadius;

                            prevPosition = startPosition + TargetBodyTransform.forward * WallRadius * preWallRange;
                            lastPosition = startPosition + TargetBodyTransform.forward * WallRadius * nextWallRange;
                            position = Vector3.Lerp(prevPosition, lastPosition, .5f);
                            Instantiate(WallPrefabTransform, position, TargetBodyTransform.rotation);
                            preWallRange++;
                            nextWallRange++;

                            collider = Instantiate(TargetCollider, prevPosition, TargetBodyTransform.rotation);
                            collider.gameObject.SetActive(true);
                        }

                        // LAST
                        prevPosition = startPosition + TargetBodyTransform.forward * WallRadius * preWallRange;
                        lastPosition = startPosition + TargetBodyTransform.forward * TargetModelTransform.localScale.z;
                        var lastDist = maxWallDist + WallRadius - preWallDist;
                        if (lastDist > .01f)
                        {
                            position = Vector3.Lerp(prevPosition, lastPosition, .5f);

                            var lastWall = Instantiate(WallPrefabTransform, position, TargetBodyTransform.rotation);
                            lastWall.localScale = new Vector3(1, 2, lastDist);
                        }
                        if (!isConnectLast)
                        {
                            collider = Instantiate(TargetCollider, lastPosition, TargetBodyTransform.rotation);
                            collider.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < preWallRange; ++i)
                        {
                            var prevPosition = startPosition + TargetBodyTransform.forward * WallRadius * i;
                            var position = Vector3.Lerp(prevPosition, startPosition + TargetBodyTransform.forward * WallRadius * (i + 1), .5f);
                            Instantiate(WallPrefabTransform, position, TargetBodyTransform.rotation);

                            collider = Instantiate(TargetCollider, prevPosition, TargetBodyTransform.rotation);
                            collider.gameObject.SetActive(true);
                        }
                        collider = Instantiate(TargetCollider, startPosition + TargetBodyTransform.forward * WallRadius * preWallRange, TargetBodyTransform.rotation);
                        collider.gameObject.SetActive(true);
                    }

                    TargetModelTransform.gameObject.SetActive(false);
                }

                CancelGenerateWall();
                WorkingUI.SetGenerateType(0);
            }
            else
            {
                if (isConnectLast)
                {
                    if (!hit.transform.CompareTag(CONNECTTAG))
                    {
                        isConnectLast = false;
                        if (preWallRange.Equals(0)) TargetModelTransform.gameObject.SetActive(false);
                    }
                    return;
                }
                else if (hit.transform.CompareTag(CONNECTTAG))
                {
                    var position = hit.transform.position;
                    position.y = startPosition.y;
                    TargetBodyTransform.LookAt(position);

                    var dist = (startPosition - position).magnitude;

                    position = Vector3.Lerp(startPosition, position, .5f);
                    TargetModelTransform.localScale = new Vector3(1, 2, dist);
                    TargetModelTransform.position = position;
                    ModelCollider.transform.localScale = new Vector3(1, 2, dist - 2f);
                    ModelCollider.transform.position = position;

                    if (dist < WallRadius)
                    {
                        TargetModelTransform.gameObject.SetActive(false);
                        ModelCollider.enabled = false;
                    }
                    else
                    {
                        TargetModelTransform.gameObject.SetActive(true);
                        ModelCollider.enabled = true;
                    }
                    isConnectLast = true;
                }
                else
                {
                    var position = hit.point;
                    position.y = startPosition.y;
                    TargetBodyTransform.LookAt(position);

                    var range = (position - startPosition).magnitude / WallRadius;
                    if (nextWallRange < range)
                    {
                        preWallRange++;
                        nextWallRange++;
                        prevWallRange++;

                        position = Vector3.Lerp(startPosition, startPosition + TargetBodyTransform.forward * WallRadius * preWallRange, .5f);
                        TargetModelTransform.localScale = new Vector3(1, 2, WallRadius * preWallRange);
                        TargetModelTransform.position = position;
                        ModelCollider.transform.localScale = new Vector3(1, 2, WallRadius * preWallRange - 2f);
                        ModelCollider.transform.position = position;

                        if (preWallRange.Equals(1))
                        {
                            TargetModelTransform.gameObject.SetActive(true);
                            ModelCollider.enabled = true;
                        }
                    }
                    if (prevWallRange > range)
                    {
                        preWallRange--;
                        nextWallRange--;
                        prevWallRange--;
                        position = Vector3.Lerp(startPosition, startPosition + TargetBodyTransform.forward * WallRadius * preWallRange, .5f);
                        TargetModelTransform.localScale = new Vector3(1, 2, WallRadius * preWallRange);
                        TargetModelTransform.position = position;
                        ModelCollider.transform.localScale = new Vector3(1, 2, WallRadius * preWallRange - 2f);
                        ModelCollider.transform.position = position;

                        if (preWallRange.Equals(0))
                        {
                            TargetModelTransform.gameObject.SetActive(false);
                            ModelCollider.enabled = false;
                        }
                    }
                }
            }
        }
    }
    
    private void CancelGenerateWall()
    {
        TargetModelTransform.gameObject.SetActive(false);
        TargetModelTransform.localScale = new Vector3(1, 2, 1);
        TargetModelTransform.localPosition = Vector3.zero;
        TargetBodyTransform.rotation = Quaternion.identity;
        TargetBodyTransform.position = Vector3.zero;

        isDragging = false;
        isConnectFirst = false;
        isConnectLast = false;

        ModelCollider.transform.localScale = new Vector3(.5f, 2, .5f);
        ModelCollider.transform.localPosition = Vector3.zero;
        ModelCollider.enabled = false;
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
            if (selectedRenderer == null && hit.transform.CompareTag(BUILDINGTAG))
            {
                selectedRenderer = hit.transform.GetComponent<MeshRenderer>();
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
            DestroyImmediate(selectedRenderer.gameObject);
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

    #region Conflict Wall Functions
    public void SetModelConflict(bool isConflict)
    {
        if (isConflict) ModelMaterial.SetColor(MATERIALCOLOR, Color.red);
        else ModelMaterial.SetColor(MATERIALCOLOR, Color.green);
    }
    #endregion
}
