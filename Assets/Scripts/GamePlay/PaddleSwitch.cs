using UnityEngine;

public class PaddleSwitch : MonoBehaviour
{
    [SerializeField]
    GameObject
        basePaddle,
        bronzePaddle,
        silverPaddle,
        goldPaddle;

    public void BasePaddle()
    {
        basePaddle.SetActive(true);
        bronzePaddle.SetActive(false);
        silverPaddle.SetActive(false);
        goldPaddle.SetActive(false);
    }

    public void BronzePaddle()
    {
        basePaddle.SetActive(false);
        bronzePaddle.SetActive(true);
        silverPaddle.SetActive(false);
        goldPaddle.SetActive(false);
    }

    public void SilverPaddle()
    {
        basePaddle.SetActive(false);
        bronzePaddle.SetActive(false);
        silverPaddle.SetActive(true);
        goldPaddle.SetActive(false);
    }

    public void GoldPaddle()
    {
        basePaddle.SetActive(false);
        bronzePaddle.SetActive(false);
        silverPaddle.SetActive(false);
        goldPaddle.SetActive(true);
    }
}
