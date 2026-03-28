using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements; 
using Button = UnityEngine.UIElements.Button;
using Slider = UnityEngine.UI.Slider;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats; // 씬에 있는 플레이어 오브젝트 연결
    public PlayerHealth playerHealth;

    [Header("UI Elements")]
    public TextMeshProUGUI txtExpAmount;
    public TextMeshProUGUI txtExpPercent;
    public Slider expBar;

    [Header("UI Toolkit: Stat Elements")]
    public UIDocument docStatMain;
    private VisualElement mainPanel;
    private Label txtStatPoints, txtSTR, txtDEX, txtINT, txtLUK; 
    private Label txtPlayerName, txtHP, txtMP, txtEXP, txtLevel;
    private VisualElement headerMain; // 창을 잡고 끌 손잡이
    private bool isDragging = false;
    private Vector2 dragStartMousePosition;
    private Vector2 dragStartPanelPosition;

    private string expPercentString = "";

    private void Start()
    {
        if (playerStats != null)
        {
            playerStats.OnExpChanged += UpdateExpUI;
            playerStats.OnExpChanged += UpdateStatText;

            playerStats.OnLevelUp += UpdateLevelUI;
            playerStats.OnLevelUp += UpdateStatText;

            playerStats.OnStatsChanged += UpdateStatText;

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateStatText; 
            }

            // 게임 시작 시 초기 화면 세팅
            UpdateLevelUI();
            UpdateExpUI();
            UpdateStatText();
        }

        InitializeUIToolkit();
    }

    private void InitializeUIToolkit()
    {
        if (docStatMain != null)
        {
            var rootMain = docStatMain.rootVisualElement;
            mainPanel = rootMain.Q<VisualElement>("MainPanel");

            // 버튼 찾기 및 기능 연결
            Button btnCloseMain = rootMain.Q<Button>("btnCloseMain");
            if (btnCloseMain != null) btnCloseMain.clicked += ToggleMainWindow;

            // Button btnDetails = rootMain.Q<Button>("btnDetails");
            // if (btnDetails != null) btnDetails.clicked += ToggleDetailWindow;

            // 스탯 증가 버튼들 연결
            Button btnSTR = rootMain.Q<Button>("btnStr");
            if (btnSTR != null) btnSTR.clicked += () => playerStats.IncreaseStat("STR");
            
            Button btnDEX = rootMain.Q<Button>("btnDex");
            if (btnDEX != null) btnDEX.clicked += () => playerStats.IncreaseStat("DEX");

            Button btnINT = rootMain.Q<Button>("btnInt");
            if (btnINT != null) btnINT.clicked += () => playerStats.IncreaseStat("INT");

            Button btnLUK = rootMain.Q<Button>("btnLuk");
            if (btnLUK != null) btnLUK.clicked += () => playerStats.IncreaseStat("LUK");

            // 글씨 바뀔 라벨들 찾기
            txtPlayerName = rootMain.Q<Label>("txtName");
            txtHP = rootMain.Q<Label>("txtHP");
            txtEXP = rootMain.Q<Label>("txtEXP");
            txtLevel = rootMain.Q<Label>("txtLevel");

            txtStatPoints = rootMain.Q<Label>("txtStatPoints");
            txtSTR = rootMain.Q<Label>("txtSTR");
            txtDEX = rootMain.Q<Label>("txtDEX");
            txtINT = rootMain.Q<Label>("txtINT");
            txtLUK = rootMain.Q<Label>("txtLUK");

            headerMain = rootMain.Q<VisualElement>("HeaderMain");
            if (headerMain != null)
            {
                // 마우스를 눌렀을 때, 움직일 때, 뗐을 때 실행할 함수들을 연결합니다.
                headerMain.RegisterCallback<PointerDownEvent>(OnDragStart);
                headerMain.RegisterCallback<PointerMoveEvent>(OnDragMove);
                headerMain.RegisterCallback<PointerUpEvent>(OnDragEnd);
                headerMain.RegisterCallback<PointerCaptureOutEvent>(OnDragEnd); // 마우스가 화면 밖으로 나갔을 때 대비
            }

            // 시작할 때 메인 창 끄기
            if (mainPanel != null) mainPanel.style.display = DisplayStyle.None;
        }

        // 시작 시 스탯창 숨기기 (CSS의 display: none 과 동일)
        if (mainPanel != null) 
        {
            mainPanel.style.display = DisplayStyle.None;
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
            txtExpAmount.text = StringToSprite("9999/9999");
            expPercentString = "100.00%";
            txtExpPercent.text = StringToSprite(expPercentString);
            return;
        }

        // 2. 현재 경험치와 필요 경험치 가져오기
        int currentExp = playerStats.currentExp;
        int requiredExp = playerStats.expToNextLevel[playerStats.currentLevel];
        
        // 3. 비율 계산 (0.0f ~ 1.0f)
        float expPercent = (float)currentExp / requiredExp;

        // 4. 슬라이더 바 채우기 (주황색 바가 차오름)
        expBar.value = expPercent;

        string expString = $"{currentExp}/{requiredExp}";
        expPercentString = $"{(expPercent * 100f):F2}%";
        txtExpAmount.text = StringToSprite(expString);
        txtExpPercent.text = StringToSprite(expPercentString);
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
        if (Input.GetKeyDown(KeyCode.S))
        {
            ToggleMainWindow();
        }
    }

    public void ToggleMainWindow()
    {
        if (mainPanel == null) return;

        bool isHidden = mainPanel.style.display == DisplayStyle.None;
        mainPanel.style.display = isHidden ? DisplayStyle.Flex : DisplayStyle.None;

        // 창이 켜질 때 최신 정보로 글씨 업데이트
        if (isHidden) UpdateStatText();
    }

    private void UpdateStatText()
    {
        if (mainPanel != null && mainPanel.style.display == DisplayStyle.Flex)
        {
            if (txtStatPoints != null) txtStatPoints.text = playerStats.statPoints.ToString();
            if (txtHP != null && playerHealth != null) txtHP.text = playerHealth.currentHealth.ToString() + "/" + playerHealth.maxHealth.ToString();
            if (txtEXP != null) txtEXP.text = expPercentString;
            if (txtLevel != null) txtLevel.text = playerStats.currentLevel.ToString();
            if (txtSTR != null) txtSTR.text = playerStats.STR.ToString();
            if (txtDEX != null) txtDEX.text = playerStats.DEX.ToString();
            if (txtINT != null) txtINT.text = playerStats.INT.ToString();
            if (txtLUK != null) txtLUK.text = playerStats.LUK.ToString();
        }
        else
        {
            Debug.LogWarning("⚠️ UpdateStatText가 불렸지만, 스탯창(mainPanel)이 꺼져있거나 null이라서 글씨 업데이트를 건너뛰었습니다.");
        }
    }

    private void OnDragStart(PointerDownEvent evt)
    {
        // 왼쪽 마우스 클릭(button == 0)일 때만 작동
        if (evt.button != 0 || mainPanel == null) return;
        if (evt.target is Button) return;

        isDragging = true;
        
        // 마우스가 너무 빨리 움직여서 헤더를 벗어나도 드래그가 끊기지 않도록 마우스를 꽉 붙잡습니다(Capture).
        headerMain.CapturePointer(evt.pointerId);

        // 드래그 시작 시점의 마우스 위치와 패널 위치를 기억해 둡니다.
        dragStartMousePosition = evt.position;
        dragStartPanelPosition = new Vector2(mainPanel.resolvedStyle.left, mainPanel.resolvedStyle.top);
    }

    private void OnDragMove(PointerMoveEvent evt)
    {
        // 드래그 중이 아니거나, 이 마우스가 우리가 붙잡은 마우스가 아니라면 무시
        if (!isDragging || !headerMain.HasPointerCapture(evt.pointerId)) return;

        // 마우스가 처음 누른 위치에서 얼마나 이동했는지 계산
        Vector2 pointerDelta = new Vector2(evt.position.x, evt.position.y) - dragStartMousePosition;

        // 패널의 원래 위치에 마우스 이동량(Delta)을 더해서 새로운 위치로 옮깁니다.
        mainPanel.style.left = dragStartPanelPosition.x + pointerDelta.x;
        mainPanel.style.top = dragStartPanelPosition.y + pointerDelta.y;
    }

    private void OnDragEnd(EventBase evt)
    {
        if (!isDragging) return;

        isDragging = false;
        
        // 마우스 붙잡은 것을 풀어줍니다.
        if (evt is PointerUpEvent upEvt)
        {
            headerMain.ReleasePointer(upEvt.pointerId);
        }
        else if (evt is PointerCaptureOutEvent outEvt)
        {
            headerMain.ReleasePointer(outEvt.pointerId);
        }

        // 💡 3. 드래그가 끝난 최종 위치를 유니티 PlayerPrefs에 영구 저장합니다!
        PlayerPrefs.SetFloat("StatWindow_X", mainPanel.resolvedStyle.left);
        PlayerPrefs.SetFloat("StatWindow_Y", mainPanel.resolvedStyle.top);
        PlayerPrefs.Save();
    }

    private void LoadWindowPosition()
    {
        // 저장해둔 X 좌표 기록이 있는지 확인합니다.
        if (PlayerPrefs.HasKey("StatWindow_X") && mainPanel != null)
        {
            float savedX = PlayerPrefs.GetFloat("StatWindow_X");
            float savedY = PlayerPrefs.GetFloat("StatWindow_Y");

            // 불러온 좌표를 패널에 적용합니다.
            mainPanel.style.left = savedX;
            mainPanel.style.top = savedY;
        }
    }
}
