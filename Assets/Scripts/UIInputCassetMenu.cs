using UnityEngine;
using UnityEngine.UI;

public class UIInputCassetMenu : MonoBehaviour
{
    public Button projectorSlot1;
    public TMPro.TMP_Text projectorText1;

    public Button projectorSlot2;
    public TMPro.TMP_Text projectorText2;

    public Button projectorSlot3;
    public TMPro.TMP_Text projectorText3;

    public Button playerSlot;
    public TMPro.TMP_Text playerText;

    public void UpdateUI(PlayerController controller, TripodController tripodController)
    {
        var db = SceneQuery.instance.dB;
        var type1 = tripodController.cassets[0].type;
        projectorSlot1.image.sprite = db.getSprite(type1);
        projectorSlot1.image.material = SceneQuery.instance.dB.getUIMaterial(type1);
        projectorText1.text = type1.ToString();

        var type2 = tripodController.cassets[1].type;
        projectorSlot2.image.sprite = db.getSprite(type2);
        projectorSlot2.image.material = SceneQuery.instance.dB.getUIMaterial(type2);
        projectorText2.text = type2.ToString();

        var type3 = tripodController.cassets[2].type;
        projectorSlot3.image.sprite = db.getSprite(type3);
        projectorSlot3.image.material = SceneQuery.instance.dB.getUIMaterial(type3);
        projectorText3.text = type3.ToString();

    }
}
