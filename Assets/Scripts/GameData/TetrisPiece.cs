using System;
using UnityEngine;

/// <summary>
/// 테트리스 블록 타입
/// 7가지 표준 테트리미노
/// </summary>
public enum TetrisPieceType
{
    None = 0,
    I = 1,  // 파란색 - 일자형
    O = 2,  // 노란색 - 정사각형
    T = 3,  // 보라색 - T자형
    S = 4,  // 초록색 - S자형
    Z = 5,  // 빨간색 - Z자형
    J = 6,  // 파란색 - J자형
    L = 7   // 주황색 - L자형
}

/// <summary>
/// 테트리스 블록 구조체
/// 블록의 타입, 위치, 회전 상태를 관리합니다.
/// </summary>
[Serializable]
public struct TetrisPiece
{
    /// <summary>
    /// 블록 타입
    /// </summary>
    public TetrisPieceType Type;

    /// <summary>
    /// 블록의 중심 X 좌표
    /// </summary>
    public int X;

    /// <summary>
    /// 블록의 중심 Y 좌표
    /// </summary>
    public int Y;

    /// <summary>
    /// 회전 상태 (0, 1, 2, 3 = 0°, 90°, 180°, 270°)
    /// </summary>
    public int Rotation;

    /// <summary>
    /// 블록 모양 정의 (4x4 그리드)
    /// [회전상태][y][x]
    /// </summary>
    private static readonly int[][][][] _shapes = new int[][][][]
    {
        // None (빈 블록)
        new int[][][]
        {
            new int[][] { new int[] {0,0,0,0}, new int[] {0,0,0,0}, new int[] {0,0,0,0}, new int[] {0,0,0,0} }
        },

        // I (일자형)
        new int[][][]
        {
            new int[][] { new int[] {0,0,0,0}, new int[] {1,1,1,1}, new int[] {0,0,0,0}, new int[] {0,0,0,0} },
            new int[][] { new int[] {0,0,1,0}, new int[] {0,0,1,0}, new int[] {0,0,1,0}, new int[] {0,0,1,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,0,0,0}, new int[] {1,1,1,1}, new int[] {0,0,0,0} },
            new int[][] { new int[] {0,1,0,0}, new int[] {0,1,0,0}, new int[] {0,1,0,0}, new int[] {0,1,0,0} }
        },

        // O (정사각형)
        new int[][][]
        {
            new int[][] { new int[] {0,0,0,0}, new int[] {0,1,1,0}, new int[] {0,1,1,0}, new int[] {0,0,0,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,1,1,0}, new int[] {0,1,1,0}, new int[] {0,0,0,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,1,1,0}, new int[] {0,1,1,0}, new int[] {0,0,0,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,1,1,0}, new int[] {0,1,1,0}, new int[] {0,0,0,0} }
        },

        // T (T자형)
        new int[][][]
        {
            new int[][] { new int[] {0,0,0,0}, new int[] {0,1,0,0}, new int[] {1,1,1,0}, new int[] {0,0,0,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,1,0,0}, new int[] {0,1,1,0}, new int[] {0,1,0,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,0,0,0}, new int[] {1,1,1,0}, new int[] {0,1,0,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,1,0,0}, new int[] {1,1,0,0}, new int[] {0,1,0,0} }
        },

        // S (S자형)
        new int[][][]
        {
            new int[][] { new int[] {0,0,0,0}, new int[] {0,1,1,0}, new int[] {1,1,0,0}, new int[] {0,0,0,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,1,0,0}, new int[] {0,1,1,0}, new int[] {0,0,1,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,0,0,0}, new int[] {0,1,1,0}, new int[] {1,1,0,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {1,0,0,0}, new int[] {1,1,0,0}, new int[] {0,1,0,0} }
        },

        // Z (Z자형)
        new int[][][]
        {
            new int[][] { new int[] {0,0,0,0}, new int[] {1,1,0,0}, new int[] {0,1,1,0}, new int[] {0,0,0,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,0,1,0}, new int[] {0,1,1,0}, new int[] {0,1,0,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,0,0,0}, new int[] {1,1,0,0}, new int[] {0,1,1,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,1,0,0}, new int[] {1,1,0,0}, new int[] {1,0,0,0} }
        },

        // J (J자형)
        new int[][][]
        {
            new int[][] { new int[] {0,0,0,0}, new int[] {1,0,0,0}, new int[] {1,1,1,0}, new int[] {0,0,0,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,1,1,0}, new int[] {0,1,0,0}, new int[] {0,1,0,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,0,0,0}, new int[] {1,1,1,0}, new int[] {0,0,1,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,1,0,0}, new int[] {0,1,0,0}, new int[] {1,1,0,0} }
        },

        // L (L자형)
        new int[][][]
        {
            new int[][] { new int[] {0,0,0,0}, new int[] {0,0,1,0}, new int[] {1,1,1,0}, new int[] {0,0,0,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,1,0,0}, new int[] {0,1,0,0}, new int[] {0,1,1,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {0,0,0,0}, new int[] {1,1,1,0}, new int[] {1,0,0,0} },
            new int[][] { new int[] {0,0,0,0}, new int[] {1,1,0,0}, new int[] {0,1,0,0}, new int[] {0,1,0,0} }
        }
    };

    /// <summary>
    /// 생성자
    /// </summary>
    /// <param name="type">블록 타입</param>
    /// <param name="x">초기 X 위치</param>
    /// <param name="y">초기 Y 위치</param>
    public TetrisPiece(TetrisPieceType type, int x, int y)
    {
        Type = type;
        X = x;
        Y = y;
        Rotation = 0;
    }

    /// <summary>
    /// 랜덤 블록 생성
    /// </summary>
    /// <param name="x">초기 X 위치</param>
    /// <param name="y">초기 Y 위치</param>
    /// <returns>랜덤 테트리스 블록</returns>
    public static TetrisPiece CreateRandom(int x, int y)
    {
        TetrisPieceType randomType = (TetrisPieceType)UnityEngine.Random.Range(1, 8);
        return new TetrisPiece(randomType, x, y);
    }

    /// <summary>
    /// 현재 회전 상태의 블록 모양 가져오기
    /// </summary>
    /// <returns>4x4 블록 모양 배열</returns>
    public int[][] GetShape()
    {
        int typeIndex = (int)Type;
        if (typeIndex < 0 || typeIndex >= _shapes.Length)
        {
            return _shapes[0][0]; // None
        }

        int rotationIndex = Rotation % 4;
        return _shapes[typeIndex][rotationIndex];
    }

    /// <summary>
    /// 특정 위치에 블록이 있는지 확인
    /// </summary>
    /// <param name="localX">로컬 X 좌표 (0~3)</param>
    /// <param name="localY">로컬 Y 좌표 (0~3)</param>
    /// <returns>블록이 있으면 true</returns>
    public bool HasBlockAt(int localX, int localY)
    {
        if (localX < 0 || localX >= 4 || localY < 0 || localY >= 4)
        {
            return false;
        }

        int[][] shape = GetShape();
        return shape[localY][localX] != 0;
    }

    /// <summary>
    /// 시계방향 회전
    /// </summary>
    public void RotateClockwise()
    {
        Rotation = (Rotation + 1) % 4;
    }

    /// <summary>
    /// 반시계방향 회전
    /// </summary>
    public void RotateCounterClockwise()
    {
        Rotation = (Rotation + 3) % 4; // -1과 동일 (mod 4)
    }

    /// <summary>
    /// 왼쪽으로 이동
    /// </summary>
    public void MoveLeft()
    {
        X--;
    }

    /// <summary>
    /// 오른쪽으로 이동
    /// </summary>
    public void MoveRight()
    {
        X++;
    }

    /// <summary>
    /// 아래로 이동
    /// </summary>
    public void MoveDown()
    {
        Y++;
    }

    /// <summary>
    /// 위로 이동 (일반적으로 사용하지 않음)
    /// </summary>
    public void MoveUp()
    {
        Y--;
    }

    /// <summary>
    /// 디버깅용 문자열 변환
    /// </summary>
    public override string ToString()
    {
        return $"TetrisPiece [Type: {Type}, Position: ({X}, {Y}), Rotation: {Rotation}]";
    }
}
