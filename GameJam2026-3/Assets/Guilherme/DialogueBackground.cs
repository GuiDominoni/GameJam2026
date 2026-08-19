using UnityEngine;
using TMPro;

public class DialogueBackground : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private Transform square;
    [SerializeField] private float bgScale = 0.1f;

    [SerializeField] private float paddingX = 0.3f;
    [SerializeField] private float paddingY = 0.2f;

    private void LateUpdate()
    {
        if (text == null || square == null)
            return;

        text.ForceMeshUpdate();

        TMP_TextInfo textInfo = text.textInfo;

        int visibleCharacters = textInfo.characterCount;

        if (visibleCharacters == 0)
        {
            square.localScale = Vector3.zero;
            return;
        }

        Bounds bounds = new Bounds();

        bool hasVisibleCharacter = false;

        for (int i = 0; i < visibleCharacters; i++)
        {
            TMP_CharacterInfo character = textInfo.characterInfo[i];

            if (!character.isVisible)
                continue;

            Vector3 bottomLeft = character.bottomLeft;
            Vector3 topRight = character.topRight;

            if (!hasVisibleCharacter)
            {
                bounds = new Bounds(
                    (bottomLeft + topRight) / 2f,
                    topRight - bottomLeft
                );

                hasVisibleCharacter = true;
            }
            else
            {
                bounds.Encapsulate(bottomLeft);
                bounds.Encapsulate(topRight);
            }
        }

        if (!hasVisibleCharacter)
        {
            square.localScale = Vector3.zero;
            return;
        }

        square.localScale = new Vector3(
        (bounds.size.x + paddingX) * bgScale,
        (bounds.size.y + paddingY) * bgScale,
        1f
        );


        // Centraliza o Square no texto
        square.localPosition = bounds.center;
    }
}