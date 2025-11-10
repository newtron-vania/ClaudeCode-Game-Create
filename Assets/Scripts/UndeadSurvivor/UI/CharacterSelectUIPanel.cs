using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UndeadSurvivor
{
    /// <summary>
    /// 캐릭터 선택 UI 메인 패널
    /// 캐릭터 목록 표시, 선택, 게임 시작 처리
    /// </summary>
    public class CharacterSelectUIPanel : UIPanel
    {
        [Header("Panels")]
        [SerializeField] private CharacterStatInfoPanel _statInfoPanel;
        [SerializeField] private Transform _characterListContent;

        [Header("Buttons")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _cancelButton;

        [Header("Error Message")]
        [SerializeField] private TextMeshProUGUI _errorMessageText;
        [SerializeField] private float _errorMessageDuration = 3f;
        [SerializeField] private Color _errorMessageColor = Color.red;

        [Header("Prefab")]
        [SerializeField] private GameObject _characterSubItemPrefab;

        private UndeadSurvivorDataProvider _dataProvider;
        private CharacterData _selectedCharacterData;
        private List<CharacterSelectSubItem> _subItems = new List<CharacterSelectSubItem>();
        private CharacterSelectSubItem _currentSelectedSubItem;

        private Coroutine _errorMessageCoroutine;

        private void Awake()
        {
            // 버튼 이벤트 등록
            if (_startButton != null)
            {
                _startButton.onClick.AddListener(OnStartButtonClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
            }

            // 에러 메시지 초기 비활성화
            if (_errorMessageText != null)
            {
                _errorMessageText.gameObject.SetActive(false);
                _errorMessageText.color = _errorMessageColor;
            }
        }

        private void OnDestroy()
        {
            // 버튼 이벤트 해제
            if (_startButton != null)
            {
                _startButton.onClick.RemoveListener(OnStartButtonClicked);
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
            }

            // SubItem 이벤트 해제
            foreach (var subItem in _subItems)
            {
                if (subItem != null)
                {
                    subItem.OnCharacterClicked -= OnCharacterSelected;
                }
            }
        }

        /// <summary>
        /// UI 패널 초기화
        /// </summary>
        public void Initialize()
        {
            Debug.Log("[CharacterSelectUIPanel] Initialize - Start");

            // DataManager에서 DataProvider 가져오기
            _dataProvider = DataManager.Instance.GetProvider<UndeadSurvivorDataProvider>("UndeadSurvivor");

            if (_dataProvider == null)
            {
                Debug.LogError("[CharacterSelectUIPanel] Initialize - DataProvider not found!");
                return;
            }

            // 모든 캐릭터 로드
            LoadAllCharacters();

            // 스탯 패널 초기화
            if (_statInfoPanel != null)
            {
                _statInfoPanel.Clear();
            }

            Debug.Log("[CharacterSelectUIPanel] Initialize - Complete");
        }

        /// <summary>
        /// 모든 캐릭터 데이터 로드 및 SubItem 생성
        /// </summary>
        private void LoadAllCharacters()
        {
            if (_dataProvider == null)
            {
                Debug.LogError("[CharacterSelectUIPanel] LoadAllCharacters - DataProvider is null");
                return;
            }

            // 기존 SubItem 제거
            ClearSubItems();

            // 모든 캐릭터 데이터 가져오기
            List<CharacterData> characters = _dataProvider.GetAllCharacters();

            if (characters == null || characters.Count == 0)
            {
                Debug.LogWarning("[CharacterSelectUIPanel] LoadAllCharacters - No characters found");
                return;
            }

            Debug.Log($"[CharacterSelectUIPanel] LoadAllCharacters - Found {characters.Count} characters");

            // 각 캐릭터에 대해 SubItem 생성
            foreach (var characterData in characters)
            {
                CreateCharacterSubItem(characterData);
            }
        }

        /// <summary>
        /// 개별 캐릭터 SubItem 생성
        /// </summary>
        private void CreateCharacterSubItem(CharacterData characterData)
        {
            if (_characterSubItemPrefab == null || _characterListContent == null)
            {
                Debug.LogError("[CharacterSelectUIPanel] CreateCharacterSubItem - Prefab or Content is null");
                return;
            }

            // SubItem 인스턴스 생성
            GameObject subItemObj = Instantiate(_characterSubItemPrefab, _characterListContent);
            CharacterSelectSubItem subItem = subItemObj.GetComponent<CharacterSelectSubItem>();

            if (subItem != null)
            {
                // SubItem 초기화
                subItem.Initialize(characterData);

                // 클릭 이벤트 등록
                subItem.OnCharacterClicked += OnCharacterSelected;

                // 리스트에 추가
                _subItems.Add(subItem);

                Debug.Log($"[CharacterSelectUIPanel] CreateCharacterSubItem - Created SubItem for {characterData.Name}");
            }
            else
            {
                Debug.LogError("[CharacterSelectUIPanel] CreateCharacterSubItem - CharacterSelectSubItem component not found on prefab");
                Destroy(subItemObj);
            }
        }

        /// <summary>
        /// 기존 SubItem 모두 제거
        /// </summary>
        private void ClearSubItems()
        {
            foreach (var subItem in _subItems)
            {
                if (subItem != null)
                {
                    subItem.OnCharacterClicked -= OnCharacterSelected;
                    Destroy(subItem.gameObject);
                }
            }

            _subItems.Clear();
            _currentSelectedSubItem = null;
        }

        /// <summary>
        /// 캐릭터 선택 이벤트 핸들러
        /// </summary>
        /// <param name="characterId">선택된 캐릭터 ID</param>
        private void OnCharacterSelected(int characterId)
        {
            Debug.Log($"[CharacterSelectUIPanel] OnCharacterSelected - Character ID: {characterId}");

            // 이전 선택 해제
            if (_currentSelectedSubItem != null)
            {
                _currentSelectedSubItem.SetSelected(false);
            }

            // 새로 선택된 SubItem 찾기
            CharacterSelectSubItem selectedSubItem = _subItems.Find(item => item.CharacterId == characterId);

            if (selectedSubItem != null)
            {
                // 새로운 선택 설정
                selectedSubItem.SetSelected(true);
                _currentSelectedSubItem = selectedSubItem;

                // CharacterData 가져오기
                _selectedCharacterData = _dataProvider.GetCharacterData(characterId);

                if (_selectedCharacterData != null)
                {
                    // 스탯 패널 업데이트
                    if (_statInfoPanel != null)
                    {
                        _statInfoPanel.UpdateCharacterInfo(_selectedCharacterData);
                    }

                    Debug.Log($"[CharacterSelectUIPanel] OnCharacterSelected - Selected {_selectedCharacterData.Name}");
                }
                else
                {
                    Debug.LogError($"[CharacterSelectUIPanel] OnCharacterSelected - CharacterData not found for ID: {characterId}");
                }
            }
            else
            {
                Debug.LogError($"[CharacterSelectUIPanel] OnCharacterSelected - SubItem not found for ID: {characterId}");
            }
        }

        /// <summary>
        /// 시작 버튼 클릭 이벤트
        /// </summary>
        private void OnStartButtonClicked()
        {
            Debug.Log("[CharacterSelectUIPanel] OnStartButtonClicked");

            // 캐릭터 선택 여부 확인
            if (_selectedCharacterData == null)
            {
                ShowErrorMessage("캐릭터를 선택해주세요!");
                Debug.LogWarning("[CharacterSelectUIPanel] OnStartButtonClicked - No character selected");
                return;
            }

            // 선택된 캐릭터 정보를 MiniGameManager에 전달
            var miniGameManager = MiniGameManager.Instance;
            if (miniGameManager != null)
            {
                // CommonPlayerData에 선택된 캐릭터 ID 저장 (확장 필요)
                Debug.Log($"[CharacterSelectUIPanel] OnStartButtonClicked - Starting game with {_selectedCharacterData.Name}");
            }

            // GameScene으로 씬 전환
            CustomSceneManager.Instance.LoadScene("Undead Survivor");

            Debug.Log("[CharacterSelectUIPanel] OnStartButtonClicked - Loading GameScene");
        }

        /// <summary>
        /// 취소 버튼 클릭 이벤트
        /// </summary>
        private void OnCancelButtonClicked()
        {
            Debug.Log("[CharacterSelectUIPanel] OnCancelButtonClicked");

            // 선택 초기화
            _selectedCharacterData = null;

            if (_currentSelectedSubItem != null)
            {
                _currentSelectedSubItem.SetSelected(false);
                _currentSelectedSubItem = null;
            }

            if (_statInfoPanel != null)
            {
                _statInfoPanel.Clear();
            }

            // UndeadSurvivor 메인 씬으로 이동
            CustomSceneManager.Instance.LoadScene("Undead Survivor");

            Debug.Log("[CharacterSelectUIPanel] OnCancelButtonClicked - Returning to main scene");
        }

        /// <summary>
        /// 에러 메시지 표시
        /// </summary>
        /// <param name="message">표시할 메시지</param>
        private void ShowErrorMessage(string message)
        {
            if (_errorMessageText == null) return;

            // 기존 코루틴 중지
            if (_errorMessageCoroutine != null)
            {
                StopCoroutine(_errorMessageCoroutine);
            }

            // 새로운 코루틴 시작
            _errorMessageCoroutine = StartCoroutine(ShowErrorMessageCoroutine(message));
        }

        /// <summary>
        /// 에러 메시지 표시 코루틴 (3초 후 자동 숨김)
        /// </summary>
        private IEnumerator ShowErrorMessageCoroutine(string message)
        {
            _errorMessageText.text = message;
            _errorMessageText.gameObject.SetActive(true);

            Debug.Log($"[CharacterSelectUIPanel] ShowErrorMessage - {message}");

            yield return new WaitForSeconds(_errorMessageDuration);

            _errorMessageText.gameObject.SetActive(false);
            _errorMessageCoroutine = null;
        }

        /// <summary>
        /// 현재 선택된 캐릭터 데이터
        /// </summary>
        public CharacterData SelectedCharacterData => _selectedCharacterData;

        /// <summary>
        /// 선택된 캐릭터가 있는지 여부
        /// </summary>
        public bool HasSelectedCharacter => _selectedCharacterData != null;
    }
}
