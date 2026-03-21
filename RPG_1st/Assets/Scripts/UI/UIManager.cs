using UnityEngine;
using TMPro;       // 텍스트용
using UnityEngine.UI; // 슬라이더(바)용

public class UIManager : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats; // 씬에 있는 플레이어 오브젝트 연결

    [Header("UI Elements")]
    public TextMeshProUGUI expText;     // 우측 상단 경험치 텍스트
    public Slider expBar;

    private void Start()
    {
        if (playerStats != null)
        {
            playerStats.OnExpChanged += UpdateExpUI;
            playerStats.OnLevelUp += UpdateLevelUI;

            // 게임 시작 시 초기 화면 세팅
            UpdateLevelUI();
            UpdateExpUI();
        }
    }

    private void UpdateLevelUI()
    {
        // levelText.text = $"Lv. {playerStats.currentLevel}";
    }

    private void UpdateExpUI()
    {
        // 만렙 체크
        if (playerStats.currentLevel >= playerStats.expToNextLevel.Length)
        {
            expBar.value = 1f;
            expText.text = StringToSprite("100.00%");
            return;
        }

        // 2. 현재 경험치와 필요 경험치 가져오기
        int currentExp = playerStats.currentExp;
        int requiredExp = playerStats.expToNextLevel[playerStats.currentLevel];
        
        // 3. 비율 계산 (0.0f ~ 1.0f)
        float expPercent = (float)currentExp / requiredExp;

        // 4. 슬라이더 바 채우기 (주황색 바가 차오름)
        expBar.value = expPercent;

        string expString = $"{currentExp}/{requiredExp}"; // 괄호는 제외했습니다 (스프라이트가 없을까봐)
        string percentString = $"{(expPercent * 100f):F2}%";
        expText.text = $"{StringToSprite(expString)}  {StringToSprite(percentString)}";
    }

    private string StringToSprite(string input)
    {
        string result = "";
        foreach (char c in input)
        {
            if (c == ' ') 
            {
                result += " "; // 띄어쓰기는 그대로 둡니다.
            }
            else 
            {
                // 글자 하나하나를 스프라이트 태그로 감쌉니다. (예: "1" -> "<sprite name="1">")
                result += $"<sprite name=\"{c}\">";
            }
        }
        return result;
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 메모리 누수를 막기 위해 이벤트 연결 해제
        if (playerStats != null)
        {
            playerStats.OnExpChanged -= UpdateExpUI;
            playerStats.OnLevelUp -= UpdateLevelUI;
        }
    }

    private void Update()
    {
        
    }
}
