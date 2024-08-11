
using UnityEngine;

public static class ItemDatabase
{
    public static Item[] Items { get; private set; }
    public static Item empty_item { get; private set; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Items = Resources.LoadAll<Item>("Item");
         empty_item = Resources.Load<Item>("empty_item");
        //Debug.Log($"Empty item: {empty_item.name}");
        //Debug.Log("ItemDatabase initialized with " + Items.Length + " items.");
    }
}
    