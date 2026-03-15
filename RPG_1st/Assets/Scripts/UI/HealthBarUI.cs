using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image fillImage;

    private void LateUpdate()
    {
        if (transform.parent != null)
        {
            Vector3 parentScale = transform.parent.localScale;
            // 캐릭터가 뒤돌아볼 때 체력바 뒤집히는 것 방지.
            // 부모 크기에 상관없이 항상 (1, 1, 1) 사이즈를 유지
            transform.localScale = new Vector3(
                Mathf.Sign(parentScale.x) / Mathf.Abs(parentScale.x),
                1f / Mathf.Abs(parentScale.y),
                1f / Mathf.Abs(parentScale.z));
        }
    }

    public void Setup(Color barColor)
    {
        if (fillImage != null)
        {
            fillImage.color = barColor;
        }
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = (float)currentHealth / maxHealth;
        }
    }
}
