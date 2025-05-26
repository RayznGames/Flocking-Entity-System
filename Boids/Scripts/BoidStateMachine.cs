using UnityEngine;
using UnityEngine.Events;

public class BoidStateMachine : MonoBehaviour
{
	Animator anim;
	bool coasting = false;
	[SerializeField] UnityEvent changeState;

	private void Awake()
	{
		anim = transform.GetComponent<Animator>();      
	}

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		changeState.AddListener(ChangeAnim);
	}

	// Update is called once per frame
	void Update()
	{
		DetectAngle();
	}

	void DetectAngle()
	{
		if (transform.rotation.x < 0) //Pointing Up
		{
			ChangeCoastingState(false);
		}
		else
		{
			ChangeCoastingState(true);
		}
	}

	void ChangeCoastingState(bool newState) 
	{
		if (coasting != newState)
		{
			coasting = newState;
			changeState.Invoke();
		}
	}

	public void ChangeAnim()
	{       
		 anim.SetBool("Coast", coasting); 
	}

}
