using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public delegate void OnStateChangeHandler ();

	public class UIManager : MonoBehaviour
	{
		static UIManager _instance;

		public event OnStateChangeHandler OnStateChange;

		public enum State
		{
			MainMenu ,
			CharacterSelect
		};

		State currState;

		Transform currentActiveCharacter;

		public GameObject mainMenu;
		public GameObject characterSelect;

		//Creating a singleton for UI manager.
		public static UIManager instance {
			get {
				if (_instance == null) {
					_instance = GameObject.FindObjectOfType<UIManager> ();
				
					//Tell unity not to destroy this object when loading a new scene!
					if (_instance != null)
						DontDestroyOnLoad (_instance.gameObject);
				}
				return _instance;
			}
		}
	
		void Awake ()
		{
			if (_instance == null) {
				_instance = this;
				DontDestroyOnLoad (this);
			} else {
				//If a Singleton already exists and you find
				//another reference in scene, destroy it!
				if (this != _instance)
					Destroy (this.gameObject);
			}
		}

		void Start ()
		{
			spawnInitCharacter ();

			OnStateChange += HandleStateChange;

			//Set initial ui state
			HandleStateChange ();
		}

		void spawnInitCharacter(){
			GameObject go = Instantiate (Resources.Load ("Prefabs/Players/Cube3")) as GameObject;
			currentActiveCharacter = go.transform;
			currentActiveCharacter.SetParent(transform);
			currentActiveCharacter.localScale = Vector3.one;
			currentActiveCharacter.localPosition = Vector3.zero;
			currentActiveCharacter.localRotation = Quaternion.identity;
			currentActiveCharacter.name = "Character 3";
		}

		void HandleStateChange ()
		{
			mainMenu.SetActive (CurrState == State.MainMenu);
			characterSelect.SetActive (CurrState == State.CharacterSelect);
		}

		public State CurrState {
			get {
				return currState;
			}
			set {
				currState = value;
			
				if (OnStateChange != null)
					OnStateChange ();
			}
		}

		public Transform CurrentActiveCharacter {
			get {
				return currentActiveCharacter;
			}
			set {
				if(currentActiveCharacter !=null)
					Destroy(currentActiveCharacter.gameObject);

				currentActiveCharacter = value;
			}
		}
	}
}