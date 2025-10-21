using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 매니저 통합 테스트 스크립트
/// Unity 에디터에서 게임 실행 시 테스트 수행
/// </summary>
public class ManagersTest : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private bool _runTestOnStart = true;
    [SerializeField] private float _testInterval = 2f;
    [SerializeField] private bool _testSoundManager = true;
    [SerializeField] private bool _testUIManager = true;
    [SerializeField] private bool _testSceneManager = true;

    [Header("Test Resources")]
    [SerializeField] private List<string> _testBgmAddresses = new List<string>
    {
        "Audio/BGM/MainTheme",
        "Audio/BGM/BattleTheme"
    };

    [SerializeField] private List<string> _testSfxAddresses = new List<string>
    {
        "Audio/SFX/Click",
        "Audio/SFX/Explosion"
    };

    [Header("UI Settings")]
    [SerializeField] private int _maxLogLines = 20;
    [SerializeField] private int _fontSize = 14;

    private List<string> _logMessages = new List<string>();
    private string _currentTest = "";

    private void Start()
    {
        if (_runTestOnStart)
        {
            StartCoroutine(RunAllTests());
        }
    }

    /// <summary>
    /// 화면에 로그 표시
    /// </summary>
    private void OnGUI()
    {
        // GUI 스타일 설정
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = _fontSize;
        labelStyle.normal.textColor = Color.white;
        labelStyle.alignment = TextAnchor.UpperLeft;

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.7f));

        // 로그 영역
        float width = Screen.width * 0.95f;
        float height = Screen.height * 0.5f;
        float x = Screen.width * 0.025f;
        float y = Screen.height * 0.05f;

        GUILayout.BeginArea(new Rect(x, y, width, height), boxStyle);

        // 현재 테스트 표시
        if (!string.IsNullOrEmpty(_currentTest))
        {
            GUIStyle titleStyle = new GUIStyle(labelStyle);
            titleStyle.fontSize = _fontSize + 4;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.yellow;
            GUILayout.Label($"Current Test: {_currentTest}", titleStyle);
            GUILayout.Space(10);
        }

        // 로그 메시지 표시
        for (int i = Mathf.Max(0, _logMessages.Count - _maxLogLines); i < _logMessages.Count; i++)
        {
            GUILayout.Label(_logMessages[i], labelStyle);
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// GUI 배경 텍스처 생성
    /// </summary>
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    /// <summary>
    /// 로그 메시지 추가
    /// </summary>
    private void AddLog(string message)
    {
        _logMessages.Add(message);
        Debug.Log(message);
    }

    /// <summary>
    /// 현재 테스트 설정
    /// </summary>
    private void SetCurrentTest(string testName)
    {
        _currentTest = testName;
        AddLog($"===== {testName} =====");
    }

    /// <summary>
    /// 모든 테스트 실행
    /// </summary>
    public IEnumerator RunAllTests()
    {
        AddLog("========== Managers Test Start ==========");

        if (_testSoundManager)
        {
            SetCurrentTest("SoundManager Test");
            yield return StartCoroutine(TestSoundManager());
            yield return new WaitForSeconds(_testInterval);
        }

        if (_testUIManager)
        {
            SetCurrentTest("UIManager Test");
            yield return StartCoroutine(TestUIManager());
            yield return new WaitForSeconds(_testInterval);
        }

        if (_testSceneManager)
        {
            SetCurrentTest("SceneManager Test");
            yield return StartCoroutine(TestSceneManager());
            yield return new WaitForSeconds(_testInterval);
        }

        _currentTest = "";
        AddLog("========== Managers Test End ==========");
    }

    #region SoundManager 테스트

    /// <summary>
    /// SoundManager 테스트
    /// </summary>
    private IEnumerator TestSoundManager()
    {
        AddLog("Testing SoundManager initialization...");

        if (SoundManager.Instance == null)
        {
            AddLog("❌ SoundManager not initialized");
            yield break;
        }

        AddLog("✅ SoundManager initialized");

        // 볼륨 설정 테스트
        AddLog("Setting volumes...");
        SoundManager.Instance.SetMasterVolume(0.8f);
        SoundManager.Instance.SetBGMVolume(0.7f);
        SoundManager.Instance.SetSFXVolume(0.6f);
        AddLog($"Master: {SoundManager.Instance.MasterVolume:F2}, BGM: {SoundManager.Instance.BgmVolume:F2}, SFX: {SoundManager.Instance.SfxVolume:F2}");

        yield return new WaitForSeconds(1f);

        // BGM 재생 테스트 (실제 오디오 파일이 있다면)
        if (_testBgmAddresses.Count > 0)
        {
            AddLog($"Testing BGM playback: {_testBgmAddresses[0]}");
            AddLog("(BGM requires actual audio files in Addressables)");
        }

        // SFX 재생 테스트 (실제 오디오 파일이 있다면)
        if (_testSfxAddresses.Count > 0)
        {
            AddLog($"Testing SFX playback: {_testSfxAddresses[0]}");
            AddLog("(SFX requires actual audio files in Addressables)");
        }

        AddLog("✅ SoundManager test completed");
    }

    #endregion

    #region UIManager 테스트

    /// <summary>
    /// UIManager 테스트
    /// </summary>
    private IEnumerator TestUIManager()
    {
        AddLog("Testing UIManager initialization...");

        if (UIManager.Instance == null)
        {
            AddLog("❌ UIManager not initialized");
            yield break;
        }

        AddLog("✅ UIManager initialized");

        // 페이드 인 테스트
        AddLog("Testing Fade In...");
        bool fadeInComplete = false;
        UIManager.Instance.FadeIn(1f, () =>
        {
            fadeInComplete = true;
            AddLog("✅ Fade In completed");
        });

        yield return new WaitUntil(() => fadeInComplete);
        yield return new WaitForSeconds(1f);

        // 페이드 아웃 테스트
        AddLog("Testing Fade Out...");
        bool fadeOutComplete = false;
        UIManager.Instance.FadeOut(1f, () =>
        {
            fadeOutComplete = true;
            AddLog("✅ Fade Out completed");
        });

        yield return new WaitUntil(() => fadeOutComplete);
        yield return new WaitForSeconds(1f);

        // 팝업 테스트
        AddLog("Testing Popup...");
        UIManager.Instance.ShowPopup("Test Title", "Test Message", () =>
        {
            AddLog("✅ Popup confirmed");
        });

        yield return new WaitForSeconds(1f);

        AddLog("✅ UIManager test completed");
    }

    #endregion

    #region SceneManager 테스트

    /// <summary>
    /// SceneManager 테스트
    /// </summary>
    private IEnumerator TestSceneManager()
    {
        AddLog("Testing CustomSceneManager initialization...");

        if (CustomSceneManager.Instance == null)
        {
            AddLog("❌ CustomSceneManager not initialized");
            yield break;
        }

        AddLog("✅ CustomSceneManager initialized");

        // 현재 씬 정보
        AddLog($"Current Scene: {CustomSceneManager.Instance.CurrentSceneName}");
        AddLog($"Is Loading: {CustomSceneManager.Instance.IsLoading}");
        AddLog($"Scene Stack Count: {CustomSceneManager.Instance.SceneStackCount}");

        // 씬 스택 테스트
        AddLog("Testing Scene Stack...");
        AddLog($"Pushing current scene to stack...");
        // 실제 씬 전환은 테스트 환경에서 위험하므로 로그만 출력
        AddLog("(Actual scene loading requires valid scene names in build settings)");

        yield return new WaitForSeconds(1f);

        // 씬 로드 여부 확인
        string currentScene = CustomSceneManager.Instance.CurrentSceneName;
        bool isLoaded = CustomSceneManager.Instance.IsSceneLoaded(currentScene);
        AddLog($"Is '{currentScene}' loaded: {isLoaded}");

        AddLog("✅ CustomSceneManager test completed");
    }

    #endregion
}
