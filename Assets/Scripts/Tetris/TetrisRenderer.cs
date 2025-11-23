using UnityEngine;

/// <summary>
/// 테트리스 시각화 렌더러
/// TetrisData를 읽어서 Sprite 기반으로 화면에 렌더링합니다.
/// </summary>
public class TetrisRenderer : MonoBehaviour
{
    [Header("Rendering Settings")]
    [SerializeField] private float _cellSize = 0.5f;
    [SerializeField] private float _previewCellSize = 0.7f;
    [SerializeField] private float _yInterval = 3.0f;
    [SerializeField] private Transform _boardOffset;
    [SerializeField] private Transform _nextPieceOffset;

    [Header("Block Sprites")]
    [SerializeField] private Sprite _blockSprite;   // 공통 Sprite (정사각형)

    [Header("Block Colors")]
    [SerializeField] private Color _emptyColor = new Color(0.1f, 0.1f, 0.1f, 1f);      // 어두운 회색
    [SerializeField] private Color _cyanColor = new Color(0f, 1f, 1f, 1f);             // I 블록 - 청록색
    [SerializeField] private Color _yellowColor = new Color(1f, 1f, 0f, 1f);           // O 블록 - 노랑
    [SerializeField] private Color _purpleColor = new Color(0.5f, 0f, 1f, 1f);         // T 블록 - 보라
    [SerializeField] private Color _greenColor = new Color(0f, 1f, 0f, 1f);            // S 블록 - 초록g
    [SerializeField] private Color _redColor = new Color(1f, 0f, 0f, 1f);              // Z 블록 - 빨강
    [SerializeField] private Color _blueColor = new Color(0f, 0f, 1f, 1f);             // J 블록 - 파랑
    [SerializeField] private Color _orangeColor = new Color(1f, 0.5f, 0f, 1f);         // L 블록 - 주황

    [Header("Grid Settings")]
    [SerializeField] private bool _showGrid = true;
    [SerializeField] private Color _gridColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    private GameObject[,] _boardCells;
    private GameObject[] _currentPieceBlocks;
    private GameObject[][] _nextPieceBlocks; // 3개의 Next Piece (각 4블록)
    private GameObject _gridContainer;

    private TetrisData _tetrisData;

    private void Start()
    {
        InitializeBoard();
        InitializeCurrentPiece();
        InitializeNextPiecePreview();

        if (_showGrid)
        {
            DrawGrid();
        }

        Debug.Log("[INFO] TetrisRenderer::Start - Tetris renderer initialized");
    }

    private void Update()
    {
        // 테트리스 데이터 가져오기
        if (MiniGameManager.Instance == null)
        {
            Debug.LogWarning("[WARNING] TetrisRenderer::Update - MiniGameManager.Instance is null");
            return;
        }

        _tetrisData = MiniGameManager.Instance.GetCurrentGameData<TetrisData>();

        if (_tetrisData == null)
        {
            return;
        }

        // 시각화 업데이트
        RenderBoard();
        RenderCurrentPiece();
        RenderNextPieces();
    }

