using UnityEngine;
using System.Collections;

public class DialogTrigger : MonoBehaviour {

	///-----------------------------------------------------------------
	///   Class:          DialogTrigger
	///   Description:    Dialog Trigger which deal with events triggered by physical triggers. 
	///   Author:         Julien Cardoux                    
	///   Date:           2016/05/23
	///   Licence:        OpenSource
	///   Notes:          Use your dialogs.json keys in the "id" string to link dialogs.
	///                   If "singleUse" is activated, the trigger will play the dialog only one time.
	///-----------------------------------------------------------------

	public string id;
	public bool singleUse;
	public bool used;

	public delegate void OnDialogEvent(string id);
	public static event OnDialogEvent OnDial;

	void OnTriggerEnter(Collider col) {
		if(col.gameObject.tag != "Player") return;
		if(singleUse && used) return;
		used = true;
		OnDial(id);
	}
}
