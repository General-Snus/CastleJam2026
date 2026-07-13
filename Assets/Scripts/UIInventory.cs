using UnityEngine;
using UnityEngine.UI;

public class UIInventory : MonoBehaviour
{
    public GameObject UICassetPrefab;
    public GameObject cassetInventoryParent;

    public void UpdateUI(PlayerController controller)
    {
        for (int i = 0; i < cassetInventoryParent.transform.childCount; i++)
        {
            Destroy(cassetInventoryParent.transform.GetChild(i).gameObject);
        }

        foreach (var casset in controller.cassetInventory)
        {
            var prefab = Instantiate(UICassetPrefab, cassetInventoryParent.transform);
            prefab.GetComponentInChildren<TMPro.TMP_Text>().text = casset.type.ToString();
            prefab.GetComponent<Button>().onClick.AddListener(() =>
            {

            });
        }
    }
}
