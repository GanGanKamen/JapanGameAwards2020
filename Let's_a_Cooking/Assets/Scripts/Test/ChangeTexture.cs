using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Test
{
    public class ChangeTexture : MonoBehaviour
    {
        Texture2D drawTexture;
        Color[] buffer;
        Texture2D mainTexture;
        new Renderer renderer;
        void Start()
        {

        }

        public void Draw(Vector2 p)
        {
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    if ((p - new Vector2(x, y)).magnitude < 2560000000000)
                    {
                        buffer.SetValue(Color.black, x + 256 * y);
                    }
                }
            }
        }

        void Update()
        {
            {
                Vector3 originPoint = transform.position;
                Vector3 direction = transform.up;
                ///レイの長さ
                float rayLength = direction.magnitude * 100;
                //Rayが当たったオブジェクトの情報を入れる箱
                RaycastHit hit;    //原点        方向
                Ray ray = new Ray(originPoint, direction);
                //Kitchenレイヤーとレイ判定を行う
                if (Physics.Raycast(ray, out hit, rayLength) && mainTexture != null)
                {
                    Debug.Log(hit.textureCoord);
                    Draw(hit.textureCoord * 256);

                    drawTexture.SetPixels(buffer);
                    drawTexture.Apply();
                    renderer.material.mainTexture = drawTexture;
                }
                
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            renderer = other.transform.GetComponentInChildren<Renderer>();
            mainTexture = (Texture2D)other.transform.GetComponentInChildren<SkinnedMeshRenderer>().material.mainTexture;
            //Debug.Log(other.transform.GetComponentInChildren<Renderer>());
            Color[] pixels = mainTexture.GetPixels();

            buffer = new Color[pixels.Length];
            pixels.CopyTo(buffer, 0);

            drawTexture = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);
            drawTexture.filterMode = FilterMode.Point;
        }
    }
}
