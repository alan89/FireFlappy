using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine;

public class ButtonsBehaviorScript : MonoBehaviour
{
	public void ButtonClicked(){
		Debug.Log("The button clicked was");
		//Debug.Log("The button clicked was" + eventData.selectedObject.name);
	}
}
