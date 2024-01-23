using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergeBattlefieldShadows : MonoBehaviour
{

    public SpriteRenderer spriteRenderer;// assumes you've dragged a reference into this
    public Transform mergeInput;// a transform with a bunch of SpriteRenderers you want to merge

    [SerializeField] private int pixelsPerUnit;
    [SerializeField] private float pivotX;
    [SerializeField] private float pivotY;

    // Use this for initialization
    void Start() {
        spriteRenderer.sprite = Create(new Vector2Int(4096, 2048), mergeInput);
    }

    /* Takes a transform holding many sprites as input and creates one flattened sprite out of them */
    public Sprite Create(Vector2Int size, Transform input) {
        var spriteRenderers = input.GetComponentsInChildren<SpriteRenderer>();
        if (spriteRenderers.Length == 0) {
            Debug.Log("No SpriteRenderers found in " + input.name + " for SpriteMerge");
            return null;
        }

        var targetTexture = new Texture2D(size.x, size.y, TextureFormat.RGBA32, false, false);
        targetTexture.filterMode = FilterMode.Point;
        var targetPixels = targetTexture.GetPixels();
        for (int i = 0; i < targetPixels.Length; i++) targetPixels[i] = Color.clear;// default pixels are not set
        var targetWidth = targetTexture.width;

        for (int i = 0; i < spriteRenderers.Length; i++) {
            var sr = spriteRenderers[i];

            Debug.Log(sr);

            var position = (Vector2)sr.transform.localPosition - sr.sprite.pivot;

            Debug.Log(position);
            var p = new Vector2Int((int)position.x, (int)position.y);
            var sourceWidth = sr.sprite.texture.width;
            // if read/write is not enabled on texture (under Advanced) then this next line throws an error
            // no way to check this without Try/Catch :(
            var sourcePixels = sr.sprite.texture.GetPixels();
            for (int j = 0; j < sourcePixels.Length; j++) {
                var source = sourcePixels[j];
                var x = (j % sourceWidth) + p.x * pixelsPerUnit;
                var y = (j / sourceWidth) + p.y * pixelsPerUnit;
                var index = x + y * targetWidth;
                if (index > 0 && index < targetPixels.Length) {
                    var target = targetPixels[index];
                    if (target.a > 0) {
                        // alpha blend when we've already written to the target
                        float sourceAlpha = source.a;
                        float invSourceAlpha = 1f - source.a;
                        float alpha = sourceAlpha + invSourceAlpha * target.a;
                        Color result = (source * sourceAlpha + target * target.a * invSourceAlpha) / alpha;
                        result.a = alpha;
                        source = result;
                    }
                    targetPixels[index] = source;
                }
            }
        }

        targetTexture.SetPixels(targetPixels);
        targetTexture.Apply(false, true);// read/write is disabled in 2nd param to free up memory
        return Sprite.Create(targetTexture, new Rect(new Vector2(), size), new Vector2(pivotX,pivotY), pixelsPerUnit, 0, SpriteMeshType.FullRect);
    }
}
