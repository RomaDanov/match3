using System;
using System.Collections;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20;
    [SerializeField] private Animation animationController;
    [SerializeField] private const string AppearAnimName = "AppearTileAnimation";
    [SerializeField] private const string DisappearAnimName = "DisappearTileAnimation";
    [SerializeField] private SpriteRenderer sprite;

    public int ID { get; private set; }
    public int Row { get; private set; }
    public int Column { get; private set; }

    private Coroutine changePositionCoroutine;
    private Color color;

    public void Init(int id, Sprite sprite, Color color)
    {
        ID = id;
        this.color = color;
        this.sprite.sprite = sprite;
        this.sprite.color = color;
    }

    public Coroutine ChangePosition(int rowIndex, int columnIndex, bool isChangePositionImmediatly = true)
    {
        Row = rowIndex;
        Column = columnIndex;
        Vector2 newPosition = new Vector2(transform.lossyScale.x * Column, -(transform.lossyScale.y * Row));

        if (isChangePositionImmediatly)
        {
            transform.localPosition = newPosition;
            return null;
        }
        else
        {
            return changePositionCoroutine = StartCoroutine(ChangePositionAnimation(newPosition));
        }
    }

    private IEnumerator ChangePositionAnimation(Vector3 newPosition)
    {
        if (changePositionCoroutine != null)
        {
            StopCoroutine(changePositionCoroutine);
        }

        while ((newPosition - transform.localPosition).magnitude > 0.001f)
        {
            transform.localPosition = Vector2.Lerp(transform.localPosition, newPosition, Time.deltaTime * moveSpeed);
            yield return null;
        }
    }

    public void ChangeOrder(int order)
    {
        sprite.sortingOrder = order;
    }

    public void Select()
    {
        sprite.color = new Color(color.r, color.g, color.b, 0.5f);
    }

    public void Deselect()
    {
        sprite.color = new Color(color.r, color.g, color.b, color.a);
    }

    public IEnumerator AppearTile()
    {
        animationController.Play(AppearAnimName);
        yield return new WaitForSeconds(animationController.GetClip(AppearAnimName).length);
    }

    public IEnumerator DisappearTile()
    {
        animationController.Play(DisappearAnimName);
        yield return new WaitForSeconds(animationController.GetClip(DisappearAnimName).length);
        Destroy(gameObject);
    }
}
