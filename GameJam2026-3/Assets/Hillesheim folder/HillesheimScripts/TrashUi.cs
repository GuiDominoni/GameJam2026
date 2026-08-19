using UnityEngine;

public enum TrashUiType
{
    plasticUi,
    paperUi,
    glassUi,
    metalUi
}
public class TrashUi : MonoBehaviour
{
    private TrashUiType uiType;
    [SerializeField] private SpriteRenderer[] _checkPlasticSprites;
    private int _plasticsUichecks = 0;
    [SerializeField] private SpriteRenderer[] _checkPaperSprites;
    private int _papersUichecks = 0;
    [SerializeField] private SpriteRenderer[] _checkGlassSprites;
    private int _glassesUichecks = 0;
    [SerializeField] private SpriteRenderer[] _checkMetalSprites;
    private int _metalsUichecks = 0;

    private Sprite _greenCheck;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _greenCheck= TrashGameController.Instance.GreenCheck;
    }
    public void TrashUIUpdate(TrashType trashType)
    {
        if (trashType.Equals(TrashType.plastic) && _checkPlasticSprites != null)
        {
            if(_checkPlasticSprites.Length == 1)
            {
                _checkPlasticSprites[0].sprite = _greenCheck;
            }
            else
            {
                _checkPlasticSprites[_plasticsUichecks].sprite = _greenCheck;
                _plasticsUichecks++;
            }
        }
        else if (trashType.Equals(TrashType.paper) && _checkPaperSprites != null)
        {
            if (_checkPaperSprites.Length == 1)
            {
                _checkPaperSprites[0].sprite = _greenCheck;
            }
            else
            {
                _checkPaperSprites[_papersUichecks].sprite = _greenCheck;
                _papersUichecks++;
            }
        }
        else if (trashType.Equals(TrashType.glass) && _checkGlassSprites != null)
        {
            if (_checkGlassSprites.Length == 1)
            {
                _checkGlassSprites[0].sprite = _greenCheck;
            }
            else
            {
                _checkGlassSprites[_glassesUichecks].sprite = _greenCheck;
                _glassesUichecks++;
            }
        }
        else if (trashType.Equals(TrashType.metal) && _checkMetalSprites != null)
        {
            if (_checkMetalSprites.Length == 1)
            {
                _checkMetalSprites[0].sprite = _greenCheck;
            }
            else
            {
                _checkMetalSprites[_metalsUichecks].sprite = _greenCheck;
                _metalsUichecks++;
            }
        }
    }
}
