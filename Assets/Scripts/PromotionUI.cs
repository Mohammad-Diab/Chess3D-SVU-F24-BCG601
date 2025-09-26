using Chess;
using UnityEngine;
using UnityEngine.UI;

public class PromotionUI : MonoBehaviour
{
    public GameObject board;
    public GameObject panel;
    public Button queenBtn, rookBtn, bishopBtn, knightBtn;

    private System.Action<PromotionType> onSelect;

    public void Show(PieceColor color, System.Action<PromotionType> onSelectCallback)
    {
        onSelect = onSelectCallback;
        board.SetActive(false);
        panel.SetActive(true);

        queenBtn.onClick.RemoveAllListeners();
        rookBtn.onClick.RemoveAllListeners();
        bishopBtn.onClick.RemoveAllListeners();
        knightBtn.onClick.RemoveAllListeners();

        queenBtn.onClick.AddListener(() => Select(PromotionType.ToQueen));
        rookBtn.onClick.AddListener(() => Select(PromotionType.ToRook));
        bishopBtn.onClick.AddListener(() => Select(PromotionType.ToBishop));
        knightBtn.onClick.AddListener(() => Select(PromotionType.ToKnight));
    }

    private void Select(PromotionType type)
    {
        panel.SetActive(false);
        board.SetActive(true);
        onSelect?.Invoke(type);
        onSelect = null;
    }
}