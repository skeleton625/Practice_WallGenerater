using UnityEngine;
using TMPro;

public class GenerateTypeUI : MonoBehaviour
{
    [Header("Type UI Setting"), Space(10)]
    [SerializeField] private TextMeshProUGUI TypeUI = null;

    public void SetGenerateType(byte type)
    {
        switch (type)
        {
            case 0: TypeUI.text = "None"; break;
            case 1: TypeUI.text = "Generate Wall"; break;
            case 2: TypeUI.text = "Remove Wall"; break;
        }
    }
}
