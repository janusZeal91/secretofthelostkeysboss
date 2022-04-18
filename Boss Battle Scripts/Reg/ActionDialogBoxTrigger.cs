using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider2D))]
public class ActionDialogBoxTrigger : MonoBehaviour {

    public GameObject actionDialogBox;
    public GameObject player;
    public Text dialogBoxText;
    public string textForActionSpanish;
    public string textForActionEnglish;
    private Animator animator;
    private bool canDestroyDialog = false;
    private bool hasEnteredTrigger = false;

    private void Awake()
    {
        this.animator = actionDialogBox.GetComponent<Animator>();
    }

    private void Start()
    {
        SetText();
    }
    private void SetText()
    {
        this.dialogBoxText.text = PlayerPrefManager.GetLanguage() == 1 ?
            textForActionEnglish : textForActionSpanish;     
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Submit") || CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            if (canDestroyDialog)
            {
                DisableTextBox();
            }

        }


    }

    public void EnablePlayerMovement()
    {
        if (player.GetComponent<CharacterController2D>() != null)
        {
            player.GetComponent<CharacterController2D>().UnFreezeMotion(true);
        }
        else
        {
            player.GetComponent<PlayerMovementWM>().SetPlayerMovementState(true);
        }

    }

    public void DisablePlayerMovement()
    {
        if (player.GetComponent<CharacterController2D>() != null)
        {
            player.GetComponent<CharacterController2D>().FreezeMotion();
        }
        else
        {
            player.GetComponent<PlayerMovementWM>().SetPlayerMovementState(false);
        }


    }

    public void DisableTextBox()
    {

        animator.SetTrigger("Exit");
        StartCoroutine(DisableAfterAnimation());

    }

    private IEnumerator StartCoolDown()
    {
        yield return new WaitForSeconds(1.0f);
        this.canDestroyDialog = true;
    }

    private IEnumerator DisableAfterAnimation()
    {

        yield return new WaitForSeconds(0.5f);
        EnablePlayerMovement();
        actionDialogBox.SetActive(false);   
        Destroy(this.gameObject);

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player" && !hasEnteredTrigger)
        {
            hasEnteredTrigger = true;
            actionDialogBox.SetActive(true);
            DisablePlayerMovement();
            StartCoroutine(this.StartCoolDown());
        }
    }  
}
