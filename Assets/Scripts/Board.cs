using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Text.RegularExpressions;
using static UnityEngine.Networking.UnityWebRequest;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public sealed class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }
    public Row[] rows;
    public Tile[,] Tiles { get; private set; }
    private bool _swapping;
    private List<Tile> _selection = new List<Tile>();
    private bool _autoSwap = false;
    public int _score = 0;
    public int Width => Tiles != null ? Tiles.GetLength(0) : 0;
    public int Height => Tiles != null ? Tiles.GetLength(1) : 0;
    private bool _checkingBoard = false;
    private int maxIterations = 400;
    private bool swapped = false;
    private List<Item> _levelItems;
    const int _DifferentItemsInLevel = 4;
    [SerializeField] AudioSource clickSource;
    [SerializeField] Text scoreLabel;
    [SerializeField] List<GameObject> particlesList;
    [SerializeField] AudioSource matchSound;
    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        _swapping = false;
       
        InitializeBoard();
        DebugBoardInitialization();
    //     await clearBoardFromMatches();
        
    }

    private async Task clearBoardFromMatches()
    {
        _checkingBoard = true;
        bool anyMatches = false;
        int iterations = 0;

        Debug.Log("Checking board");
        do
        {
            anyMatches = await refreshBoard(null);
            Debug.Log($"Any Matches? {anyMatches}");
            iterations++;
            if (iterations >= maxIterations)
            {
                Debug.LogWarning("Max iterations reached, breaking out of the loop to avoid infinite loop");
                break;
            }
        } while (anyMatches);

        _checkingBoard = false;
        Debug.Log("Checking board ended!");
    }

    private void InitializeBoard()
    {
        if (ItemDatabase.Items == null || ItemDatabase.Items.Length == 0)
        {
            Debug.LogError("ItemDatabase or its items are not initialized.");
            return;
        }
        _levelItems = new List<Item>();
        for (int i=0;i<_DifferentItemsInLevel;i++)
        {
            var randomItem = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];
            while (_levelItems.Exists(item => item == randomItem)) randomItem = ItemDatabase.Items[UnityEngine.Random.Range(0, ItemDatabase.Items.Length)];
            _levelItems.Add(randomItem);
        }
        int maxRowLength = rows.Max(row => row.tiles.Length);
        int rowCount = rows.Length;

        if (maxRowLength == 0 || rowCount == 0)
        {
            Debug.LogError("Rows or tiles array is not properly initialized.");
            return;
        }

        Tiles = new Tile[maxRowLength, rowCount];

        for (var y = 0; y < rowCount; y++)
        {
            if (rows[y] == null || rows[y].tiles == null)
            {
                Debug.LogError($"Row {y} or its tiles array is not initialized.");
                continue;
            }

            for (var x = 0; x < rows[y].tiles.Length; x++)
            {
                if (rows[y].tiles[x] == null)
                {
                    Debug.LogError($"Tile at ({x}, {y}) is not initialized.");
                    continue;
                }

                var tile = rows[y].tiles[x];
                tile.x = x;
                tile.y = y;

                tile.Item = _levelItems[UnityEngine.Random.Range(0, _levelItems.Count)];
                Tiles[x, y] = tile;
            }
        }
        this.gameObject.SetActive(true);
        Debug.Log($"Board initialized with Width: {Width} and Height: {Height}");
    }

    private void DebugBoardInitialization()
    {
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                if (Tiles[x, y] == null)
                {
                    Debug.LogError($"Tile at ({x}, {y}) is null.");
                }
                else
                {
                    Debug.Log($"Tile at ({x}, {y}) initialized with item {Tiles[x, y].Item.name}.");
                }
            }
        }

        Debug.Log($"Board initialized with Width: {Width} and Height: {Height}");
    }

    public async void Select(Tile tile)
    {
        Debug.Log($"Tile {tile.name} is selected");
        clickSource.Play();
       // tile.gameObject.SetActive(false);
        if (!_selection.Contains(tile))
        {
            _selection.Add(tile);
        }

        if (_selection.Count < 2) return;

        var tile1 = _selection[0];
        var tile2 = _selection[1];

        if (AreNeighbors(tile1, tile2))
        {
            Debug.Log($"Selected tiles at ({tile1.x}, {tile1.y}) and ({tile2.x}, {tile2.y})");
            await Swap(tile1, tile2);
        }
        else
        {
            Debug.Log("Selected tiles are not neighbors.");
        }

        _selection.Clear();
    }

    public async Task Swap(Tile tile1, Tile tile2)
    {
        if (!_swapping)
        {
            _swapping = true;
            var icon1 = tile1.icon;
            var icon2 = tile2.icon;

            var icon1Transform = icon1.transform;
            var icon2Transform = icon2.transform;

            // Animate the swap
            Vector3 icon1StartPos = icon1Transform.position;
            Vector3 icon2StartPos = icon2Transform.position;

            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                icon1Transform.position = Vector3.Lerp(icon1StartPos, icon2StartPos, t);
                icon2Transform.position = Vector3.Lerp(icon2StartPos, icon1StartPos, t);

                await Task.Yield();
            }

            icon1Transform.position = icon2StartPos;
            icon2Transform.position = icon1StartPos;

            // Swap the parent transforms
            icon1Transform.SetParent(tile2.transform);
            icon2Transform.SetParent(tile1.transform);

            // Swap the icons in the Tile objects
            tile1.icon = icon2;
            tile2.icon = icon1;

            // Swap the Item references in the Tile objects
            var tempItem = tile1.Item;
            tile1.Item = tile2.Item;
            tile2.Item = tempItem;

            Debug.Log($"Swapped tiles at ({tile1.x}, {tile1.y}) and ({tile2.x}, {tile2.y})");

            var match1 = await refreshBoard(tile1);
            var match2 = await refreshBoard(tile2);
            
            // Swap back if no match found            
        if (!match1 && !match2)
        {
            await autoSwap(tile1, tile2); // Swap back if no match found
        }
        else
            {
                StartCoroutine(checkBoardForMatchesAfterSwap()); // Check the entire board for matches after a valid swap
            }

            _swapping = false;
        }
    }

    public async Task autoSwap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;

        // Animate the swap
        Vector3 icon1StartPos = icon1Transform.position;
        Vector3 icon2StartPos = icon2Transform.position;

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            icon1Transform.position = Vector3.Lerp(icon1StartPos, icon2StartPos, t);
            icon2Transform.position = Vector3.Lerp(icon2StartPos, icon1StartPos, t);

            await Task.Yield();
        }

        icon1Transform.position = icon2StartPos;
        icon2Transform.position = icon1StartPos;

        // Swap the parent transforms
        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        // Swap the icons in the Tile objects
        tile1.icon = icon2;
        tile2.icon = icon1;

        // Swap the Item references in the Tile objects
        var tempItem = tile1.Item;
        tile1.Item = tile2.Item;
        tile2.Item = tempItem;

        Debug.Log($"Auto Swapped back tiles at ({tile1.x}, {tile1.y}) and ({tile2.x}, {tile2.y})");
    }

    private bool AreNeighbors(Tile tile1, Tile tile2)
    {
        return (tile1.x == tile2.x && Mathf.Abs(tile1.y - tile2.y) == 1) ||
               (tile1.y == tile2.y && Mathf.Abs(tile1.x - tile2.x) == 1);
    }

    private async Task<bool> refreshBoardChecker() //check all matches
    {
        bool result = false;
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                var tile = Tiles[i, j];
                var tempTile = tile;
                int[] neighbors = { 0, 0, 0, 0 }; // Left, Right, Up, Down
                List<Tile> tilesToManipulate = new List<Tile>();
                tilesToManipulate.Add(tile);
                //Left
                while ((tempTile.Left != null) && (tempTile.Item.sprite == tempTile.Left.Item.sprite))
                {
                    neighbors[0]++;
                    tempTile = tempTile.Left;
                }
                tempTile = tile;
                //Right
                while ((tempTile.Right != null) && (tempTile.Item.sprite == tempTile.Right.Item.sprite))
                {
                    neighbors[1]++;
                    tempTile = tempTile.Right;
                }
                tempTile = tile;
                //Top
                while ((tempTile.Top != null) && (tempTile.Item.sprite == tempTile.Top.Item.sprite))
                {
                    neighbors[2]++;
                    tempTile = tempTile.Top;
                }
                tempTile = tile;
                //Bottom
                while ((tempTile.Bottom != null) && (tempTile.Item.sprite == tempTile.Bottom.Item.sprite))
                {
                    neighbors[3]++;
                    tempTile = tempTile.Bottom;
                }
                //Checking matches
                int horizontalMatch = neighbors[0] + neighbors[1];
                int verticalMatch = neighbors[2] + neighbors[3];
                if (horizontalMatch >= 2 || verticalMatch >= 2) //In case match found
                {
                    //Left
                    tempTile = tile;
                    for (int k = 0; k < neighbors[0]; k++)
                    {
                        tempTile = tempTile.Left;
                        tilesToManipulate.Add(tempTile);
                        _score += !_checkingBoard ? tile.Item.value : 0;
                    }
                    //Right
                    tempTile = tile;
                    for (int k = 0; k < neighbors[1]; k++)
                    {
                        tempTile = tempTile.Right;
                        tilesToManipulate.Add(tempTile);
                        _score += !_checkingBoard ? tile.Item.value : 0;
                    }
                    //Top
                    tempTile = tile;
                    for (int k = 0; k < neighbors[2]; k++)
                    {
                        tempTile = tempTile.Top;
                        tilesToManipulate.Add(tempTile);
                        _score += !_checkingBoard ? tile.Item.value : 0;
                    }
                    //Bottom
                    tempTile = tile;
                    for (int k = 0; k < neighbors[3]; k++)
                    {
                        tempTile = tempTile.Bottom;
                        tilesToManipulate.Add(tempTile);
                        _score += !_checkingBoard ? tile.Item.value : 0;
                    }
                    await this.StartCoroutineAsTask(animation(tilesToManipulate));
                    Debug.Log($"Match found and items switched | up : {neighbors[2]} | down : {neighbors[3]} | left : {neighbors[0]} | right: {neighbors[1]} ");
                    result = true;
                }
            }
        }
        return result;
    }

    private async Task<bool> refreshBoard(Tile selectedTile)
    {
        if (selectedTile == null)
        {
            bool result = false;
            List<Tile> tilesToRefresh = new List<Tile>();
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    var tile = Tiles[i, j];
                    Debug.Log($"Tile ({i + 1},{j + 1}) : {tile.Item.sprite.name}");
                    Tile tempTile;
                    if (tile.Right != null)
                    {
                        tempTile = tile.Right;
                        if (tempTile.Right != null)
                            if (tile.Item.sprite == tempTile.Item.sprite)
                            {
                                if (tile.Right.Item.sprite == tempTile.Right.Item.sprite)
                                {
                                    tilesToRefresh.Add(tile);
                                    tilesToRefresh.Add(tempTile);
                                    tilesToRefresh.Add(tempTile.Right);
                                    _score += 5;
                                    if (tempTile.Right.Right != null && tempTile.Right.Right.Item.sprite == tile.Right.Item.sprite)
                                    {
                                        tilesToRefresh.Add(tile.Right.Right);
                                        _score += 5;
                                        await this.StartCoroutineAsTask(animation(tilesToRefresh));
                                        Debug.Log("Switched bottom 4x items");
                                    }
                                    else await this.StartCoroutineAsTask(animation(tilesToRefresh));

                                    result = true;
                                    Debug.Log($"Automatic match found right :{tilesToRefresh} | (i, j) = {i},{j} |  result : {result}");
                                }
                            }
                    }

                    if (tile.Top != null)
                    {
                        tempTile = tile.Top;
                        if (tempTile.Top != null)
                        {
                            if (tile.Item.sprite == tempTile.Item.sprite)
                            {
                                if (tile.Top.Item.sprite == tile.Top.Top.Item.sprite)
                                {
                                    tilesToRefresh.Add(tile);
                                    tilesToRefresh.Add(tempTile);
                                    tilesToRefresh.Add(tempTile.Top);
                                    _score += 5;
                                    if (tempTile.Top.Top != null && tempTile.Top.Top.Item.sprite == tile.Top.Item.sprite)
                                    {
                                        tilesToRefresh.Add(tempTile.Top.Top);
                                        _score += 5;
                                        if (tempTile.Top.Top.Top != null && tempTile.Top.Top.Top.Item.sprite == tile.Top.Top.Item.sprite)
                                        {
                                            tilesToRefresh.Add(tempTile.Top.Top.Top);
                                            _score += 5;
                                            await this.StartCoroutineAsTask(animation(tilesToRefresh));
                                            Debug.Log("Switched top 5x items");
                                        }
                                    }
                                    else await this.StartCoroutineAsTask(animation(tilesToRefresh));
                                    result = true;
                                    Debug.Log($"Automatic match found top :{tilesToRefresh} | (i, j) = {i},{j} |  result : {result}");
                                }
                            }
                        }
                    }
                    if (tile.Left != null)
                    {
                        tempTile = tile.Left;
                        if (tempTile.Left != null)
                        {
                            if (tile.Item.sprite == tempTile.Item.sprite)
                            {
                                if (tile.Left.Item.sprite == tile.Left.Left.Item.sprite)
                                {
                                    tilesToRefresh.Add(tile);
                                    tilesToRefresh.Add(tempTile);
                                    tilesToRefresh.Add(tempTile.Left);
                                    _score += 5;
                                    if (tempTile.Left.Left != null && tempTile.Left.Left.Item.sprite == tile.Left.Item.sprite)
                                    {
                                        tilesToRefresh.Add(tempTile.Left.Left);
                                        _score += 5;
                                        await this.StartCoroutineAsTask(animation(tilesToRefresh));
                                        Debug.Log("Switched left 4x items");
                                    }
                                    else await this.StartCoroutineAsTask(animation(tilesToRefresh));
                                    result = true;
                                    Debug.Log($"Automatic match found left :{tilesToRefresh} | (i, j) = {i},{j} |  result : {result}");
                                }
                            }
                        }
                    }
                    if (tile.Bottom != null)
                    {
                        tempTile = tile.Bottom;
                        if (tempTile.Bottom != null)
                        {
                            if (tile.Item.sprite == tempTile.Item.sprite)
                            {
                                if (tile.Bottom.Item.sprite == tile.Bottom.Bottom.Item.sprite)
                                {
                                    tilesToRefresh.Add(tile);
                                    tilesToRefresh.Add(tempTile);
                                    tilesToRefresh.Add(tempTile.Bottom);
                                    _score += 5;
                                    if (tempTile.Bottom.Bottom != null && tempTile.Bottom.Bottom.Item == tile.Bottom.Item)
                                    {
                                        tilesToRefresh.Add(tile.Bottom.Bottom);
                                        _score += 5;
                                        if (tempTile.Bottom.Bottom.Bottom != null && tempTile.Bottom.Bottom.Bottom.Item == tile.Bottom.Bottom.Item)
                                        {
                                            tilesToRefresh.Add(tempTile.Bottom.Bottom.Bottom);
                                            _score += 5;
                                            await this.StartCoroutineAsTask(animation(tilesToRefresh));
                                            Debug.Log("Switched bottom 5x items");
                                        }
                                        else await this.StartCoroutineAsTask(animation(tilesToRefresh));
                                        result = true;
                                        Debug.Log($"Automatic match found bottom :{tilesToRefresh} | (i, j) = {i},{j} |  result : {result}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
        else
        {
            Debug.Log($"Checking cell {selectedTile.x + 1},{selectedTile.y + 1}");
            bool result = false;
            var tile = selectedTile;
            var tempTile = tile;
            int[] neighbors = { 0, 0, 0, 0 }; // Left, Right, Up, Down
            List<Tile> tilesToManipulate = new List<Tile>();
            tilesToManipulate.Add(tile);
            //Left
            while ((tempTile.Left != null) && (tempTile.Item.sprite == tempTile.Left.Item.sprite))
            {
                neighbors[0]++;
                tempTile = tempTile.Left;
            }
            tempTile = tile;
            //Right
            while ((tempTile.Right != null) && (tempTile.Item.sprite == tempTile.Right.Item.sprite))
            {
                neighbors[1]++;
                tempTile = tempTile.Right;
            }
            tempTile = tile;
            //Top
            while ((tempTile.Top != null) && (tempTile.Item.sprite == tempTile.Top.Item.sprite))
            {
                neighbors[2]++;
                tempTile = tempTile.Top;
            }
            tempTile = tile;
            //Bottom
            while ((tempTile.Bottom != null) && (tempTile.Item.sprite == tempTile.Bottom.Item.sprite))
            {
                neighbors[3]++;
                tempTile = tempTile.Bottom;
            }
            int horizontalMatch = neighbors[0] + neighbors[1];
            int verticalMatch = neighbors[2] + neighbors[3];
            if (horizontalMatch >= 2 || verticalMatch >= 2)
            {
                //Left
                tempTile = tile;
                for (int i = 0; i < neighbors[0]; i++)
                {
                    tempTile = tempTile.Left;
                    tilesToManipulate.Add(tempTile);
                    _score += !_checkingBoard ? tile.Item.value : 0;
                }
                //Right
                tempTile = tile;
                for (int i = 0; i < neighbors[1]; i++)
                {
                    tempTile = tempTile.Right;
                    tilesToManipulate.Add(tempTile);
                    _score += !_checkingBoard ? tile.Item.value : 0;
                }
                //Top
                tempTile = tile;
                for (int i = 0; i < neighbors[2]; i++)
                {
                    tempTile = tempTile.Top;
                    tilesToManipulate.Add(tempTile);
                    _score += !_checkingBoard ? tile.Item.value : 0;
                }
                //Bottom
                tempTile = tile;
                for (int i = 0; i < neighbors[3]; i++)
                {
                    tempTile = tempTile.Bottom;
                    tilesToManipulate.Add(tempTile);
                    _score += !_checkingBoard ? tile.Item.value : 0;
                }
                await this.StartCoroutineAsTask(animation(tilesToManipulate));
                Debug.Log($"Match found and items switched | up : {neighbors[2]} | down : {neighbors[3]} | left : {neighbors[0]} | right: {neighbors[1]} ");
                result = true;
            }
            scoreLabel.text = $"SCORE: {_score}";
            return result;
        }
    }

    public void SayHi()
    {
        Debug.Log("Hi");
        particlesList[0].SetActive(true);


    }
    IEnumerator animation(List<Tile> tiles)
    {
        scoreLabel.text = $"SCORE: {_score}";
        matchSound.Play();
        int particleIndex;
        foreach (var aTile in tiles)
        {
            aTile.SetIconTransparency(0);
            particleIndex = ((aTile.x)*4)+aTile.y;
            particlesList[particleIndex].SetActive(true);
        }//aTile.Item = ItemDatabase.empty_item;
        
        yield return new WaitForSeconds(0.5f); // Wait for the animation to finish
        foreach (var aTile in tiles)
        {
            aTile.Item = _levelItems[UnityEngine.Random.Range(0, _levelItems.Count)];
            aTile.gameObject.SetActive(true);
            aTile.SetIconTransparency(1);
        }
        swapped = true;
        //
       // yield return new WaitForSeconds(3.0f);
        //
      //  StartCoroutine(CallClearBoardFromMatches());
    }

    private IEnumerator CallClearBoardFromMatches()
    {
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(clearBoardFromMatchesCoroutine());
    }

    private IEnumerator clearBoardFromMatchesCoroutine()
    {
        yield return clearBoardFromMatches().AsIEnumerator();
    }

    private IEnumerator checkBoardForMatchesAfterSwap()
    {
        Debug.Log("Checking the entire board for matches after a valid swap...");
        yield return clearBoardFromMatches().AsIEnumerator();
    }
}

public static class MonoBehaviourExtensions
{
    public static Task StartCoroutineAsTask(this MonoBehaviour monoBehaviour, IEnumerator coroutine)
    {
        var tcs = new TaskCompletionSource<bool>();
        monoBehaviour.StartCoroutine(RunCoroutine(coroutine, tcs));
        return tcs.Task;
    }

    private static IEnumerator RunCoroutine(IEnumerator coroutine, TaskCompletionSource<bool> tcs)
    {
        yield return coroutine;
        tcs.SetResult(true);
    }
}

public static class TaskExtensions
{
    public static IEnumerator AsIEnumerator(this Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted)
        {
            throw task.Exception;
        }
    }
}
