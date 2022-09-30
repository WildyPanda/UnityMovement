using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private CharacterController CC;
    // la velocite du personnage
    [SerializeField]
    private Vector3 velocity = Vector3.zero;
    // la velocite du personnage
    [SerializeField]
    private Camera cam;


    // true si la gravite s'applique
    [SerializeField]
    private bool applyGravity = true;
    // true si le CC touche le sol
    [SerializeField]
    private bool isGrounded;
    // true si la marche / course est autorisé
    [SerializeField]
    private bool canWalk = true;



    // la hauteur du saut
    [SerializeField]
    private int jumpForce;
    // le nombre de saut restant
    [SerializeField]
    private float jumpLeft;
    // le nombre de saut
    [SerializeField]
    private float baseJumpLeft;
    // la puissance de la gravite
    [SerializeField]
    private int gravityPower;
    // la vitesse de base, modifie la vitesse max et l'acceleration
    [SerializeField]
    private int walkSpeed;
    // l'acceleration vers la vitesse max (0 pas de mouvement - 1 vitesse maximum instantanne) ex: 0.1 le joueur mettra 10 ticks a atteindre sa vitesse maximum
    // Eviter 0 au maximum
    [SerializeField]
    private float walkAcceleration;
    // la vitesse du ralentissement au sol (0 pour aucun arret - 1 pour arret instantanne)
    // Eviter 0 au maximum
    [SerializeField]
    private float walkStopOnGround;
    // la vitesse du ralentissement en l'air (0 pour aucun arret - 1 pour arret instantanne)
    // Eviter 0 au maximum
    [SerializeField]
    private float walkStopOnFly;
    // le multiplicateur de vitesse de la course
    [SerializeField]
    private float runSpeed;
    // le multiplicateur de vitesse de l'accroupissement
    [SerializeField]
    private float crouchSpeed;
    // le modificateur de taille en accroupi 1 pas de changement 0.5 taille divise par deux
    [SerializeField]
    private float crouchHeight;
    // la position du centre de la tete positionnne au niveau de la tete
    [SerializeField]
    private Transform headColliderCenter;
    // la position du centre de la tete positionnne au niveau de la tete
    [SerializeField]
    private float headColliderRadius;
    // true si la tete a touche a la derniere frame
    [SerializeField]
    private bool doesHeadHit;
    // sensibilite au vecteur X du regard
    [SerializeField]
    private float lookSpeedX;
    // sensibilite au vecteur Y du regard
    [SerializeField]
    private float lookSpeedY;







    void Start()
    {
        // initialise le CharacterController
        CC = this.GetComponent<CharacterController>();
        cam = this.GetComponentInChildren<Camera>();
        headColliderCenter = transform.GetChild(1).GetChild(0).GetComponent<Transform>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Methoque qui gere tout les mouvements
        Movement();

        // applique le regard au CC
        ApplyLook();
    }

    // Mouvement
    void Movement()
    {
        isGrounded = CC.isGrounded;

        // Remet a 0 le mouvement en Y si le Character touche le sol
        if (isGrounded)
        {
            velocity.y = 0;
            jumpLeft = baseJumpLeft;
            doesHeadHit = false;
        }

        if (doesHeadHit && velocity.y > 0)
        {
            velocity.y = 0;
        }

        // applique la gravite a la velocite si elle doit s'appliquer
        // on applique la gravite meme si on touche le sol afin d'avoir isGrounded en true tout le temps ou on touche le sol
        if (applyGravity) {
            ApplyGravity();
        }


        // applique le saut si le bouton saut est appuyé
        if (Input.GetButtonDown("Jump") && ( isGrounded || jumpLeft > 0 ) )
        {
            ApplyJump();
            jumpLeft -= 1;
        }

        // applique les deplacement sur les axes X et Z
        if (canWalk)
        {
            ApplyWalk();

        }

        // Move le CC avec la velocite et applique le time.deltaTime
        ApplyMovement();
    }

    void ApplyGravity()
    {
        velocity.y += Physics.gravity.y * Time.deltaTime * gravityPower;
    }

    void ApplyJump()
    {
        velocity.y = Mathf.Sqrt(jumpForce * Physics.gravity.y * -2 * gravityPower);
        
    }

    void ApplyWalk()
    {

        // ajouter l'acceleration de deplacement
        Vector3 forwardMove = transform.forward * Input.GetAxis("Vertical");
        Vector3 rightMove = transform.right * Input.GetAxis("Horizontal");
        Vector3 newWalk = (forwardMove + rightMove).normalized * walkSpeed * walkAcceleration;

        // ajoute le multiplicateur de course si la course est enclenché
        if (Input.GetButton("Crouch"))
        {
            newWalk *= crouchSpeed;
        }
        else if (Input.GetButton("Sprint"))
        {
            newWalk *= runSpeed;
        }

        if (Input.GetButtonUp("Crouch")){
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * (1 / crouchHeight), transform.localScale.z);
        }
        if (Input.GetButtonDown("Crouch"))
        {
            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y * crouchHeight, transform.localScale.z);
        }

        // recuperation d'un vector de deplacement sans le deplacement vertical
        Vector2 walk = new Vector2(velocity.x, velocity.z);
        // si la magnitude depasse la vitesse max ( (newWalk / walkAcceleration).magnitude ( la vitesse maximum du deplacement effectue au dessus ) ) appliquer le ralentissement
        if (walk.magnitude > (newWalk / walkAcceleration).magnitude)
        {
            float walkMag = walk.magnitude;
            float overMag = walk.magnitude - (newWalk / walkAcceleration).magnitude;
            float newWalkMag = newWalk.magnitude;
            // la portion du deplacement a ne pas modifier ( correspond a la marche - la portion de la marche depassant la vitesse max - la magnitude de la nouvelle direction )
            Vector2 keepWalk = walk * (1 - ( ( overMag + newWalkMag ) / walkMag));
            // la portion du deplacement a laquelle est affecte le ralentissement
            Vector2 overWalk = walk * (overMag / walkMag);

            // applique le ralentissement de overWalk selon si il est au sol ou n l'air
            if (isGrounded)
            {
                overWalk *= walkStopOnGround;
            }
            else
            {
                overWalk *= walkStopOnFly;
            }

            // si la magnitude est trop petite, l'annule
            if (overWalk.magnitude < .01f)
            {
                overWalk = Vector2.zero;
            }
            walk = keepWalk + overWalk;
        }

        // applique les deux deplacement a la velocite
        velocity.x = walk.x;
        velocity.z = walk.y;
        velocity += newWalk;
    }

    void ApplyMovement()
    {
        CC.Move(velocity * Time.deltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.collider.GetType() != typeof(TerrainCollider))
        {
            if ((hit.point - headColliderCenter.position).magnitude <= headColliderRadius && !doesHeadHit)
            {
                doesHeadHit = true;
            }
        }
    }



    // Mouvement

    // Regard
    void ApplyLook()
    {
        transform.Rotate(transform.up, lookSpeedX * Input.GetAxis("Mouse X"));
        cam.transform.Rotate(new Vector3(1,0,0), -lookSpeedY * Input.GetAxis("Mouse Y"));
    }

    // Regard

}
