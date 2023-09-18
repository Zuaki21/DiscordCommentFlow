using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zuaki
{
    public class Player : MonoBehaviour
    {
        Rigidbody2D rb;
        bool isGrounded = true;
        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            //Spacebar to jump
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.AddForce(Vector2.up * 10, ForceMode2D.Impulse);
                isGrounded = false;
            }

            //左右移動Input.GetAxis("Horizontal")で-1~1の値を取得
            transform.position += new Vector3(Input.GetAxisRaw("Horizontal") * 3, 0, 0) * Time.deltaTime;

            //移動可能な範囲は画面内に制限Mathf.Clamp
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, -8f, 8f), transform.position.y, transform.position.z);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "Ground")
            {
                isGrounded = true;
            }
        }
    }
}
