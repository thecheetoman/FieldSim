using System.Collections.Generic;
using UnityEngine;
using Util;

public static class GamePiecePool
{
    private static readonly Dictionary<PieceNames, Stack<GamePiece>> _pool = new();
    private static readonly Dictionary<PieceNames, GameObject> _prefabCache = new();

    private static Transform _poolRoot;

    private static Transform GetPoolRoot()
    {
        if (_poolRoot == null)
        {
            var go = new GameObject("_GamePiecePool");
            go.SetActive(false);
            Object.DontDestroyOnLoad(go);
            _poolRoot = go.transform;
        }
        return _poolRoot;
    }

    public static void Prewarm(PieceNames type, GameObject prefab, int count)
    {
        _prefabCache[type] = prefab;

        if (!_pool.ContainsKey(type))
            _pool[type] = new Stack<GamePiece>();

        var stack = _pool[type];
        for (int i = stack.Count; i < count; i++)
        {
            var piece = Object.Instantiate(prefab, GetPoolRoot()).GetComponent<GamePiece>();
            piece.gameObject.SetActive(false);
            stack.Push(piece);
        }
    }

    public static GamePiece Get(PieceNames type, GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        _prefabCache[type] = prefab;

        if (_pool.TryGetValue(type, out var stack) && stack.Count > 0)
        {
            var piece = stack.Pop();
            var go = piece.gameObject;
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);
            piece.state = GamePieceState.World;

            if (piece.rb != null)
            {
                piece.rb.velocity = Vector3.zero;
                piece.rb.angularVelocity = Vector3.zero;
                piece.rb.detectCollisions = true;
            }

            if (piece.colliderParent != null)
                piece.colliderParent.SetActive(true);

            return piece;
        }

        var instance = Object.Instantiate(prefab, position, rotation, parent);
        return instance.GetComponent<GamePiece>();
    }

    public static void Return(GamePiece piece)
    {
        var go = piece.gameObject;
        go.SetActive(false);
        go.transform.SetParent(GetPoolRoot(), false);

        if (piece.rb != null)
        {
            piece.rb.velocity = Vector3.zero;
            piece.rb.angularVelocity = Vector3.zero;
            piece.rb.detectCollisions = false;
        }

        if (piece.colliderParent != null)
            piece.colliderParent.SetActive(false);

        var type = piece.pieceType;
        if (!_pool.ContainsKey(type))
            _pool[type] = new Stack<GamePiece>();

        _pool[type].Push(piece);
    }

    public static void Clear()
    {
        foreach (var stack in _pool.Values)
        {
            foreach (var piece in stack)
            {
                if (piece != null)
                    Object.Destroy(piece.gameObject);
            }
            stack.Clear();
        }
        _pool.Clear();
        _prefabCache.Clear();
    }
}