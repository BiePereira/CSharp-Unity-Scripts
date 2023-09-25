using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlappyController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    public float jumpForce;

    public int life;
    public bool alive;

    public bool isTriggered;

    public Text healthCountText;
    public Camera mainCamera;
    private Animator camAnimator;

    public AudioSource ASWing;
    public AudioSource ASQuero;

    public bool godMode;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        camAnimator = mainCamera.GetComponent<Animator>();

        alive = true;
        healthCountText.text = life.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump")) && alive)
        {
            animator.Play("Flap", -1);
            ASWing.Play();

            
            rb.velocity = Vector2.zero;
            rb.AddForce(new Vector2(0, jumpForce));
        }

        if(life == 0 && alive)
        {
            alive = false;
            StartCoroutine(QueroQueroFall());
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")
            && !collision.GetComponent<EnemyFlappyController>().triggered
            && !godMode
            && alive)
        {
            //necessário para evitar duas colisões com o mesmo pássaro
            //collision.GetComponent<Rigidbody2D>().Sleep();
            collision.GetComponent<EnemyFlappyController>().triggered = true;
            print(collision.name + " hits Quero-Quero and damaged");

            TakeDamage(1);
        }

        if (collision.CompareTag("GameOver"))
        {
            StartCoroutine(QueroQueroFell());
        }
    }

    private IEnumerator QueroQueroFall()
    {
        rb.velocity = Vector2.zero;
        rb.gravityScale = 1.5f;
        yield return new WaitForSeconds(2f);
        Faint();
    }

    private IEnumerator QueroQueroFell()
    {
        //must inform the "dead" state before the faint method so the plane can fly away on StageController
        alive = false;
        yield return new WaitForSeconds(1f);
        Faint();
    }

    private void TakeDamage(int damage)
    {
        life -= damage;
        healthCountText.text = life.ToString();
        camAnimator.Play("TakeDamage", -1);
        animator.Play("TakeDamage", -1);

        ASQuero.Play();
    }

    public void Faint()
    {
        life = 0;
        healthCountText.text = life.ToString();
        alive = false;
    }
}
