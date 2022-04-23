using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] List<Sprite> walkRightSprites;

    public float moveX { get; set; }
    public float moveY { get; set; }
    public bool IsMoving { get; set; }

    SpriteAnimator walkDownAnim;
    SpriteAnimator walkUpAnim;
    SpriteAnimator walkLeftAnim;
    SpriteAnimator walkRightAnim;
    SpriteAnimator currentAnim;

    SpriteRenderer spriteRenderer;

	private void Start()
	{
        spriteRenderer = GetComponent<SpriteRenderer>();

        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);

        currentAnim = walkDownAnim;
	}

	private void Update()
	{
        var prevAnim = currentAnim;

        if (moveX == 1)
            currentAnim = walkRightAnim;
        else if (moveX == -1)
            currentAnim = walkLeftAnim;
        else if (moveY == 1)
            currentAnim = walkUpAnim;
        else if (moveY == -1)
            currentAnim = walkDownAnim;

        if (currentAnim != prevAnim)
            currentAnim.Start();

        if (IsMoving)
		    currentAnim.HandleUpdate();
        else 
            spriteRenderer.sprite = currentAnim.Frames[0];
    }


}
