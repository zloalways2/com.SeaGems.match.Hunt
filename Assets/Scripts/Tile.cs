using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class Tile : MonoBehaviour
{
    public int x;
    public int y;

    private Item _item;
    
    public Item Item
    {
        get => _item;
        set
        {
            if (_item == value || value == null) return; // Check for null before setting
            _item = value;
            if (icon != null && _item != null)
            {
                icon.sprite = _item.sprite; // Ensure icon and _item are not null
            }
        }
    }

    public Image icon;
    public Button button;

    public Tile Left => x > 0 ? Board.Instance.Tiles[x - 1, y] : null;
    public Tile Top => y > 0 ? Board.Instance.Tiles[x, y - 1] : null;
    public Tile Right => x < Board.Instance.Width - 1 ? Board.Instance.Tiles[x + 1, y] : null;
    public Tile Bottom => y < Board.Instance.Height - 1 ? Board.Instance.Tiles[x, y + 1] : null;

    public Tile[] Neighbours => new[]
    {
        Left,
        Top,
        Right,
        Bottom,
    };

    private void Start()
    {
        if (button == null)
        {
            // Debug.LogError("Button reference is not assigned.");
            return;
        }
        button.onClick.AddListener(() =>
        {
        Debug.Log("Tile clicked");
        Board.Instance.Select(this);
    });
        Debug.Log("Button OnClick listener added.");
    }

    public List<Tile> GetConnectedTiles(List<Tile> exclude = null)
    {
        var result = new List<Tile> { this };

        if (exclude == null)
        {
            exclude = new List<Tile> { this };
        }
        else
        {
            exclude.Add(this);
        }

        foreach (var neighbour in Neighbours)
        {
            if (neighbour == null)
            {
              //  Debug.Log($"Neighbour is null at ({x}, {y})");
                continue;
            }

            if (exclude.Contains(neighbour))
            {
                //Debug.Log($"Neighbour already excluded at ({neighbour.x}, {neighbour.y})");
                continue;
            }

            if (neighbour.Item != Item)
            {
                //Debug.Log($"Neighbour item mismatch at ({neighbour.x}, {neighbour.y})");
                continue;
            }

            result.AddRange(neighbour.GetConnectedTiles(exclude));
        }

        return result;
    }
    public void SetIconTransparency(float alpha)
    {
        if (icon != null)
        {
            Color color = icon.color;
            color.a = alpha;
            icon.color = color;
        }
        else
        {
            Debug.LogWarning($"Icon is not assigned for tile at ({x}, {y}).");
        }
    }
}