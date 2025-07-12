using TMPro;

using UnityEngine;

public class InteractionIndicator : MonoBehaviour
{
    [SerializeField]
    float Spacing = 2f;

    [SerializeField]
    TMP_Text Instructions;

    public void Activate(InteractionItem item)
    {
        gameObject.SetActive(true);

        var offset = (item.Radius) + (Spacing);

        transform.position = item.transform.position + (Vector3.up * offset);

        Instructions.text = item.Instructions;
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}