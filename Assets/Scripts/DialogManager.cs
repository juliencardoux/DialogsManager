using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LitJson;
using UnityEngine.UI;

///-----------------------------------------------------------------
///   Class:          DialogManager
///   Description:    Simple dialog class to display dialogs from Canvas and play voice dialogs.
///   Author:         Julien Cardoux                    
///   Date:           2016/05/22
///   Licence:        OpenSource
///   Notes:          Put your Json content in the  Assets/Resources/json/dialogs.json file.
///                   Put your vocals in the Assets/Resources/Speech folder.
///                   Put your thumbnails in the Assets/Resources/Images folder.
///                   If not the case, add the LitJson dll to the project and and add it as a reference in the project.
///                   Check "debug" to see Error messages from ThrowError.
///                   Check "playVocals" to use the vocals with the text dialogs.
///-----------------------------------------------------------------

public class DialogManager : MonoBehaviour {

	private static DialogManager _instance;
	private DialogManager(){ }

	public static DialogManager Instance { get; private set; }

	public bool debug;
	public bool playVocals;
	public bool fullUITextColored;
	public bool showBackground;

	private String _jsonString;
	private JsonData _jsonData;
	private int _dialStep;

	private AudioSource _audio;

	public GameObject dialTxtCanvas;
	public GameObject dialNameCanvas;
	public GameObject imgCanvas;
	public GameObject textBackground;

	private Text _dialTxt;
	private Text _dialName;
	private Image _dialImg;
	private Sprite _neutralImg;

	[SerializeField]
	private bool _isDialoguing;
	[SerializeField]
	private string _currentId;

	public List<DialogStyle> characters;
	public List<String> waitList;

	void Awake(){
		if (Instance != null) {
			DestroyImmediate(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);

		DialogTrigger.OnDial += ThrowDialog;
	}

	void Start () {
		_audio = transform.GetComponent<AudioSource>();
		_dialTxt = dialTxtCanvas.GetComponent<Text>();
		_dialName = dialNameCanvas.GetComponent<Text>();
		_dialImg = imgCanvas.GetComponent<Image>();
		_neutralImg = _dialImg.sprite;
		_dialStep = 1;

		LoadJson ();

	}

	IEnumerator LoadJsonFromWeb(string fileURL)
	{
		WWW jsonFromWeb = new WWW(fileURL);
		yield return jsonFromWeb;

		if (!string.IsNullOrEmpty(jsonFromWeb.error)) ThrowError(6);
		else _jsonString = jsonFromWeb.text;

		try {
			_jsonData = JsonMapper.ToObject (_jsonString);
		}
		catch {
			ThrowError(0);
		}
	}

	private void LoadJson(){
		_jsonString = Resources.Load("json/dialogs").ToString();

		try {
			_jsonData = JsonMapper.ToObject (_jsonString);
		}
		catch {
			ThrowError(0);
		}
	}

	IEnumerator StartDialog(string id) {
		InitDialog (id);
		yield return null;
	}

	private void InitDialog(string dial) {

		if(!_jsonData.Keys.Contains("dial_" + dial)) {
			ThrowError(1);
			return;
		}
		var currentPath = _jsonData ["dial_" + dial] ["step" + _dialStep];
		if(!currentPath.Keys.Contains("ID") || !currentPath.Keys.Contains("TEXT") ||!currentPath.Keys.Contains("DURATION")) {
			ThrowError(2);
			return;
		}

		int dialSteps = _jsonData ["dial_" + dial].Count;
		int charaID = Int16.Parse(currentPath["ID"].ToString());

		DoDialog(charaID, currentPath["TEXT"].ToString(), "dial_" + dial + "_step" + _dialStep);
		StartCoroutine (Dialoguing(dial, currentPath["DURATION"], dialSteps));
	}

	private void DoDialog(int charaID, string text, string dialID) {
		try {
			_dialName.text = characters[charaID].name;
			if(fullUITextColored) _dialTxt.color = characters[charaID].color;
			_dialName.color = characters[charaID].color;
			_dialImg.sprite = characters[charaID].img[0];
			if(showBackground) textBackground.SetActive(true);
		}
		catch {
			ThrowError(5);
			return;
		}

		if(playVocals) SetAudio(dialID);
		_dialTxt.text = text;
	}

	IEnumerator Dialoguing(string dial, JsonData waitTime, int maxSteps) {
		_dialStep++;
		float waiter = Int16.Parse(waitTime.ToString());
		yield return new WaitForSeconds(waiter);
		if (_dialStep > maxSteps) {
			ResetUI();
			yield break;
		}
		InitDialog (dial);
	}

	private void SetAudio(string audioPath) {
		if(Resources.Load<AudioClip>("Speech/" + audioPath) == null) {
			ThrowError(3);
			return;
		}
		_audio.clip = (AudioClip)Resources.Load<AudioClip>("Speech/" + audioPath) as AudioClip;
		_audio.Play();
	}

	private void ResetUI() {
		_dialTxt.text = "";
		_dialName.text = "";
		_dialImg.sprite = _neutralImg;
		_isDialoguing = false;
		_dialStep = 1;
		_currentId = "";
		if(showBackground) textBackground.SetActive(false);
		StartCoroutine(CheckWaitList());
	}

	IEnumerator CheckWaitList() {
		yield return new WaitForSeconds(1.5f);
		if(waitList.Count <= 0) yield break;
		else {
			ThrowDialog(waitList[0]);
			waitList.RemoveAt(0);
		}
	}

	private void ThrowDialog(string id) {
		if(!_jsonData.Keys.Contains("dial_" + id)) {
			ThrowError(1);
			return;
		} else {
			if (_isDialoguing) {
				if(_currentId != id) {
					waitList.Add(id);
					_currentId = id;
				}
				return;
			}
			else {
				StartCoroutine(StartDialog(id));
				_isDialoguing = true;
				_currentId = id;
			}
		}
	}

	private void ThrowError(int error) {
		if(!debug) return;
		switch (error) {
		case 0:
			Debug.LogError("Error " + error + " : Your Json isn't a validate Json, verify your Json file.");
			break;
		case 1:
			Debug.LogWarning("Error " + error + " : There is no corresponding ID in the loaded Json, please verify the DialTrigger ID and your Json keys.");
			break;
		case 2:
			Debug.LogWarning("Error " + error + " : There is no field ID, TEXT or DURATION in your Json step.");
			break;
		case 3:
			Debug.LogWarning("Error " + error + " : There is no audio in the Ressource folder for this dialog.");
			break;
		case 4:
			Debug.LogWarning("Error " + error + " : Missing script DialTrigger on a trigger tagged as a dialog trigger.");
			break;
		case 5:
			Debug.LogWarning("Error " + error + " : No correspondance in names_data.json with the ID specified in dialogs.json.");
			break;
		case 6:
			Debug.LogWarning("Error " + error + " : Can't load Json from Web, please verify the path and make sure that dialogs.json is at the root of your WebPlayer/WebGL build.");
			break;
		}
	}

}
