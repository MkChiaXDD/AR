using UnityEngine;
using UnityEngine.UI;

public class Base : MonoBehaviour
{
    private int maxHp;
    private int currentHp;

    public Image hpFill;

    public void Init(int maxHp)
    {
        this.maxHp = maxHp;
        currentHp = maxHp;
    }

    public void TakeDamage(int amount)
    {
        currentHp -= amount;
        if (currentHp < 0) currentHp = 0;

        UpdateUI();

        if (currentHp == 0)
        {
            OnBaseDestroyed();
        }
    }

    private void UpdateUI()
    {
        if (hpFill != null)
        {
            hpFill.fillAmount = (float)currentHp / maxHp;
        }
    }

    private void OnBaseDestroyed()
    {
        Debug.Log("Base destroyed!");
        // Here you can call GameManager.Instance.LoseGame(); etc.
    }
}
