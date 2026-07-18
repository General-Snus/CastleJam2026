using UnityEngine;
using UnityEngine.UI;

public class UIInventory : MonoBehaviour
{
    public Button UICassetButton;
    public TMPro.TMP_Text UICassetText;

    public void UpdateUI(PlayerController controller)
    {
        var heldType = controller.currentlyHeldCasset.type;
        UICassetButton.image.sprite = SceneQuery.instance.dB.getSprite(heldType);
        UICassetButton.image.material = SceneQuery.instance.dB.getUIMaterial(heldType);
        UICassetText.text = heldType.ToString();
    }
}
