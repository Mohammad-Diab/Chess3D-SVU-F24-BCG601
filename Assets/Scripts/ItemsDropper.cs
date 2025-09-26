using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemsDropper : MonoBehaviour
{
    public Transform[] itemsParents;
    public float dropHeight = 3f;
    public float dropTime = 0.2f;
    public float delayBetween = 0.01f;

    private Transform[] _items;

    void Start()
    {
        _items = new Transform[0];
        foreach (var parent in itemsParents)
        {
            var lcoalItems = parent.GetComponentsInChildren<Transform>(true);
            _items = _items.Concat(System.Array.FindAll(lcoalItems, t => t.childCount == 0)).ToArray();
        }

    }

    public void StartDropping()
    {
        StartCoroutine(DropTilesRandomly());
    }


    IEnumerator DropTilesRandomly()
    {
        Vector3[] originalPositions = new Vector3[_items.Length];
        for (int i = 0; i < _items.Length; i++)
        {
            originalPositions[i] = _items[i].position;
            _items[i].position += Vector3.up * dropHeight;
        }

        List<int> indices = new List<int>();
        for (int i = 0; i < _items.Length; i++) indices.Add(i);
        for (int i = 0; i < indices.Count; i++)
        {
            int rand = Random.Range(i, indices.Count);
            (indices[i], indices[rand]) = (indices[rand], indices[i]);
        }

        foreach (int idx in indices)
        {
            StartCoroutine(MoveTile(_items[idx], originalPositions[idx]));
            yield return new WaitForSeconds(delayBetween);
        }
    }

    IEnumerator MoveTile(Transform tile, Vector3 targetPos)
    {
        tile.gameObject.SetActive(true);
        Vector3 startPos = tile.position;
        float elapsed = 0f;
        while (elapsed < dropTime)
        {
            tile.position = Vector3.Lerp(startPos, targetPos, elapsed / dropTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        tile.position = targetPos;
    }
}