    /// <summary>
    /// 보드 그리드 초기화 (10x20)
    /// </summary>
    private void InitializeBoard()
    {
        try
        {
            _boardCells = new GameObject[TetrisData.BOARD_HEIGHT, TetrisData.BOARD_WIDTH];

            for (int y = 0; y < TetrisData.BOARD_HEIGHT; y++)
            {
                for (int x = 0; x < TetrisData.BOARD_WIDTH; x++)
                {
                    GameObject cell = new GameObject($"Cell_{x}_{y}");
                    if (cell == null)
                    {
                        Debug.LogError($"[ERROR] TetrisRenderer::InitializeBoard - Failed to create cell at [{y}, {x}]");
                        continue;
                    }

                    cell.transform.parent = transform;

                    Vector3 position = new Vector3(
                        _boardOffset.position.x + x * _cellSize,
                        _boardOffset.position.y + (TetrisData.BOARD_HEIGHT - 1 - y) * _cellSize,
                        0f
                    );
                    cell.transform.position = position;

                    SpriteRenderer spriteRenderer = cell.AddComponent<SpriteRenderer>();
                    if (spriteRenderer == null)
                    {
                        Debug.LogError($"[ERROR] TetrisRenderer::InitializeBoard - Failed to add SpriteRenderer at [{y}, {x}]");
                        continue;
                    }

                    spriteRenderer.sprite = _blockSprite;
                    spriteRenderer.color = _emptyColor;
                    spriteRenderer.sortingOrder = 0;

                    _boardCells[y, x] = cell;
                }
            }

            Debug.Log($"[INFO] TetrisRenderer::InitializeBoard - Board initialized: {TetrisData.BOARD_WIDTH}x{TetrisData.BOARD_HEIGHT}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ERROR] TetrisRenderer::InitializeBoard - Exception during board initialization: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// 현재 블록용 GameObject 초기화 (4개)
    /// </summary>
    private void InitializeCurrentPiece()
    {
        try
        {
            _currentPieceBlocks = new GameObject[4];

            for (int i = 0; i < 4; i++)
            {
                GameObject block = new GameObject($"CurrentBlock_{i}");
                if (block == null)
                {
                    Debug.LogError($"[ERROR] TetrisRenderer::InitializeCurrentPiece - Failed to create block at index {i}");
                    continue;
                }

                block.transform.parent = transform;

                SpriteRenderer spriteRenderer = block.AddComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    Debug.LogError($"[ERROR] TetrisRenderer::InitializeCurrentPiece - Failed to add SpriteRenderer at index {i}");
                    continue;
                }

                spriteRenderer.sprite = _blockSprite;
                spriteRenderer.sortingOrder = 1; // 보드 위에 렌더링
                spriteRenderer.enabled = false; // 처음엔 숨김

                _currentPieceBlocks[i] = block;
            }

            Debug.Log("[INFO] TetrisRenderer::InitializeCurrentPiece - Current piece blocks initialized");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ERROR] TetrisRenderer::InitializeCurrentPiece - Exception during initialization: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// 다음 블록 미리보기용 GameObject 초기화 (3개 x 4블록 = 12개)
    /// </summary>
    private void InitializeNextPiecePreview()
    {
        try
        {
            _nextPieceBlocks = new GameObject[3][];

            for (int pieceIndex = 0; pieceIndex < 3; pieceIndex++)
            {
                _nextPieceBlocks[pieceIndex] = new GameObject[4];

                for (int blockIndex = 0; blockIndex < 4; blockIndex++)
                {
                    GameObject block = new GameObject($"NextBlock_{pieceIndex}_{blockIndex}");
                    if (block == null)
                    {
                        Debug.LogError($"[ERROR] TetrisRenderer::InitializeNextPiecePreview - Failed to create block at [{pieceIndex}][{blockIndex}]");
                        continue;
                    }

                    block.transform.parent = transform;

                    SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
                    if (renderer == null)
                    {
                        Debug.LogError($"[ERROR] TetrisRenderer::InitializeNextPiecePreview - Failed to add SpriteRenderer at [{pieceIndex}][{blockIndex}]");
                        continue;
                    }

                    renderer.sprite = _blockSprite;
                    renderer.sortingOrder = 2;
                    renderer.enabled = false;

                    _nextPieceBlocks[pieceIndex][blockIndex] = block;
                }
            }

            Debug.Log("[INFO] TetrisRenderer::InitializeNextPiecePreview - Next piece preview initialized (3 pieces)");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ERROR] TetrisRenderer::InitializeNextPiecePreview - Exception during initialization: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// 그리드 라인 그리기 (선택적)
    /// </summary>
    private void DrawGrid()
    {
        _gridContainer = new GameObject("Grid");
        _gridContainer.transform.parent = transform;

        // 세로 라인
        for (int x = 0; x <= TetrisData.BOARD_WIDTH; x++)
        {
            GameObject line = new GameObject($"GridLine_V_{x}");
            line.transform.parent = _gridContainer.transform;

            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = _gridColor;
            lineRenderer.endColor = _gridColor;
            lineRenderer.sortingOrder = -1;

            Vector3 start = new Vector3(
                _boardOffset.position.x + x * _cellSize,
                _boardOffset.position.y,
                0f
            );
            Vector3 end = new Vector3(
                _boardOffset.position.x + x * _cellSize,
                _boardOffset.position.y + TetrisData.BOARD_HEIGHT * _cellSize,
                0f
            );

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }

        // 가로 라인
        for (int y = 0; y <= TetrisData.BOARD_HEIGHT; y++)
        {
            GameObject line = new GameObject($"GridLine_H_{y}");
            line.transform.parent = _gridContainer.transform;

            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = _gridColor;
            lineRenderer.endColor = _gridColor;
            lineRenderer.sortingOrder = -1;

            Vector3 start = new Vector3(
                _boardOffset.position.x,
                _boardOffset.position.y + y * _cellSize,
                0f
            );
            Vector3 end = new Vector3(
                _boardOffset.position.x + TetrisData.BOARD_WIDTH * _cellSize,
                _boardOffset.position.y + y * _cellSize,
                0f
            );

            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }

        Debug.Log("[INFO] TetrisRenderer::DrawGrid - Grid drawn");
    }

    /// <summary>
    /// 보드 렌더링 (고정된 블록들)
    /// </summary>
    private void RenderBoard()
    {
        if (_tetrisData == null)
        {
            Debug.LogWarning("[WARNING] TetrisRenderer::RenderBoard - TetrisData is null");
            return;
        }

        if (_boardCells == null)
        {
            Debug.LogError("[ERROR] TetrisRenderer::RenderBoard - Board cells not initialized");
            return;
        }

        for (int y = 0; y < TetrisData.BOARD_HEIGHT; y++)
        {
            for (int x = 0; x < TetrisData.BOARD_WIDTH; x++)
            {
                // 배열 경계 검사
                if (y >= _boardCells.GetLength(0) || x >= _boardCells.GetLength(1))
                {
                    Debug.LogError($"[ERROR] TetrisRenderer::RenderBoard - Array index out of bounds: [{y}, {x}]");
                    continue;
                }

                GameObject cell = _boardCells[y, x];
                if (cell == null)
                {
                    Debug.LogError($"[ERROR] TetrisRenderer::RenderBoard - Cell GameObject is null at [{y}, {x}]");
                    continue;
                }

                SpriteRenderer spriteRenderer = cell.GetComponent<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    Debug.LogError($"[ERROR] TetrisRenderer::RenderBoard - SpriteRenderer is null at [{y}, {x}]");
                    continue;
                }

                int blockType = _tetrisData.GetBlock(x, y);
                Color blockColor = GetColorForBlockType(blockType);

                spriteRenderer.sprite = _blockSprite;
                spriteRenderer.color = blockColor;
            }
        }
    }

    /// <summary>
    /// 현재 떨어지는 블록 렌더링
    /// </summary>
    private void RenderCurrentPiece()
    {
        if (_tetrisData == null)
        {
            Debug.LogWarning("[WARNING] TetrisRenderer::RenderCurrentPiece - TetrisData is null");
            return;
        }

        if (_currentPieceBlocks == null || _currentPieceBlocks.Length != 4)
        {
            Debug.LogError("[ERROR] TetrisRenderer::RenderCurrentPiece - Current piece blocks not initialized properly");
            return;
        }

        TetrisPiece piece = _tetrisData.CurrentPiece;
        if (piece.Type == TetrisPieceType.None)
        {
            Debug.LogWarning("[WARNING] TetrisRenderer::RenderCurrentPiece - CurrentPiece type is None");
            // 모든 블록 숨기기
            HideBlocks(_currentPieceBlocks);
            return;
        }

        int[][] shape = piece.GetShape();
        if (shape == null || shape.Length != 4)
        {
            Debug.LogError("[ERROR] TetrisRenderer::RenderCurrentPiece - Invalid shape array");
            HideBlocks(_currentPieceBlocks);
            return;
        }

        Color blockColor = GetColorForBlockType((int)piece.Type);
        int blockIndex = 0;

        for (int y = 0; y < 4; y++)
        {
            // Shape 행 검증
            if (shape[y] == null || shape[y].Length != 4)
            {
                Debug.LogError($"[ERROR] TetrisRenderer::RenderCurrentPiece - Invalid shape row at y={y}");
                continue;
            }

            for (int x = 0; x < 4; x++)
            {
                if (shape[y][x] != 0)
                {
                    // blockIndex 오버플로우 체크 및 경고
                    if (blockIndex >= 4)
                    {
                        Debug.LogError($"[ERROR] TetrisRenderer::RenderCurrentPiece - Block index overflow: {blockIndex} (expected max 3). Shape has more than 4 blocks.");
                        break;
                    }

                    GameObject block = _currentPieceBlocks[blockIndex];
                    if (block == null)
                    {
                        Debug.LogError($"[ERROR] TetrisRenderer::RenderCurrentPiece - Block GameObject is null at index {blockIndex}");
                        blockIndex++;
                        continue;
                    }

                    int boardX = piece.X + x - 1;
                    int boardY = piece.Y + y - 1;

                    // 화면 좌표로 변환
                    Vector3 position = new Vector3(
                        _boardOffset.position.x + boardX * _cellSize,
                        _boardOffset.position.y + (TetrisData.BOARD_HEIGHT - 1 - boardY) * _cellSize,
                        0f
                    );

                    block.transform.position = position;

                    SpriteRenderer spriteRenderer = block.GetComponent<SpriteRenderer>();
                    if (spriteRenderer == null)
                    {
                        Debug.LogError($"[ERROR] TetrisRenderer::RenderCurrentPiece - SpriteRenderer is null at index {blockIndex}");
                        blockIndex++;
                        continue;
                    }

                    spriteRenderer.sprite = _blockSprite;
                    spriteRenderer.color = blockColor;
                    spriteRenderer.enabled = true;

                    blockIndex++;
                }
            }
        }

        // 나머지 블록 숨기기
        for (int i = blockIndex; i < 4; i++)
        {
            if (_currentPieceBlocks[i] != null)
            {
                SpriteRenderer spriteRenderer = _currentPieceBlocks[i].GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = false;
                }
            }
        }
    }

    /// <summary>
    /// 다음 블록 미리보기 렌더링 (3개)
    /// </summary>
    private void RenderNextPieces()
    {
        if (_tetrisData == null)
        {
            Debug.LogWarning("[WARNING] TetrisRenderer::RenderNextPieces - TetrisData is null");
            return;
        }

        if (_nextPieceBlocks == null || _nextPieceBlocks.Length != 3)
        {
            Debug.LogError("[ERROR] TetrisRenderer::RenderNextPieces - Next piece blocks not initialized properly");
            return;
        }

        // 3개의 Next Piece 렌더링
        for (int pieceIndex = 0; pieceIndex < 3; pieceIndex++)
        {
            if (pieceIndex >= _tetrisData.NextPieces.Count)
            {
                // 큐에 블록이 충분하지 않으면 숨김
                HideBlocks(_nextPieceBlocks[pieceIndex]);
                continue;
            }

            TetrisPiece piece = _tetrisData.NextPieces[pieceIndex];
            if (piece.Type == TetrisPieceType.None)
            {
                HideBlocks(_nextPieceBlocks[pieceIndex]);
                continue;
            }

            int[][] shape = piece.GetShape();
            if (shape == null || shape.Length != 4)
            {
                Debug.LogError($"[ERROR] TetrisRenderer::RenderNextPieces - Invalid shape array for piece {pieceIndex}");
                HideBlocks(_nextPieceBlocks[pieceIndex]);
                continue;
            }

            Color blockColor = GetColorForBlockType((int)piece.Type);
            int blockIndex = 0;

            // 각 피스의 Y 오프셋 (아래로 배치)
            float yOffset = pieceIndex * _yInterval * _cellSize * _previewCellSize;

            for (int y = 0; y < 4; y++)
            {
                if (shape[y] == null || shape[y].Length != 4)
                {
                    Debug.LogError($"[ERROR] TetrisRenderer::RenderNextPieces - Invalid shape row at y={y} for piece {pieceIndex}");
                    continue;
                }

                for (int x = 0; x < 4; x++)
                {
                    if (shape[y][x] != 0)
                    {
                        if (blockIndex >= 4)
                        {
                            Debug.LogError($"[ERROR] TetrisRenderer::RenderNextPieces - Block index overflow for piece {pieceIndex}");
                            break;
                        }

                        GameObject block = _nextPieceBlocks[pieceIndex][blockIndex];
                        if (block == null)
                        {
                            Debug.LogError($"[ERROR] TetrisRenderer::RenderNextPieces - Block GameObject is null at [{pieceIndex}][{blockIndex}]");
                            blockIndex++;
                            continue;
                        }

                        Vector3 position = new Vector3(
                            _nextPieceOffset.position.x + x * _cellSize * _previewCellSize,
                            _nextPieceOffset.position.y - y * _cellSize * _previewCellSize - yOffset,
                            0f
                        );

                        block.transform.position = position;
                        block.transform.localScale = Vector3.one * _previewCellSize;

                        SpriteRenderer spriteRenderer = block.GetComponent<SpriteRenderer>();
                        if (spriteRenderer == null)
                        {
                            Debug.LogError($"[ERROR] TetrisRenderer::RenderNextPieces - SpriteRenderer is null at [{pieceIndex}][{blockIndex}]");
                            blockIndex++;
                            continue;
                        }

                        spriteRenderer.sprite = _blockSprite;
                        spriteRenderer.color = blockColor;
                        spriteRenderer.enabled = true;

                        blockIndex++;
                    }
                }
            }

            // 나머지 블록 숨기기
            for (int i = blockIndex; i < 4; i++)
            {
                if (_nextPieceBlocks[pieceIndex][i] != null)
                {
                    SpriteRenderer spriteRenderer = _nextPieceBlocks[pieceIndex][i].GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.enabled = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 블록 배열의 모든 SpriteRenderer 숨기기 (헬퍼 메서드)
    /// </summary>
    /// <param name="blocks">숨길 GameObject 배열</param>
    private void HideBlocks(GameObject[] blocks)
    {
        if (blocks == null)
        {
            return;
        }

        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i] != null)
            {
                SpriteRenderer spriteRenderer = blocks[i].GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = false;
                }
            }
        }
    }

    /// <summary>
    /// 블록 타입에 따른 색상 가져오기
    /// </summary>
    /// <param name="blockType">블록 타입 (0: 빈 칸, 1~7: 블록)</param>
    /// <returns>해당하는 Color</returns>
    private Color GetColorForBlockType(int blockType)
    {
        switch (blockType)
        {
            case 0: return _emptyColor;    // 빈 칸 - 어두운 회색
            case 1: return _cyanColor;     // I 블록 - 청록색
            case 2: return _yellowColor;   // O 블록 - 노랑
            case 3: return _purpleColor;   // T 블록 - 보라
            case 4: return _greenColor;    // S 블록 - 초록
            case 5: return _redColor;      // Z 블록 - 빨강
            case 6: return _blueColor;     // J 블록 - 파랑
            case 7: return _orangeColor;   // L 블록 - 주황
            default:
                Debug.LogWarning($"[WARNING] TetrisRenderer::GetColorForBlockType - Unknown block type: {blockType}");
                return _emptyColor;
        }
    }

    // /// <summary>
    // /// 현재 블록 위치 디버그 표시 (OnGUI)
    // /// </summary>
    // private void OnGUI()
    // {
    //     if (_tetrisData == null)
    //     {
    //         return;
    //     }
    //
    //     TetrisPiece currentPiece = _tetrisData.CurrentPiece;
    //     if (currentPiece.Type == TetrisPieceType.None)
    //     {
    //         return;
    //     }
    //
    //     // GUI 스타일 설정
    //     GUIStyle style = new GUIStyle();
    //     style.fontSize = 20;
    //     style.normal.textColor = Color.white;
    //     style.fontStyle = FontStyle.Bold;
    //
    //     // 현재 블록 정보 표시
    //     string info = $"Current Piece:\n" +
    //                   $"Type: {currentPiece.Type}\n" +
    //                   $"Position: ({currentPiece.X}, {currentPiece.Y})\n" +
    //                   $"Rotation: {currentPiece.Rotation}";
    //
    //     // 화면 왼쪽 상단에 표시
    //     GUI.Label(new Rect(10, 10, 300, 100), info, style);
    // }

    /// <summary>
    /// 정리
    /// </summary>
    private void OnDestroy()
    {
        Debug.Log("[INFO] TetrisRenderer::OnDestroy - Renderer destroyed");
    }
}
